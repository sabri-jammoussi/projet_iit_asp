using System.Reflection;
using Back.Data.Infrastructure.EF;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Notification.Hubs;
using Notification.Jobs;
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
builder.Services.AddSwaggerGen();

// Entity Framework - Notification DbContext
builder.Services.AddDbContext<OltpDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("UsersConnections"));
});

// Hangfire
builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("UsersConnections"),
        new SqlServerStorageOptions
        {
            PrepareSchemaIfNecessary = true,
            QueuePollInterval = TimeSpan.FromMilliseconds(200)
        });
});

builder.Services.AddHangfireServer(options =>
{
    options.ServerName = "NOTIF-SERVER";
    options.Queues = new[] { "default", "010_notif" };
});

// Services
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<NotificationJob>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DisplayOperationId();
        options.DisplayRequestDuration();
    });
}

app.UseRouting();

app.MapControllers();

// WebSockets for SignalR
app.UseWebSockets();
app.MapHub<NotificationHub>("/hubs/notif");

// Hangfire Dashboard
app.UseHangfireDashboard(pathMatch: "/hf", options: new DashboardOptions
{
    DashboardTitle = "NOTIFICATION-JOBS",
    DisplayStorageConnectionString = false,
    StatsPollingInterval = 1000
});

// Schedule recurring jobs
RecurringJob.AddOrUpdate<NotificationJob>(
    "process-pending-notifications",
    job => job.ProcessPendingNotificationsAsync(),
    Cron.Minutely);

RecurringJob.AddOrUpdate<NotificationJob>(
    "cleanup-old-notifications",
    job => job.CleanupOldNotificationsAsync(30),
    Cron.Daily);

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
