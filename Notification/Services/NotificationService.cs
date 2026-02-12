using Back.Data.Infrastructure.EF;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Notification.DTOs;
using Notification.Hubs;
using Notification.Models;

namespace Notification.Services;
public class NotificationService : INotificationService
{
    private readonly OltpDbContext _dbContext;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        OltpDbContext dbContext,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<NotificationDto> CreateAsync(CreateNotificationDto dto)
    {
        var notification = new NotificationDao
        {
            Type = dto.Type,
            Title = dto.Title,
            Message = dto.Message,
            OrderId = dto.OrderId,
            CustomerId = dto.CustomerId,
            CustomerEmail = dto.CustomerEmail,
            CreatedAt = dto.CreatedAt,
            IsRead = false
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Notification created with ID {Id} for Order {OrderId}", notification.Id, notification.OrderId);

        // Send real-time notification via SignalR
        await SendSignalRNotificationAsync(notification);

        return MapToDto(notification);
    }

    public async Task<IList<NotificationDto>> GetAllAsync()
    {
        var notifications = await _dbContext.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(MapToDto).ToList();
    }

    public async Task<IList<NotificationDto>> GetByCustomerIdAsync(int customerId)
    {
        var notifications = await _dbContext.Notifications
            .Where(n => n.CustomerId == customerId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(MapToDto).ToList();
    }

    public async Task<IList<NotificationDto>> GetUnreadAsync(int? customerId = null)
    {
        var query = _dbContext.Notifications.Where(n => !n.IsRead);

        if (customerId.HasValue)
        {
            query = query.Where(n => n.CustomerId == customerId);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(MapToDto).ToList();
    }

    public async Task<bool> MarkAsReadAsync(int id)
    {
        var notification = await _dbContext.Notifications.FindAsync(id);
        if (notification == null)
        {
            return false;
        }

        notification.IsRead = true;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Notification {Id} marked as read", id);
        return true;
    }
    public async Task<int> GetUnreadCountAsync()
    {
        var res = await _dbContext.Notifications
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .CountAsync(x => !x.IsRead);
        return res;
    }


	private async Task SendSignalRNotificationAsync(NotificationDao notification)
    {
        try
        {
            var notificationDto = MapToDto(notification);
            notificationDto.SentAt = DateTime.UtcNow;

            // Send to specific customer if exists
            if (notification.CustomerId.HasValue)
            {
                await _hubContext.Clients
                    .Group($"user_{notification.CustomerId}")
                    .SendAsync("ReceiveNotification", notificationDto);

                _logger.LogInformation("Notification sent via SignalR to user_{CustomerId}", notification.CustomerId);
            }

            // Also send to admins group
            await _hubContext.Clients
                .Group("admins")
                .SendAsync("ReceiveNotification", notificationDto);

            // Update sent timestamp
            notification.SentAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SignalR notification for ID {Id}", notification.Id);
        }
    }

    private static NotificationDto MapToDto(NotificationDao notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            OrderId = notification.OrderId,
            CustomerId = notification.CustomerId,
            CustomerEmail = notification.CustomerEmail,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            SentAt = notification.SentAt
        };
    }
}
