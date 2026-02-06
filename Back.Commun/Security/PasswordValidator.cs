using System.Text.RegularExpressions;

namespace Back.Commun.Security;

public class PasswordValidator : IPasswordValidator
{
    public Task<bool> IsPasswordValid(string password)
    {
        return Task.FromResult(ValidatePassword(password));
    }

    public bool ValidatePassword(string password)
    {

        var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$";
        var regex = new Regex(pattern);

        return regex.IsMatch(password);
    }
}
