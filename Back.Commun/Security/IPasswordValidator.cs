namespace Back.Commun.Security;

public interface IPasswordValidator
{
    Task<bool> IsPasswordValid(string password);
}
