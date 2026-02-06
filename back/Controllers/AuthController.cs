using back.Models.Authentification;
using back.Services;
using Back.Commun.Security;
using Back.Data.Infrastructure.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace back.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly AuthService _authSevice;

    public AuthController(ILogger<AuthController> logger, AuthService authSevice)
    {
        _logger = logger;
        _authSevice = authSevice;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [SwaggerOperation(Tags = ["[Authentication]"])]

    public async Task<IActionResult> Register(RegistrationRequest request)
    {
        try
        {
            await _authSevice.Register(request);
            return Ok("User successfully created!");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during authentication: {Message}", ex.Message);
            return BadRequest(new
            {
                title = "Opération invalide",
                message = ex.Message
            });
        }
    }
    [AllowAnonymous]
    [HttpPost("login")]
    [SwaggerOperation(Tags = ["[Authentication]"])]

    public async Task<ActionResult<AuthResponse>> Authenticate(AuthRequest request)
    {
        try
        {
            var token = await _authSevice.Login(request);
            return Ok(token.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during authentication: {Message}", ex.Message);
            return BadRequest(new
            {
                title = "Opération invalide",
                message = ex.Message
            });
        }
    }
}
