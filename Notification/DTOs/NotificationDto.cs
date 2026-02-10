namespace Notification.DTOs;

/// <summary>
/// DTO for creating a notification.
/// </summary>
public class CreateNotificationDto
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerEmail { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO for returning notification data.
/// </summary>
public class NotificationDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerEmail { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
}
