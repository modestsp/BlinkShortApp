using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using UrlShortener.Dtos;
using UrlShortener.Models;

namespace UrlShortener.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _ctxAccessor;

    public AuthenticationService(UserManager<User> userManager,
    IConfiguration configuration,
    IHttpContextAccessor ctxAccessor)
    {
        _userManager = userManager;
        _configuration = configuration;
        _ctxAccessor = ctxAccessor;
    }

    public async Task<Result<string>> Register(RegisterRequest request)
    {
        var userByEmail = await _userManager.FindByEmailAsync(request.Email);
        var userByUsername = await _userManager.FindByNameAsync(request.UserName);

        if (userByEmail is not null || userByUsername is not null)
        {
            return Result.Fail(new Error($"User with email {request.Email} or username {request.UserName} already exists"));
        }

        User user = new()
        {
            Email = request.Email,
            UserName = request.UserName,
            SecurityStamp = Guid.NewGuid().ToString(),
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        await _userManager.AddToRoleAsync(user, Role.User);

        if (!result.Succeeded)
        {
            return Result.Fail($"Unable to register user {request.UserName} errors: {GetErrorsText(result.Errors)}");
        }

        return await Login(new LoginRequest { UserName = request.UserName, Password = request.Password });
    }

    public async Task<Result<string>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName) ?? await _userManager.FindByEmailAsync(request.UserName);


        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Result.Fail($"Unable to authenticate use {request.UserName}");
        }

        var authClaims = new List<Claim>
        {
            new("username", user.UserName),
            new("email", user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("id", user.Id)
        };

        var userRoles = await _userManager.GetRolesAsync(user);

        authClaims.AddRange(userRoles.Select(userRole => new Claim("role", userRole)));

        var token = GetToken(authClaims);

        return Result.Ok(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public bool VerifyJwt(string userId)
    {
        var token = _ctxAccessor.HttpContext.Request.Headers.Authorization;
        string[] authHeaderParts = token.ToString().Split(' ');
        if (authHeaderParts.Length != 2 || !authHeaderParts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        var jwt = authHeaderParts[1];
        var handler = new JwtSecurityTokenHandler();
        var decodedJwt = handler.ReadJwtToken(jwt);
        var userIdFromJwt = decodedJwt.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

        if (userIdFromJwt == null || userIdFromJwt.ToString() != userId)
        {
            return false;
        }
        return true;
    }
    private JwtSecurityToken GetToken(IEnumerable<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("Secret")));

        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("ValidIssuer"),
            audience: Environment.GetEnvironmentVariable("ValidAudience"),
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

        return token;
    }
    private string GetErrorsText(IEnumerable<IdentityError> errors)
    {
        return string.Join(", ", errors.Select(error => error.Description).ToArray());
    }
}
