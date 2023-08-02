using Microsoft.AspNetCore.Identity;

namespace UrlShortener.Models;

public class User : IdentityUser
{
    public ICollection<Url> Urls { get; set; }

}