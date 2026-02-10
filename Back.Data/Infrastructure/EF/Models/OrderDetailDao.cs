using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Data.Infrastructure.EF.Models;

[Table("ORDER_DETAIL")]
public class OrderDetailDao
{
    [Key]
    [Column("OD_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("OD_QUANTITY")]
    public int Quantity { get; set; }

    [Required]
    [Column("OD_UNIT_PRICE", TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Required]
    [Column("OD_LINE_TOTAL", TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }

    // Foreign key to Order
    [Required]
    [Column("O_ID")]
    public int OrderId { get; set; }

    // Navigation property: OrderDetail -> Order (Many-to-One)
    [ForeignKey(nameof(OrderId))]
    public virtual OrderDao Order { get; set; } = null!;

    // Foreign key to Product
    [Required]
    [Column("OD_PRODUCT_ID")]
    public int ProductId { get; set; }

    // Navigation property: OrderDetail -> Product (Many-to-One)
    [ForeignKey(nameof(ProductId))]
    public virtual ProductDao Product { get; set; } = null!;
}
