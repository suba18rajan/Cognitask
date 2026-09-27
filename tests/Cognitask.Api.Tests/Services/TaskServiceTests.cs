using Cognitask.Api.DTOs.Tasks;
using Cognitask.Api.Entities;
using Cognitask.Api.Repositories.Interfaces;
using Cognitask.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;
using TaskPriority = Cognitask.Api.Enums.TaskPriority;
using TaskStatus = Cognitask.Api.Enums.TaskStatus;

namespace Cognitask.Api.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<ILogger<TaskService>> _loggerMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _loggerMock = new Mock<ILogger<TaskService>>();

        _taskService = new TaskService(
            _taskRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var project = new Project
        {
            Id = projectId,
            UserId = userId,
            Name = "Test Project"
        };

        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = 3
        };

        _taskRepositoryMock
            .Setup(r => r.GetProjectForUserAsync(
                projectId,
                userId))
            .ReturnsAsync(project);

        _taskRepositoryMock
            .Setup(r => r.AddAsync(
                It.IsAny<TaskItem>()))
            .Returns(Task.CompletedTask);

        _taskRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _taskService.CreateAsync(
                userId,
                projectId,
                request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Task", result.Title);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(projectId, result.ProjectId);
        Assert.Equal(TaskPriority.High, result.Priority);
        Assert.Equal(TaskStatus.Pending, result.Status);

        _taskRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<TaskItem>()),
            Times.Once);

        _taskRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenProjectDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = 2
        };

        _taskRepositoryMock
            .Setup(r => r.GetProjectForUserAsync(
                projectId,
                userId))
            .ReturnsAsync((Project?)null);

        // Act
        var exception =
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _taskService.CreateAsync(
                    userId,
                    projectId,
                    request));

        // Assert
        Assert.Equal(
            "Project not found.",
            exception.Message);

        _taskRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<TaskItem>()),
            Times.Never);

        _taskRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectInvalidPriority()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var project = new Project
        {
            Id = projectId,
            UserId = userId,
            Name = "Test Project"
        };

        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = 99
        };

        _taskRepositoryMock
            .Setup(r => r.GetProjectForUserAsync(
                projectId,
                userId))
            .ReturnsAsync(project);

        // Act
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _taskService.CreateAsync(
                    userId,
                    projectId,
                    request));

        // Assert
        Assert.Equal(
            "Invalid task priority.",
            exception.Message);
    }

    [Fact]
    public async Task GetByProjectAsync_ShouldRejectInvalidPageNumber()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        // Act
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _taskService.GetByProjectAsync(
                    userId,
                    projectId,
                    0,
                    10));

        // Assert
        Assert.Equal(
            "Page number must be greater than 0.",
            exception.Message);
    }

    [Fact]
    public async Task GetByProjectAsync_ShouldRejectPageSizeAbove100()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        // Act
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _taskService.GetByProjectAsync(
                    userId,
                    projectId,
                    1,
                    101));

        // Assert
        Assert.Equal(
            "Page size must be between 1 and 100.",
            exception.Message);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var task = new TaskItem
        {
            Id = taskId,
            Title = "Test Task",
            Description = "Test Description",
            Status = TaskStatus.Pending,
            Priority = TaskPriority.Medium,
            ProjectId = Guid.NewGuid()
        };

        var request = new UpdateTaskStatusRequest
        {
            Status = (int)TaskStatus.Completed
        };

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(
                taskId,
                userId))
            .ReturnsAsync(task);

        _taskRepositoryMock
            .Setup(r => r.UpdateAsync(
                It.IsAny<TaskItem>()))
            .Returns(Task.CompletedTask);

        _taskRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _taskService.UpdateStatusAsync(
                userId,
                taskId,
                request);

        // Assert
        Assert.Equal(
            TaskStatus.Completed,
            result.Status);

        _taskRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<TaskItem>()),
            Times.Once);

        _taskRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldRejectInvalidStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var request = new UpdateTaskStatusRequest
        {
            Status = 99
        };

        // Act
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _taskService.UpdateStatusAsync(
                    userId,
                    taskId,
                    request));

        // Assert
        Assert.Equal(
            "Invalid task status.",
            exception.Message);
    }

    [Fact]
    public async Task UpdatePriorityAsync_ShouldUpdatePriority()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var task = new TaskItem
        {
            Id = taskId,
            Title = "Test Task",
            Description = "Test Description",
            Status = TaskStatus.Pending,
            Priority = TaskPriority.Low,
            ProjectId = Guid.NewGuid()
        };

        var request = new UpdateTaskPriorityRequest
        {
            Priority = (int)TaskPriority.High
        };

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(
                taskId,
                userId))
            .ReturnsAsync(task);

        _taskRepositoryMock
            .Setup(r => r.UpdateAsync(
                It.IsAny<TaskItem>()))
            .Returns(Task.CompletedTask);

        _taskRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _taskService.UpdatePriorityAsync(
                userId,
                taskId,
                request);

        // Assert
        Assert.Equal(
            TaskPriority.High,
            result.Priority);

        _taskRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<TaskItem>()),
            Times.Once);

        _taskRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenTaskDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(
                taskId,
                userId))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var exception =
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _taskService.GetByIdAsync(
                    userId,
                    taskId));

        // Assert
        Assert.Equal(
            "Task not found.",
            exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var task = new TaskItem
        {
            Id = taskId,
            Title = "Test Task",
            Description = "Test Description",
            ProjectId = Guid.NewGuid()
        };

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(
                taskId,
                userId))
            .ReturnsAsync(task);

        _taskRepositoryMock
            .Setup(r => r.DeleteAsync(
                It.IsAny<TaskItem>()))
            .Returns(Task.CompletedTask);

        _taskRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _taskService.DeleteAsync(
            userId,
            taskId);

        // Assert
        _taskRepositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<TaskItem>()),
            Times.Once);

        _taskRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);
    }
}