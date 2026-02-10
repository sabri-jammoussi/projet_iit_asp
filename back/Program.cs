using System.Text;
using back.Repositories.Customer;
using back.Repositories.Order;
using back.Repositories.OrderDetail;
using back.Repositories.Product;
using back.Services;
using back.Services.Customers;
using Back.Commun.Security;
using Back.Commun.Account;
using Back.Data.Infrastructure.EF;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using back.Services.Orders;
using Hangfire;
using Hangfire.SqlServer;

namespace back;

public class Program
{
    public static void Main(string[] args)
    {
        // Enable PII logging for debugging (disable in production!)
        IdentityModelEventSource.ShowPII = true;

        var builder = WebApplication.CreateBuilder(args);

        // JWT Configuration
        var validIssuer = builder.Configuration.GetValue<string>("JwtTokenSettings:ValidIssuer");
        var validAudience = builder.Configuration.GetValue<string>("JwtTokenSettings:ValidAudience");
        var symmetricSecurityKey = builder.Configuration.GetValue<string>("JwtTokenSettings:SymmetricSecurityKey");

        if (string.IsNullOrWhiteSpace(symmetricSecurityKey))
        {
            throw new InvalidOperationException("Configuration manquante: 'JwtTokenSettings:SymmetricSecurityKey' est vide.");
        }

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // Swagger with JWT Bearer authentication
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Sales Analysis Platform API",
                Version = "v1",
                Description = "API Backend pour la plateforme d'analyse des ventes"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = "Bearer",
                Description = "JWT Authorization header using the Bearer scheme.\r\n\r\nEnter 'Bearer' [space] and then your token.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new List<string>()
                }
            });
        });

        // Database
        builder.Services.AddDbContext<OltpDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("UsersConnections")));

        // Hangfire (background jobs)
        var hangfireConn = builder.Configuration.GetConnectionString("UsersConnections") ?? builder.Configuration.GetConnectionString("UsersConnections");
        builder.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(hangfireConn, new SqlServerStorageOptions
            {
                PrepareSchemaIfNecessary = true,
                QueuePollInterval = TimeSpan.Zero,
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

        // Add the Hangfire server to process jobs
        builder.Services.AddHangfireServer();

        // JWT Authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.IncludeErrorDetails = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.FromMinutes(5),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = validIssuer,
                ValidAudience = validAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(symmetricSecurityKey))
            };

            // Log authentication failures for debugging
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = context.Request.Headers["Authorization"].FirstOrDefault();
                   // Console.WriteLine($"Token received: {token}");
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddAuthorization();

        // HttpClient for Notification service
        builder.Services.AddHttpClient("NotificationService", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5555");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Security services
        builder.Services.AddScoped<TokenService>();
        builder.Services.AddScoped<PasswordHasherProvider>();
        builder.Services.AddScoped<EmailValidator>();
        builder.Services.AddScoped<PasswordValidator>();

        // Auth service
        builder.Services.AddScoped<AuthService>();

        // Repositories
        builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();

        // Services
        builder.Services.AddScoped<ICustomerService, CustomerService>();
        builder.Services.AddScoped<IOrderService, OrderService>();

        builder.Services.AddHttpContextAccessor();
        // Current user provider (uses IHttpContextAccessor internally)
        builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
        // Keyed Hangfire storage and client for 'notif' queue
        builder.Services.AddSingleton<JobStorage>(sp =>
            new SqlServerStorage(hangfireConn, new SqlServerStorageOptions
            {
                PrepareSchemaIfNecessary = true,
                QueuePollInterval = TimeSpan.FromMilliseconds(200)
            }));

        builder.Services.AddSingleton<IBackgroundJobClient>(sp =>
            new BackgroundJobClient(sp.GetRequiredService<JobStorage>()));
		var app = builder.Build();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // Auto-migrate database
        try
        {
            using var scope = app.Services.CreateScope();
            using var dbContext = scope.ServiceProvider.GetService<OltpDbContext>();
            dbContext?.Database.Migrate();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        app.Run();
    }
}
