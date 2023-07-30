using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Dtos;

public class RegisterRequest
{
    [MinLength(Consts.UserNameMinLength, ErrorMessage = Consts.UsernameLengthValidationError)]
    public string? UserName { get; set; }

    [EmailAddress(ErrorMessage = Consts.EmailValidationError)]
    public string? Email { get; set; }

    [RegularExpression(Consts.PasswordRegex, ErrorMessage = Consts.PasswordValidationError)]
    public string? Password { get; set; }
}