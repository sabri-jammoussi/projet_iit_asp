using System.ComponentModel.DataAnnotations;
using Back.Data.Infrastructure.EF.Enums;

namespace back.Models.Authentification;

public class RegistrationRequest
{


    [Required]
    public required string FirstName { get; set; }
    [Required]

    public required string LastName { get; set; }
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }

    public UserRole Role { get; set; }
}
