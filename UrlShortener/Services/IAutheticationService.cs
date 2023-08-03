using FluentResults;
using UrlShortener.Dtos;

namespace UrlShortener.Services;

public interface IAuthenticationService
{
    Task<Result<string>> Register(RegisterRequest request);
    Task<Result<string>> Login(LoginRequest request);
    bool VerifyJwt(string userId);
}