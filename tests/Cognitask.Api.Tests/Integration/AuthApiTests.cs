using System.Net;
using System.Net.Http.Json;
using Cognitask.Api.DTOs.Auth;

namespace Cognitask.Api.Tests.Integration;

public class AuthApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturnCreatedUser()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "Integration",
            LastName = "Test",
            Email =
                $"integration-{Guid.NewGuid()}@example.com",
            Password = "Password123"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.Equal(
            request.Email.ToLowerInvariant(),
            result.Email);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
    }

    [Fact]
    public async Task Register_ShouldRejectDuplicateEmail()
    {
        // Arrange
        var email =
            $"duplicate-{Guid.NewGuid()}@example.com";

        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            Password = "Password123"
        };

        // First registration
        var firstResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                request);

        Assert.Equal(
            HttpStatusCode.OK,
            firstResponse.StatusCode);

        // Act
        var secondResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Conflict,
            secondResponse.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnAccessToken()
    {
        // Arrange
        var email =
            $"login-{Guid.NewGuid()}@example.com";

        var password = "Password123";

        var registerRequest = new RegisterRequest
        {
            FirstName = "Login",
            LastName = "Test",
            Email = email,
            Password = password
        };

        await _client.PostAsJsonAsync(
            "/api/Auth/register",
            registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/Auth/login",
                loginRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
    }

    [Fact]
    public async Task Login_ShouldRejectInvalidPassword()
    {
        // Arrange
        var email =
            $"invalid-password-{Guid.NewGuid()}@example.com";

        await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = email,
                Password = "Password123"
            });

        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = "WrongPassword"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/Auth/login",
                loginRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Register_ShouldRejectInvalidRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "",
            LastName = "",
            Email = "invalid-email",
            Password = "123"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task GetMe_ShouldReturnUnauthorized_WithoutToken()
    {
        // Act
        var response =
            await _client.GetAsync("/api/Auth/me");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetMe_ShouldReturnUser_WhenAuthenticated()
    {
        // Arrange
        var email =
            $"me-{Guid.NewGuid()}@example.com";

        var password = "Password123";

        await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new RegisterRequest
            {
                FirstName = "Current",
                LastName = "User",
                Email = email,
                Password = password
            });

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/login",
                new LoginRequest
                {
                    Email = email,
                    Password = password
                });

        var authResponse =
            await loginResponse.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(authResponse);

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                authResponse.AccessToken);

        // Act
        var response =
            await _client.GetAsync("/api/Auth/me");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }
}