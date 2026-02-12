using System.Reflection;
using Back.Data.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
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

// Swagger
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
