using System.Text.RegularExpressions;

namespace Back.Commun.Security;

public class EmailValidator : IEmailValidator
{
    public Task<bool> IsEmailValid(string email)
    {
        return Task.FromResult(ValidateEmail(email));
    }

    public bool ValidateEmail(string email)
    {
        var pattern = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$";
        var regex = new Regex(pattern);

        return regex.IsMatch(email);
    }
}
