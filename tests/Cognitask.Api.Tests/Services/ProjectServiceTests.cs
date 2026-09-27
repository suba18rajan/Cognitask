using Cognitask.Api.DTOs.Projects;
using Cognitask.Api.Entities;
using Cognitask.Api.Repositories.Interfaces;
using Cognitask.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Cognitask.Api.Tests.Services;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<ILogger<ProjectService>> _loggerMock;
    private readonly ProjectService _projectService;

    public ProjectServiceTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _loggerMock = new Mock<ILogger<ProjectService>>();

        _projectService = new ProjectService(
            _projectRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateProject()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var request = new CreateProjectRequest
        {
            Name = "Test Project",
            Description = "Test Description"
        };

        _projectRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Project>()))
            .Returns(Task.CompletedTask);

        _projectRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _projectService.CreateAsync(
                userId,
                request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Project", result.Name);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(userId, result.UserId);

        _projectRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Project>()),
            Times.Once);

        _projectRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldRejectInvalidPageNumber()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _projectService.GetAllAsync(
                userId,
                0,
                10));

        // Assert
        Assert.Equal(
            "Page number must be greater than 0.",
            exception.Message);
    }

    [Fact]
    public async Task GetAllAsync_ShouldRejectPageSizeAbove100()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _projectService.GetAllAsync(
                userId,
                1,
                101));

        // Assert
        Assert.Equal(
            "Page size must be between 1 and 100.",
            exception.Message);
    }
}