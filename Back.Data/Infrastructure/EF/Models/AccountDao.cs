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

    [StringLength(30)]
    [Column("A_FIRSTNAME")]
    public required string FirstName { get; set; }

    [StringLength(30)]
    [Column("A_LASTNAME")]
    public required string LastName { get; set; }

    [StringLength(30)]
    [Column("A_EMAIL")]
    public required string Email { get; set; }

    [Column("A_PASSWORD_HASH")]
    public byte[]? PasswordHash { get; set; }

    [Column("A_PASSWORD_SALT")]
    public byte[]? PasswordSalt { get; set; }

    [EnumDataType(typeof(UserRole))]
    [Column("A_ROLE")]
    public UserRole Role { get; set; }
}
