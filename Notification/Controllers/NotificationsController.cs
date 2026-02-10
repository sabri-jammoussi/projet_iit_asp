using Microsoft.AspNetCore.Mvc;
using Notification.DTOs;
using Notification.Services;

namespace Notification.Controllers;

/// <summary>
/// API Controller for Notifications.
/// </summary>
[ApiController]
[Route("api/nf/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new notification (called by back API).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<NotificationDto>> Create([FromBody] CreateNotificationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _notificationService.CreateAsync(dto);
            _logger.LogInformation("Notification created with ID {Id}", created.Id);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create notification");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all notifications.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IList<NotificationDto>>> GetAll()
    {
        var notifications = await _notificationService.GetAllAsync();
        return Ok(notifications);
    }

    /// <summary>
    /// Get notifications by customer ID.
    /// </summary>
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IList<NotificationDto>>> GetByCustomerId(int customerId)
    {
        var notifications = await _notificationService.GetByCustomerIdAsync(customerId);
        return Ok(notifications);
    }

    /// <summary>
    /// Get unread notifications.
    /// </summary>
    [HttpGet("unread")]
    public async Task<ActionResult<IList<NotificationDto>>> GetUnread([FromQuery] int? customerId)
    {
        var notifications = await _notificationService.GetUnreadAsync(customerId);
        return Ok(notifications);
    }

    /// <summary>
    /// Mark notification as read.
    /// </summary>
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var result = await _notificationService.MarkAsReadAsync(id);
        if (!result)
        {
            return NotFound(new { message = $"Notification with ID {id} not found." });
        }
        return NoContent();
    }

    /// <summary>
    /// Mark all notifications as read for a customer.
    /// </summary>
    [HttpPatch("customer/{customerId}/read-all")]
    public async Task<IActionResult> MarkAllAsRead(int customerId)
    {
        await _notificationService.MarkAllAsReadAsync(customerId);
        return NoContent();
    }
}
