using Auth.Application.DTOs.Authentication;
using Auth.Application.Interfaces;
using Auth.Infrastructure.Authentication;
using Auth.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenService _jwtTokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "User account is inactive.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(
            user,
            request.Password);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var accessToken = await _jwtTokenService.GenerateTokenAsync(user);

        return new LoginResponse
        {
            AccessToken = accessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }
}