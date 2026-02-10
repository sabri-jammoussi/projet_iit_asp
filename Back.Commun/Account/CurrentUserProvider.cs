using System.Security.Claims;
using Back.Data.Infrastructure.EF;
using Back.Data.Infrastructure.EF.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Back.Commun.Account;

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly OltpDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserProvider(
      OltpDbContext context,
      IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }
    public bool IsAuthenticated()
       => _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier) != null;
    public async Task<AccountDao> Get()
    {
        if (!IsAuthenticated())
            return null;
        var userIdentifier = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        var user = await _context.Accounts
            .SingleOrDefaultAsync(u => u.Id.ToString() == userIdentifier)
            .ConfigureAwait(false);

        if (user == null)
            throw new Exception("Impossible de charger l'utilisateur courant");

        return user;
    }
}
