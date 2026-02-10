using Back.Data.Infrastructure.EF.Enums;
using Back.Data.Infrastructure.EF.Models;

namespace Back.Commun.Account;

public static class AccountHelper
{
    public static bool IsAdmin(this AccountDao account)
    {
        return account.Role == UserRole.Admin;
    }

    public static bool IsClient(this AccountDao account)
    {
        return account.Role ==  UserRole.Client;
    }
}
