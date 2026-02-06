using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Data.Infrastructure.EF.Models;

[Table("PRODUCT")]
public class ProductDao
{
    [Key]
    [Column("P_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Column("P_NAME")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Column("P_DESCRIPTION")]
    public string? Description { get; set; }

    [Required]
    [Column("P_PRICE", TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    [Column("P_STOCK")]
    public int Stock { get; set; }

    [StringLength(50)]
    [Column("P_CATEGORY")]
    public string? Category { get; set; }

    [Column("P_CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property: One Product -> Many OrderDetails
    public virtual ICollection<OrderDetailDao> OrderDetails { get; set; } = new List<OrderDetailDao>();
}
