using FluentResults;
using UrlShortener.Dtos;
using UrlShortener.Models;

namespace UrlShortener.Services;

public interface IUrlService
{
    Task<Result<CreateUrlResponse>> CreateUrl(CreateUrlRequest request);
    Task<Result<string>> RedirectToUrl();
    Task<Result<List<Url>>> GetUrlsFromUser(string userId);
    Task<Result<string>> DeleteUrl(string urlId);
}