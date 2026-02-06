using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Data.Infrastructure.EF.Models;

[Table("ORDERS")]
public class OrderDao
{
    [Key]
    [Column("O_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("O_ORDER_DATE")]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("O_TOTAL_AMOUNT", TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Required]
    [StringLength(20)]
    [Column("O_STATUS")]
    public string Status { get; set; } = "Pending";

    [StringLength(200)]
    [Column("O_SHIPPING_ADDRESS")]
    public string? ShippingAddress { get; set; }

    // Foreign key to Customer
    [Required]
    [Column("O_CUSTOMER_ID")]
    public int CustomerId { get; set; }

    // Navigation property: Order -> Customer (Many-to-One)
    [ForeignKey(nameof(CustomerId))]
    public virtual CustomerDao Customer { get; set; } = null!;

    // Navigation property: One Order -> Many OrderDetails
    public virtual ICollection<OrderDetailDao> OrderDetails { get; set; } = new List<OrderDetailDao>();
}
