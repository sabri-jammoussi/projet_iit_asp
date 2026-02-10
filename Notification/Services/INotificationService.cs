using Notification.DTOs;
using Notification.Models;

namespace Notification.Services;

/// <summary>
/// Service interface for Notification operations.
/// </summary>
public interface INotificationService
{
    Task<NotificationDto> CreateAsync(CreateNotificationDto dto);
    Task<IList<NotificationDto>> GetAllAsync();
    Task<IList<NotificationDto>> GetByCustomerIdAsync(int customerId);
    Task<IList<NotificationDto>> GetUnreadAsync(int? customerId = null);
    Task<bool> MarkAsReadAsync(int id);
    Task<bool> MarkAllAsReadAsync(int customerId);
}
