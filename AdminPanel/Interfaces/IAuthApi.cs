using AdminPanel.Domain.Models.Auth.Requests;
using AdminPanel.Domain.Models.Auth.Responses;
using Refit;

namespace AdminPanel.Interfaces;

public interface IAuthApi
{
    [Post("/api/auth/login")]
    Task<AuthResponse> LoginAsync([Body] LoginRequest request);

    [Post("/api/auth/register")]
    Task RegisterAsync([Body] RegisterRequest request);

    [Post("/api/auth/set-password")]
    Task SetPasswordAsync([Body] SetPasswordRequest request);

    [Post("/api/auth/refresh")]
    Task<RefreshTokenResponse> RefreshAsync([Body] RefreshTokenRequest request);

    [Post("/api/auth/logout")]
    Task LogoutAsync([Body] RefreshTokenRequest request);
}
