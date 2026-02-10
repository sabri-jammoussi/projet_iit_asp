using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notification.Models;

/// <summary>
/// Notification entity stored in database.
/// </summary>
[Table("NOTIFICATION")]
public class NotificationDao
{
    [Key]
    [Column("N_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Column("N_TYPE")]
    public string Type { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Column("N_TITLE")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    [Column("N_MESSAGE")]
    public string Message { get; set; } = string.Empty;

    [Column("N_ORDER_ID")]
    public int? OrderId { get; set; }

    [Column("N_CUSTOMER_ID")]
    public int? CustomerId { get; set; }

    [StringLength(100)]
    [Column("N_CUSTOMER_EMAIL")]
    public string? CustomerEmail { get; set; }

    [Column("N_IS_READ")]
    public bool IsRead { get; set; } = false;

    [Column("N_CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("N_SENT_AT")]
    public DateTime? SentAt { get; set; }
}
