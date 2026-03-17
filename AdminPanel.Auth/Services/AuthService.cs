using AdminPanel.Domain.Entities;
using AdminPanel.Domain.Interfaces.Auth;
using AdminPanel.Domain.Models.Auth.Requests;
using AdminPanel.Domain.Models.Auth.Responses;
using AdminPanel.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace AdminPanel.Auth.Services;

public class AuthService
{
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthService(
        IPasswordHasherService passwordHasherService,
        ITokenService tokenService,
        IConfiguration config,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _passwordHasherService = passwordHasherService;
        _tokenService = tokenService;
        _config = config;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<User> Register(RegisterRequest request)
    {
        if (await _userRepository.FirstOrDefaultAsync(request.Login) is not null)
            throw new InvalidOperationException("User already exists");

        var user = new User
        {
            FullName = request.FullName,
            Login = request.Login,
            PasswordHash = null,
            Salt = null,
            Permissions = request.Permissions,
            CreatedAt = DateTime.UtcNow
        };
        _userRepository.Add(user);
        return user;
    }

    public async Task<AuthResponse> Login(LoginRequest request)
    {
        var user = await _userRepository.FirstOrDefaultAsync(request.Login);
        if (user is null)
            throw new UnauthorizedAccessException("Invalid credentials");

        if (!user.PasswordSet)
        {
            return new AuthResponse
            {
                RequiresPasswordSetup = true,
                FullName = user.FullName,
                Permissions = user.Permissions
            };
        }

        if (!_passwordHasherService.VerifyPassword(request.Password, user.PasswordHash, user.Salt))
            throw new UnauthorizedAccessException("Invalid credentials");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = await GenerateAndSaveRefreshToken(user.Id);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            FullName = user.FullName,
            Permissions = user.Permissions,
            RequiresPasswordSetup = false
        };
    }

    public async Task SetPassword(SetPasswordRequest request)
    {
        var user = await _userRepository.FirstOrDefaultAsync(request.Login);
        if (user is null)
            throw new Exception("User not found");

        if (user.PasswordSet)
            throw new Exception("Password already set");

        var hash = _passwordHasherService.HashPassword(request.Password, out var salt);
        user.PasswordHash = hash;
        user.Salt = salt;
        await _userRepository.SaveAsync();
    }

    public async Task<RefreshTokenResponse> RefreshToken(string refreshToken)
    {
        var storedToken = await _refreshTokenRepository.FirstOrDefaultIncludeUserAsync(refreshToken);

        if (storedToken is null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        var newAccessToken = _tokenService.GenerateAccessToken(storedToken.User);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddDays(7);

        storedToken.IsRevoked = true;

        _refreshTokenRepository.Add(new RefreshToken
        {
            UserId = storedToken.UserId,
            Token = newRefreshToken,
            ExpiryDate = expiry,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        });
        await _refreshTokenRepository.SaveAsync();

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task RevokeRefreshToken(string refreshToken)
    {
        var storedToken = await _refreshTokenRepository.FirstOrDefaultAsync(refreshToken);
        if (storedToken is not null)
        {
            storedToken.IsRevoked = true;
            await _refreshTokenRepository.SaveAsync();
        }
    }

    private async Task<string> GenerateAndSaveRefreshToken(int userId)
    {
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddDays(7);

        _refreshTokenRepository.Add(new RefreshToken
        {
            UserId = userId,
            Token = refreshToken,
            ExpiryDate = expiry,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        });
        await _refreshTokenRepository.SaveAsync();
        return refreshToken;
    }
}

