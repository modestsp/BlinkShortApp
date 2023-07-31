using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Database;
using UrlShortener.Dtos;
using UrlShortener.Models;

namespace UrlShortener.Controllers;

[ApiController]
[Route("/")]
public class UrlController : ControllerBase
{
    private readonly AppDbContext _context;
    public UrlController(AppDbContext context)
    {
        _context = context;
    }


    [HttpPost("create")]
    public async Task<IActionResult> CreateUrl(CreateUrlRequest request)
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var inputUrl))
        {
            return BadRequest("Invalid url");
        }

        var random = new Random();

        const string chars = "ABCDEFGHIJKLMNQPRTSUVWZ1234567890ab@";

        var randomStr = new string(Enumerable.Repeat(chars, 8)
            .Select(x => x[random.Next(x.Length)]).ToArray());

        var sUrl = new Url()
        {
            OriginalUrl = request.Url,
            ShortUrl = randomStr
        };

        await _context.Urls.AddAsync(sUrl);
        await _context.SaveChangesAsync();

        var response = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{sUrl.ShortUrl}";

        return Ok(new CreateUrlResponse()
        {
            ShortUrl = response
        });
    }
    [HttpGet("/{shortUrl}")]
    public async Task<IActionResult> RedirectToUrl()
    {
        var path = HttpContext.Request.Path.ToUriComponent().Trim('/');
        var urlMatch = await _context.Urls.FirstOrDefaultAsync(
            x => x.ShortUrl.Trim() == path.Trim()
        );

        if (urlMatch == null)
            return BadRequest("Invalid url");

        return Redirect(urlMatch.OriginalUrl);
    }
}

