using FluentResults;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Database;
using UrlShortener.Dtos;
using UrlShortener.Models;

namespace UrlShortener.Services;

public class UrlService : IUrlService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _ctxAccessor;
    private const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@";

    public UrlService(AppDbContext db, IHttpContextAccessor httpCtx)
    {
        _db = db;
        _ctxAccessor = httpCtx;
    }

    public async Task<Result<CreateUrlResponse>> CreateUrl(CreateUrlRequest request)
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var inputUrl))
        {
            return Result.Fail($"Invalid url");
        }

        var random = new Random();
        var randomStr = new string(Enumerable.Repeat(AllowedChars, 8)
                    .Select(x => x[random.Next(x.Length)]).ToArray());

        var user = await _db.Users.FirstOrDefaultAsync(user => user.Id == request.UserId);

        var sUrl = new Url()
        {
            OriginalUrl = request.Url,
            ShortUrl = randomStr,
            UserId = user?.Id ?? null
        };

        await _db.Urls.AddAsync(sUrl);
        await _db.SaveChangesAsync();

        var result = $"{_ctxAccessor.HttpContext.Request.Scheme}://{_ctxAccessor.HttpContext.Request.Host}/{sUrl.ShortUrl}";

        return Result.Ok(new CreateUrlResponse()
        {
            ShortUrl = result,
            Id = sUrl.Id,
            CreatedAt = sUrl.CreatedAt
        });

    }

    public async Task<Result<List<GetUrlResponse>>> GetUrlsFromUser(string userId)
    {
        var urls = await _db.Urls
            .Where(url => url.UserId == userId)
            .Select(url => new GetUrlResponse()
            {
                Id = url.Id,
                ShortUrl = url.ShortUrl,
                OriginalUrl = url.OriginalUrl,
                CreatedAt = url.CreatedAt
            })
            .ToListAsync();
        if (urls == null)
        {
            return Result.Fail("User not found");
        }
        return Result.Ok(urls);
    }

    public async Task<Result<string>> RedirectToUrl()
    {
        var path = _ctxAccessor.HttpContext.Request.Path.ToUriComponent().Trim('/');
        var urlMatch = await _db.Urls.FirstOrDefaultAsync(
            url => url.ShortUrl.Trim() == path.Trim()
        );

        if (urlMatch == null)
            return Result.Fail("Invalid url");

        return Result.Ok(urlMatch.OriginalUrl);
    }

    public async Task<Result<string>> DeleteUrl(string urlId)
    {
        var urlToDelete = await _db.Urls.FirstOrDefaultAsync(url =>
        url.Id.ToString() == urlId);
        if (urlToDelete == null)
        {
            return Result.Fail("The url does not exists");
        }
        _db.Urls.Remove(urlToDelete);
        return await _db.SaveChangesAsync() > 0 ? Result.Ok("Deleted Successfully") : Result.Fail("Something went wrong while processing the request");
    }
}
