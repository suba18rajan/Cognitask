using Cognitask.Api.DTOs.Auth;

namespace Cognitask.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);

    Task<AuthResponse> RefreshTokenAsync(string refreshToken);

    Task LogoutAsync(Guid userId, string refreshToken);

    Task<AuthResponse> GetCurrentUserAsync(Guid userId);
}