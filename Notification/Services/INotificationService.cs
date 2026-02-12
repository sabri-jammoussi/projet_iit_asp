using Notification.DTOs;

namespace Notification.Services;

public interface INotificationService
{
    Task<NotificationDto> CreateAsync(CreateNotificationDto dto);
    Task<IList<NotificationDto>> GetAllAsync();
    Task<bool> MarkAsReadAsync(int id);
    Task<int> GetUnreadCountAsync();
}
