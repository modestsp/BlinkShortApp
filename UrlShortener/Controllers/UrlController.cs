using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Database;
using UrlShortener.Dtos;
using UrlShortener.Extensions;
using UrlShortener.Models;
using UrlShortener.Services;

namespace UrlShortener.Controllers;

[ApiController]
[Route("/")]
public class UrlController : ControllerBase
{
    private readonly IUrlService _urlService;
    private readonly IAuthenticationService _authService;
    public UrlController(IUrlService urlService, IAuthenticationService authService)
    {
        _urlService = urlService;
        _authService = authService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateUrl(CreateUrlRequest request)
    {
        var result = await _urlService.CreateUrl(request);
        var resultDto = result.ToResultDto();

        if (!resultDto.IsSuccess)
        {
            return BadRequest(resultDto);
        }
        return Ok(resultDto);
    }
    [Authorize("StandardRights")]
    [HttpGet("/user/{userId}")]
    public async Task<IActionResult> GetUrls(string userId)
    {
        var checkJwt = _authService.VerifyJwt(userId);

        if (!checkJwt)
            return Unauthorized();

        var result = await _urlService.GetUrlsFromUser(userId);
        var resultDto = result.ToResultDto();

        if (!resultDto.IsSuccess)
        {
            return BadRequest(resultDto);
        }

        return Ok(resultDto);

    }
    [HttpGet("/{shortUrl}")]
    public async Task<IActionResult> RedirectToUrl()
    {
        var result = await _urlService.RedirectToUrl();
        var resultDto = result.ToResultDto();

        if (!resultDto.IsSuccess)
        {
            return BadRequest(resultDto);
        };

        return Redirect(resultDto.Response);
    }
    [HttpDelete("/{userId}/{urlId}")]
    public async Task<IActionResult> DeleteUrl(string urlId, string userId)
    {
        var checkJwt = _authService.VerifyJwt(userId);
        if (!checkJwt)
            return Unauthorized();

        var result = await _urlService.DeleteUrl(urlId);
        var resultDto = result.ToResultDto();

        if (!resultDto.IsSuccess)
        {
            return BadRequest(resultDto.Errors);
        }

        return Ok(resultDto);
    }
}

