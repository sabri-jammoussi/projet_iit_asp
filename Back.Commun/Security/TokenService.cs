using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Back.Data.Infrastructure.EF.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Back.Commun.Security;

public class TokenService
{
    private const int ExpirationMinutes = 150;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string CreateToken(AccountDao user)
    {
        ValidateConfiguration();
        var expiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
        var token = CreateJwtToken(
            CreateClaims(user),
            CreateSigningCredentials(),
            expiration
        );
        var tokenHandler = new JwtSecurityTokenHandler();

        _logger.LogInformation("JWT Token created");
        return tokenHandler.WriteToken(token);
    }

    private void ValidateConfiguration()
    {
        var requiredSettings = new[]
        {
            "JwtTokenSettings:ValidIssuer",
            "JwtTokenSettings:ValidAudience",
            "JwtTokenSettings:SymmetricSecurityKey"
        };

        foreach (var setting in requiredSettings)
        {
            if (string.IsNullOrEmpty(_configuration[setting]))
            {
                throw new InvalidOperationException($"Configuration setting '{setting}' is missing.");
            }
        }
    }

    private JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials, DateTime expiration)
    {
        var validIssuer = _configuration["JwtTokenSettings:ValidIssuer"];
        var validAudience = _configuration["JwtTokenSettings:ValidAudience"];

        return new JwtSecurityToken(
            issuer: validIssuer,
            audience: validAudience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );
    }

    private List<Claim> CreateClaims(AccountDao user)
    {
        var jwtSub = _configuration["JwtTokenSettings:JwtRegisteredClaimNamesSub"] ?? string.Empty;

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        return claims;
    }

    private SigningCredentials CreateSigningCredentials()
    {
        var symmetricSecurityKey = _configuration["JwtTokenSettings:SymmetricSecurityKey"] ?? string.Empty;

        return new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(symmetricSecurityKey)),
            SecurityAlgorithms.HmacSha256
        );
    }
}
