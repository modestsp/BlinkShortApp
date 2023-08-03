namespace UrlShortener.Models;

public class Url
{
    public int Id { get; set; }
    public string OriginalUrl { get; set; } = "";
    public string ShortUrl { get; set; } = "";
    public User? User { get; set; }
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
