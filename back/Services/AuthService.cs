using back.Models.Authentification;
using Back.Commun.Security;
using Back.Data.Infrastructure.EF;
using Back.Data.Infrastructure.EF.Enums;
using Back.Data.Infrastructure.EF.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back.Services;

public class AuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly OltpDbContext _dbContext;
    private readonly PasswordHasherProvider _passwordHasherProvider;
    private readonly TokenService _tokenService;
    private readonly EmailValidator _emailValidator;
    private readonly PasswordValidator _passwordValidator;

    public AuthService(
        ILogger<AuthService> logger,
        OltpDbContext dbContext,
        PasswordHasherProvider passwordHasherProvider,
        TokenService tokenService,
        EmailValidator emailValidator,
        PasswordValidator passwordValidator)
    {
        _logger = logger;
        _dbContext = dbContext;
        _passwordHasherProvider = passwordHasherProvider;
        _tokenService = tokenService;
        _emailValidator = emailValidator;
        _passwordValidator = passwordValidator;
    }

    public async Task<IActionResult> Register(RegistrationRequest request)
    {
        _logger.LogInformation("Registering user with email: {Email}", request.Email);

        // Check if user with the same email already exists
        var userInDb = await _dbContext.Accounts.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (userInDb != null)
            throw new Exception("Email is already registered.");

        // Validate password
        var validPassword = _passwordValidator.ValidatePassword(request.Password);
        if (!validPassword)
            throw new Exception("Le mot de passe doit contenir au moins 8 caractères, une lettre majuscule, une lettre minuscule, un chiffre et un caractère spécial.");

        // Validate email
        var validEmail = _emailValidator.ValidateEmail(request.Email);
        if (!validEmail)
            throw new Exception("Email invalid");

        // Generate password hash and salt
        _passwordHasherProvider.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        // Use transaction to ensure both Account and Customer are created together
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Create Account
            var newAccount = new AccountDao
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = UserRole.Client // Default role is Client
            };

            _dbContext.Accounts.Add(newAccount);
            await _dbContext.SaveChangesAsync();

            // If role is Client, create Customer profile
            if (newAccount.Role == UserRole.Client)
            {
                var newCustomer = new CustomerDao
                {
                    AccountId = newAccount.Id,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Customers.Add(newCustomer);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Customer profile created for Account {AccountId}", newAccount.Id);
            }

            await transaction.CommitAsync();

            _logger.LogInformation("User registered successfully with ID {AccountId}", newAccount.Id);
            return new OkResult();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to register user");
            throw;
        }
    }

    public async Task<ActionResult<AuthResponse>> Login(AuthRequest request)
    {
        // Check if the user exists in the database
        var userInDb = await _dbContext.Accounts
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (userInDb is null)
            throw new InvalidOperationException("Email ou Mot de passe invalid !");

        if (userInDb.PasswordHash is null || userInDb.PasswordSalt is null)
            throw new InvalidOperationException("Email ou Mot de passe invalid !");

        if (!_passwordHasherProvider.VerifyPasswordHash(request.Password, userInDb.PasswordHash, userInDb.PasswordSalt))
            throw new InvalidOperationException("Email ou Mot de passe invalid !");

        var accessToken = _tokenService.CreateToken(userInDb);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        return new AuthResponse
        {
            Token = accessToken
        };
    }
}
