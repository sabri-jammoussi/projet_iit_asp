using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Data.Infrastructure.EF.Models;

[Table("CUSTOMER")]
public class CustomerDao
{
    [Key]
    [Column("C_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    // FK to Account (one-to-one)
    [Required]
    [Column("A_ID")]
    public int AccountId { get; set; }

    [Required]
    [StringLength(50)]
    [Column("C_FIRSTNAME")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Column("C_LASTNAME")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Column("C_EMAIL")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    [Column("C_PHONE")]
    public string? Phone { get; set; }

    [StringLength(200)]
    [Column("C_ADDRESS")]
    public string? Address { get; set; }

    [StringLength(50)]
    [Column("C_CITY")]
    public string? City { get; set; }

    [StringLength(50)]
    [Column("C_COUNTRY")]
    public string? Country { get; set; }

    [Column("C_CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property: One Customer -> Many Orders
    public virtual ICollection<OrderDao> Orders { get; set; } = new List<OrderDao>();

	[ForeignKey(nameof(AccountId))]
	public virtual AccountDao? Account { get; set; }
}
