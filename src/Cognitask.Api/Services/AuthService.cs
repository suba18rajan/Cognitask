using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Cognitask.Api.Configuration;
using Cognitask.Api.DTOs.Auth;
using Cognitask.Api.Entities;
using Cognitask.Api.Repositories.Interfaces;
using Cognitask.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cognitask.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.EmailExistsAsync(email))
        {
            throw new InvalidOperationException(
                "A user with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password);

        await _userRepository.AddAsync(user);

        var tokenData = CreateRefreshToken(user.Id);

        await _userRepository.AddRefreshTokenAsync(tokenData.RefreshToken);

        await _userRepository.SaveChangesAsync();

        return CreateAuthResponse(user, tokenData);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var passwordResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var tokenData = CreateRefreshToken(user.Id);

        await _userRepository.AddRefreshTokenAsync(
            tokenData.RefreshToken);

        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync();

        return CreateAuthResponse(user, tokenData);
    }

    public async Task<AuthResponse> RefreshTokenAsync(
        string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken =
            await _userRepository.GetRefreshTokenAsync(tokenHash);

        if (storedToken is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid refresh token.");
        }

        if (storedToken.RevokedAtUtc.HasValue)
        {
            throw new UnauthorizedAccessException(
                "Refresh token has been revoked.");
        }

        if (storedToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException(
                "Refresh token has expired.");
        }

        await _userRepository.RevokeRefreshTokenAsync(storedToken);

        var newTokenData = CreateRefreshToken(
            storedToken.UserId);

        await _userRepository.AddRefreshTokenAsync(
            newTokenData.RefreshToken);

        await _userRepository.SaveChangesAsync();

        return CreateAuthResponse(
            storedToken.User,
            newTokenData);
    }

    public async Task LogoutAsync(
        Guid userId,
        string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken =
            await _userRepository.GetRefreshTokenAsync(tokenHash);

        if (storedToken is null)
        {
            return;
        }

        if (storedToken.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You cannot revoke another user's token.");
        }

        if (!storedToken.RevokedAtUtc.HasValue)
        {
            await _userRepository.RevokeRefreshTokenAsync(
                storedToken);

            await _userRepository.SaveChangesAsync();
        }
    }

    public async Task<AuthResponse> GetCurrentUserAsync(
        Guid userId)
    {
        var user =
            await _userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "User not found.");
        }

        return new AuthResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role
        };
    }

    private (RefreshToken RefreshToken, string TokenValue)
        CreateRefreshToken(Guid userId)
    {
        var randomBytes =
            RandomNumberGenerator.GetBytes(64);

        var token =
            Convert.ToBase64String(randomBytes);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = HashToken(token),
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc =
                DateTime.UtcNow.AddDays(
                    _jwtSettings.RefreshTokenExpirationDays)
        };

        return (refreshToken, token);
    }

    private AuthResponse CreateAuthResponse(
        User user,
        (RefreshToken RefreshToken, string TokenValue) tokenData)
    {
        var expiresAtUtc =
            DateTime.UtcNow.AddMinutes(
                _jwtSettings.AccessTokenExpirationMinutes);

        var accessToken =
            GenerateAccessToken(user, expiresAtUtc);

        return new AuthResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            AccessToken = accessToken,
            RefreshToken = tokenData.TokenValue,
            AccessTokenExpiresAtUtc = expiresAtUtc
        };
    }

    private string GenerateAccessToken(
        User user,
        DateTime expiresAtUtc)
    {
        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Email,
                user.Email),

            new(
                ClaimTypes.Role,
                user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _jwtSettings.SecretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(bytes);
    }
}