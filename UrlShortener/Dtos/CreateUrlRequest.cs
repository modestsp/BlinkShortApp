namespace UrlShortener.Dtos;

public class CreateUrlRequest
{
    public string Url { get; set; } = "";
    public string? UserId { get; set; }
}