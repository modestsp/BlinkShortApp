namespace UrlShortener.Dtos;

public class CreateUrlResponse
{
    public int Id { get; set; }
    public string ShortUrl { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}