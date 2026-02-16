using System.Reflection;
using System.Text;
using Back.Data.Infrastructure.EF;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Notification.Hubs;
using Notification.Services;
using Serilog;

var name = "NOTIFICATION-SERVICE";

var builder = WebApplication.CreateBuilder(args);

// --- Logging ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

var logger = Log.Logger.ForContext<Program>();

// Read app version
var appVersion = Assembly.GetEntryAssembly()
    ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
    ?.InformationalVersion ?? "1.0.0";

logger.Information($"[{name}] v{appVersion}");
if (Environment.UserInteractive)
{
    Console.Title = $"{name} v{appVersion}";
    logger.Verbose("Running on user interactive mode");
}
else
{
    logger.Verbose("Running as windows service mode");
}

// Controllers
builder.Services.AddControllers();

// SignalR
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = null;
    options.EnableDetailedErrors = true;
});
// JWT Configuration
var validIssuer = builder.Configuration.GetValue<string>("JwtTokenSettings:ValidIssuer");
var validAudience = builder.Configuration.GetValue<string>("JwtTokenSettings:ValidAudience");
var symmetricSecurityKey = builder.Configuration.GetValue<string>("JwtTokenSettings:SymmetricSecurityKey");

if (string.IsNullOrWhiteSpace(symmetricSecurityKey))
{
	throw new InvalidOperationException("Configuration manquante: 'JwtTokenSettings:SymmetricSecurityKey' est vide.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
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

		options.SaveToken = true;

		// CRITICAL: This is the fix for SignalR
		options.Events = new JwtBearerEvents
		{
			OnMessageReceived = context =>
			{
				// First try to get token from Authorization header (regular HTTP requests)
				var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();

				if (!string.IsNullOrEmpty(authorizationHeader) &&
					authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
				{
					context.Token = authorizationHeader.Substring("Bearer ".Length).Trim();
					Console.WriteLine("Token found in Authorization header");
					return Task.CompletedTask;
				}

				// For SignalR connections, get token from query string
				var path = context.HttpContext.Request.Path;
				if (path.StartsWithSegments("/hubs"))
				{
					var accessToken = context.Request.Query["access_token"].FirstOrDefault();

					if (!string.IsNullOrEmpty(accessToken))
					{
						context.Token = accessToken;
						Console.WriteLine("Token found in query string for SignalR");
					}
					else
					{
						Console.WriteLine("No token found in query string for SignalR path");
					}
				}

				return Task.CompletedTask;
			},
			OnAuthenticationFailed = context =>
			{
				Console.WriteLine($"Token validation failed: {context.Exception.Message}");
				Console.WriteLine($"Path: {context.HttpContext.Request.Path}");
				//Console.WriteLine($"Token present: {context.Token != null}");
				return Task.CompletedTask;
			},
			OnTokenValidated = context =>
			{
				Console.WriteLine("Token successfully validated");
				return Task.CompletedTask;
			},
			OnChallenge = context =>
			{
				Console.WriteLine($"Challenge triggered: {context.Error}, {context.ErrorDescription}");
				return Task.CompletedTask;
			}
		};
	});// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notification API",
        Version = "v1",
        Description = "Notification service - API + SignalR"
    });
});

// Entity Framework - DbContext
builder.Services.AddDbContext<OltpDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("UsersConnections"));
});

// Services
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Swagger (enabled in all environments for testing)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification API v1");
    options.DisplayOperationId();
    options.DisplayRequestDuration();
});

app.UseRouting();

// Authentication/Authorization (ready for future use)
app.UseAuthentication();
app.UseAuthorization();

// WebSockets for SignalR
app.UseWebSockets();

// Map controllers and SignalR hub
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notif");

// Auto-migrate database
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OltpDbContext>();
    dbContext.Database.Migrate();
    logger.Information("Database migrated successfully");
}
catch (Exception ex)
{
    logger.Error(ex, "Error migrating database");
}

logger.Information($"[{name}] started on port 5555");

app.Run();
