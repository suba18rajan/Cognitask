using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cognitask.Api.DTOs.Auth;
using Cognitask.Api.DTOs.Projects;

namespace Cognitask.Api.Tests.Integration;

public class ProjectApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProjectApiTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProjects_ShouldReturnUnauthorized_WithoutToken()
    {
        // Act
        var response =
            await _client.GetAsync("/api/Projects");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateProject_ShouldCreateProject_WhenAuthenticated()
    {
        // Arrange
        var email =
            $"project-{Guid.NewGuid()}@example.com";

        await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new RegisterRequest
            {
                FirstName = "Project",
                LastName = "Test",
                Email = email,
                Password = "Password123"
            });

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/login",
                new LoginRequest
                {
                    Email = email,
                    Password = "Password123"
                });

        var auth =
            await loginResponse.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(auth);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                auth.AccessToken);

        var request = new CreateProjectRequest
        {
            Name = "Integration Project",
            Description = "Created through integration test"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/Projects",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var project =
            await response.Content
                .ReadFromJsonAsync<ProjectResponse>();

        Assert.NotNull(project);
        Assert.Equal(
            "Integration Project",
            project.Name);
        Assert.Equal(
            auth.UserId,
            project.UserId);
    }
}