using Back.Data.Infrastructure.EF;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Notification.Hubs;

namespace Notification.Jobs;

/// <summary>
/// Hangfire background job for processing notifications.
/// </summary>
public class NotificationJob
{
    private readonly OltpDbContext _dbContext;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationJob> _logger;

    public NotificationJob(
		OltpDbContext dbContext,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationJob> logger)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Process pending notifications (can be scheduled via Hangfire).
    /// </summary>
    [Queue("010_notif")]
    public async Task ProcessPendingNotificationsAsync()
    {
        _logger.LogInformation("Processing pending notifications...");

        var pendingNotifications = _dbContext.Notifications
            .Where(n => n.SentAt == null)
            .ToList();

        foreach (var notification in pendingNotifications)
        {
            try
            {
                if (notification.CustomerId.HasValue)
                {
                    await _hubContext.Clients
                        .Group($"user_{notification.CustomerId}")
                        .SendAsync("ReceiveNotification", notification);
                }

                await _hubContext.Clients
                    .Group("admins")
                    .SendAsync("ReceiveNotification", notification);

                notification.SentAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Notification {Id} sent successfully", notification.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification {Id}", notification.Id);
            }
        }

        _logger.LogInformation("Finished processing {Count} notifications", pendingNotifications.Count);
    }

    /// <summary>
    /// Clean up old read notifications (scheduled job).
    /// </summary>
    [Queue("default")]
    public async Task CleanupOldNotificationsAsync(int daysOld = 30)
    {
        _logger.LogInformation("Cleaning up notifications older than {Days} days...", daysOld);

        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var oldNotifications = _dbContext.Notifications
            .Where(n => n.IsRead && n.CreatedAt < cutoffDate)
            .ToList();

        _dbContext.Notifications.RemoveRange(oldNotifications);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted {Count} old notifications", oldNotifications.Count);
    }
}
