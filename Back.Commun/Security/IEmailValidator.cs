namespace Back.Commun.Security;

public interface IEmailValidator
{
    Task<bool> IsEmailValid(string email);
}
