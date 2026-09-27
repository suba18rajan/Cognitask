using System.Security.Cryptography;
using System.Text;
using Cognitask.Api.Configuration;
using Cognitask.Api.DTOs.Auth;
using Cognitask.Api.Entities;
using Cognitask.Api.Repositories.Interfaces;
using Cognitask.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Cognitask.Api.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher<User>>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        var jwtSettings = Options.Create(
            new JwtSettings
            {
                SecretKey =
                    "Test-Secret-Key-That-Is-Long-Enough-For-Tests-123456789",
                Issuer = "Cognitask.Api",
                Audience = "Cognitask.Client",
                AccessTokenExpirationMinutes = 15,
                RefreshTokenExpirationDays = 7
            });

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            jwtSettings,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "John@Example.com",
            Password = "Password123"
        };

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync("john@example.com"))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(h => h.HashPassword(
                It.IsAny<User>(),
                "Password123"))
            .Returns("hashed-password");

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(r => r.AddRefreshTokenAsync(
                It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _authService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal("User", result.Role);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);

        _userRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<User>()),
            Times.Once);

        _userRepositoryMock.Verify(
            r => r.AddRefreshTokenAsync(
                It.IsAny<RefreshToken>()),
            Times.Once);

        _userRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "Password123"
        };

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync("john@example.com"))
            .ReturnsAsync(true);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(request));

        // Assert
        Assert.Equal(
            "A user with this email already exists.",
            exception.Message);

        _userRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PasswordHash = "hashed-password",
            Role = "User"
        };

        var request = new LoginRequest
        {
            Email = "JOHN@EXAMPLE.COM",
            Password = "Password123"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("john@example.com"))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.VerifyHashedPassword(
                user,
                "hashed-password",
                "Password123"))
            .Returns(PasswordVerificationResult.Success);

        _userRepositoryMock
            .Setup(r => r.AddRefreshTokenAsync(
                It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("john@example.com", result.Email);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "unknown@example.com",
            Password = "Password123"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("unknown@example.com"))
            .ReturnsAsync((User?)null);

        // Act
        var exception =
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(request));

        // Assert
        Assert.Equal(
            "Invalid email or password.",
            exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIsInvalid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "john@example.com",
            PasswordHash = "hashed-password"
        };

        var request = new LoginRequest
        {
            Email = "john@example.com",
            Password = "WrongPassword"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("john@example.com"))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.VerifyHashedPassword(
                user,
                "hashed-password",
                "WrongPassword"))
            .Returns(PasswordVerificationResult.Failed);

        // Act
        var exception =
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(request));

        // Assert
        Assert.Equal(
            "Invalid email or password.",
            exception.Message);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldRejectPreviouslyRevokedToken()
    {
        var refreshTokenValue = "revoked-refresh-token";

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = HashToken(refreshTokenValue),
            UserId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(6),
            RevokedAtUtc = DateTime.UtcNow.AddMinutes(-5),
            User = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Role = "User"
            }
        };

        _userRepositoryMock
            .Setup(r => r.GetRefreshTokenAsync(
                HashToken(refreshTokenValue)))
            .ReturnsAsync(storedToken);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.RefreshTokenAsync(refreshTokenValue));

        Assert.Equal(
            "Refresh token has been revoked.",
            exception.Message);

        _userRepositoryMock.Verify(
            r => r.AddRefreshTokenAsync(
                It.IsAny<RefreshToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldRotateToken()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Role = "User"
        };

        var refreshTokenValue = "old-refresh-token";

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = HashToken(refreshTokenValue),
            UserId = userId,
            User = user,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(5)
        };

        _userRepositoryMock
            .Setup(r => r.GetRefreshTokenAsync(
                HashToken(refreshTokenValue)))
            .ReturnsAsync(storedToken);

        _userRepositoryMock
            .Setup(r => r.RevokeRefreshTokenAsync(
                It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(token =>
            {
                token.RevokedAtUtc = DateTime.UtcNow;
            })
            .Returns(Task.CompletedTask);   

        _userRepositoryMock
            .Setup(r => r.AddRefreshTokenAsync(
                It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _authService.RefreshTokenAsync(
                refreshTokenValue);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.NotEqual(
            refreshTokenValue,
            result.RefreshToken);

        Assert.NotNull(storedToken.RevokedAtUtc);

        _userRepositoryMock.Verify(
            r => r.AddRefreshTokenAsync(
                It.IsAny<RefreshToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrow_WhenTokenDoesNotExist()
    {
        // Arrange
        var refreshToken = "invalid-token";

        _userRepositoryMock
            .Setup(r => r.GetRefreshTokenAsync(
                HashToken(refreshToken)))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var exception =
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.RefreshTokenAsync(
                    refreshToken));

        // Assert
        Assert.Equal(
            "Invalid refresh token.",
            exception.Message);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrow_WhenTokenIsRevoked()
    {
        // Arrange
        var refreshToken = "revoked-token";

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = HashToken(refreshToken),
            UserId = Guid.NewGuid(),
            RevokedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(5)
        };

        _userRepositoryMock
            .Setup(r => r.GetRefreshTokenAsync(
                HashToken(refreshToken)))
            .ReturnsAsync(storedToken);

        // Act
        var exception =
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.RefreshTokenAsync(
                    refreshToken));

        // Assert
        Assert.Equal(
            "Refresh token has been revoked.",
            exception.Message);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrow_WhenTokenIsExpired()
    {
        // Arrange
        var refreshToken = "expired-token";

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = HashToken(refreshToken),
            UserId = Guid.NewGuid(),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1)
        };

        _userRepositoryMock
            .Setup(r => r.GetRefreshTokenAsync(
                HashToken(refreshToken)))
            .ReturnsAsync(storedToken);

        // Act
        var exception =
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.RefreshTokenAsync(
                    refreshToken));

        // Assert
        Assert.Equal(
            "Refresh token has expired.",
            exception.Message);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Role = "User"
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result =
            await _authService.GetCurrentUserAsync(userId);

        // Assert
        Assert.Equal(userId, result.UserId);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal("User", result.Role);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var exception =
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.GetCurrentUserAsync(userId));

        // Assert
        Assert.Equal(
            "User not found.",
            exception.Message);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(bytes);
    }
}