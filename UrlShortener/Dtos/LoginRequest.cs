using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Dtos;

public class LoginRequest
{
    [MinLength(Consts.UserNameMinLength, ErrorMessage = Consts.UsernameLengthValidationError)]
    public string? UserName { get; set; }

    [RegularExpression(Consts.PasswordRegex, ErrorMessage = Consts.PasswordValidationError)]
    public string? Password { get; set; }
}