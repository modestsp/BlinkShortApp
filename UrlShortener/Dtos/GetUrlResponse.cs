namespace UrlShortener.Dtos;

public class GetUrlResponse
{
    public int Id { get; set; }
    public string OriginalUrl { get; set; }
    public string ShortUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}