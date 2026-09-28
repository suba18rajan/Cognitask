using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cognitask.Api.DTOs.Auth;
using Cognitask.Api.DTOs.Projects;
using Cognitask.Api.DTOs.Tasks;
using Xunit;

namespace Cognitask.Api.Tests.Integration;

public class OwnershipSecurityTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OwnershipSecurityTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task User_ShouldNotAccessAnotherUsersProject()
    {
        var userA = await RegisterAndLogin(
            "usera@test.com");

        var userB = await RegisterAndLogin(
            "userb@test.com");

        var projectId =
            await CreateProject(
                userB.AccessToken,
                "User B Project");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                userA.AccessToken);

        var response =
            await _client.GetAsync(
                $"/api/projects/{projectId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task User_ShouldNotUpdateAnotherUsersProject()
    {
        var userA = await RegisterAndLogin(
            "update-a@test.com");

        var userB = await RegisterAndLogin(
            "update-b@test.com");

        var projectId =
            await CreateProject(
                userB.AccessToken,
                "Original Project");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                userA.AccessToken);

        var request = new UpdateProjectRequest
        {
            Name = "Hacked Project",
            Description = "Should not update"
        };

        var response =
            await _client.PutAsJsonAsync(
                $"/api/projects/{projectId}",
                request);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task User_ShouldNotDeleteAnotherUsersProject()
    {
        var userA = await RegisterAndLogin(
            "delete-a@test.com");

        var userB = await RegisterAndLogin(
            "delete-b@test.com");

        var projectId =
            await CreateProject(
                userB.AccessToken,
                "Protected Project");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                userA.AccessToken);

        var response =
            await _client.DeleteAsync(
                $"/api/projects/{projectId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task User_ShouldNotAccessAnotherUsersTask()
    {
        var userA = await RegisterAndLogin(
            "task-a@test.com");

        var userB = await RegisterAndLogin(
            "task-b@test.com");

        var projectId =
            await CreateProject(
                userB.AccessToken,
                "User B Task Project");

        var taskId =
            await CreateTask(
                userB.AccessToken,
                projectId,
                "User B Task");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                userA.AccessToken);

        var response =
            await _client.GetAsync(
                $"/api/tasks/{taskId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task User_ShouldNotUpdateAnotherUsersTask()
    {
        var userA = await RegisterAndLogin(
            "task-update-a@test.com");

        var userB = await RegisterAndLogin(
            "task-update-b@test.com");

        var projectId =
            await CreateProject(
                userB.AccessToken,
                "Task Project");

        var taskId =
            await CreateTask(
                userB.AccessToken,
                projectId,
                "Original Task");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                userA.AccessToken);

        var request = new UpdateTaskRequest
        {
            Title = "Hacked Task",
            Description = "Should not update"
        };

        var response =
            await _client.PutAsJsonAsync(
                $"/api/tasks/{taskId}",
                request);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task User_ShouldNotDeleteAnotherUsersTask()
    {
        var userA = await RegisterAndLogin(
            "task-delete-a@test.com");

        var userB = await RegisterAndLogin(
            "task-delete-b@test.com");

        var projectId =
            await CreateProject(
                userB.AccessToken,
                "Task Delete Project");

        var taskId =
            await CreateTask(
                userB.AccessToken,
                projectId,
                "Protected Task");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                userA.AccessToken);

        var response =
            await _client.DeleteAsync(
                $"/api/tasks/{taskId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    private async Task<AuthResponse> RegisterAndLogin(
        string email)
    {
        var registerRequest = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            Password = "Password123!"
        };

        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                registerRequest);

        registerResponse.EnsureSuccessStatusCode();

        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = "Password123!"
        };

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

        loginResponse.EnsureSuccessStatusCode();

        return (await loginResponse.Content
            .ReadFromJsonAsync<AuthResponse>())!;
    }

    private async Task<Guid> CreateProject(
        string accessToken,
        string name)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        var request = new CreateProjectRequest
        {
            Name = name,
            Description = "Ownership security test"
        };

        var response =
            await _client.PostAsJsonAsync(
                "/api/projects",
                request);

        response.EnsureSuccessStatusCode();

        var project =
            await response.Content
                .ReadFromJsonAsync<ProjectResponse>();

        return project!.Id;
    }

    private async Task<Guid> CreateTask(
        string accessToken,
        Guid projectId,
        string title)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        var request = new CreateTaskRequest
        {
            Title = title,
            Description = "Ownership security test",
            Priority = 2
        };

        var response =
            await _client.PostAsJsonAsync(
                $"/api/projects/{projectId}/tasks",
                request);

        response.EnsureSuccessStatusCode();

        var task =
            await response.Content
                .ReadFromJsonAsync<TaskResponse>();

        return task!.Id;
    }
}