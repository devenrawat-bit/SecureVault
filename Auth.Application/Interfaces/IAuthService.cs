using Auth.Application.DTOs.Authentication;

namespace Auth.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}