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
    public UrlController(IUrlService urlService)
    {
        _urlService = urlService;
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
        string[] authHeaderParts = HttpContext.Request.Headers.Authorization.ToString().Split(' ');
        if (authHeaderParts.Length != 2 || !authHeaderParts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized();
        }
        var jwt = authHeaderParts[1];
        var handler = new JwtSecurityTokenHandler();
        var decodedToken = handler.ReadJwtToken(jwt);
        var userIdFromToken = decodedToken.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

        if (userIdFromToken == null || userIdFromToken.ToString() != userId)
        {
            return Unauthorized();
        }

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
}

