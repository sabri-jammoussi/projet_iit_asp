using Back.Data.Infrastructure.EF;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace back.Jobs;

/// <summary>
/// Hangfire background jobs for notification-related tasks.
/// Centralized in the Back API project.
/// </summary>
public class NotificationJobs
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationJobs> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public NotificationJobs(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationJobs> logger,
        IHttpClientFactory httpClientFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Process pending notifications that haven't been sent via SignalR.
    /// Called by Hangfire recurring job.
    /// </summary>
    [Queue("default")]
    public async Task ProcessPendingNotificationsAsync()
    {
        _logger.LogInformation("Processing pending notifications...");

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OltpDbContext>();

        var pendingNotifications = await dbContext.Notifications
            .Where(n => n.SentAt == null)
            .ToListAsync();

        if (pendingNotifications.Count == 0)
        {
            _logger.LogInformation("No pending notifications to process");
            return;
        }

        var client = _httpClientFactory.CreateClient("NotificationService");

        foreach (var notification in pendingNotifications)
        {
            try
            {
                // Call the Notification service to resend via SignalR
                var payload = new
                {
                    Type = notification.Type,
                    Title = notification.Title,
                    Message = notification.Message,
                    OrderId = notification.OrderId,
                    CustomerId = notification.CustomerId,
                    CustomerEmail = notification.CustomerEmail,
                    CreatedAt = notification.CreatedAt
                };

                var response = await client.PostAsJsonAsync("/api/nf/notifications/resend", payload);

                if (response.IsSuccessStatusCode)
                {
                    notification.SentAt = DateTime.UtcNow;
                    _logger.LogInformation("Notification {Id} sent successfully", notification.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to send notification {Id}. Status: {Status}", notification.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification {Id}", notification.Id);
            }
        }

        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Finished processing {Count} notifications", pendingNotifications.Count);
    }

    /// <summary>
    /// Clean up old read notifications.
    /// Called by Hangfire recurring job (daily).
    /// </summary>
    [Queue("default")]
    public async Task CleanupOldNotificationsAsync(int daysOld = 30)
    {
        _logger.LogInformation("Cleaning up notifications older than {Days} days...", daysOld);

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OltpDbContext>();

        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var oldNotifications = await dbContext.Notifications
            .Where(n => n.IsRead && n.CreatedAt < cutoffDate)
            .ToListAsync();

        if (oldNotifications.Count == 0)
        {
            _logger.LogInformation("No old notifications to clean up");
            return;
        }

        dbContext.Notifications.RemoveRange(oldNotifications);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted {Count} old notifications", oldNotifications.Count);
    }
}
