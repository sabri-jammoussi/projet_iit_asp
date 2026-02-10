using Back.Data.Infrastructure.EF.Models;

namespace Back.Commun.Account;

public interface ICurrentUserProvider
{
    Task<AccountDao> Get();
}
