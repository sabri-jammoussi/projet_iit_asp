using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Back.Data.Infrastructure.EF.Enums;

namespace Back.Data.Infrastructure.EF.Models;

[Table("ACCOUNT")]
public class AccountDao
{
    [Key]
    [Column("A_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    [Column("A_FIRSTNAME")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    [Column("A_LASTNAME")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    [Column("A_EMAIL")]
    public string Email { get; set; } = string.Empty;

    [Column("A_PASSWORD_HASH")]
    public byte[]? PasswordHash { get; set; }

    [Column("A_PASSWORD_SALT")]
    public byte[]? PasswordSalt { get; set; }

    [EnumDataType(typeof(UserRole))]
    [Column("A_ROLE")]
    public UserRole Role { get; set; }

    // Navigation property: One Account -> One Customer (only for Client role)
    public virtual CustomerDao? Customer { get; set; }
}
