using Cognitask.Api.DTOs.Tasks;
using Cognitask.Api.Entities;
using Cognitask.Api.Repositories.Interfaces;
using Cognitask.Api.Services.Interfaces;
using TaskPriority = Cognitask.Api.Enums.TaskPriority;
using TaskStatus = Cognitask.Api.Enums.TaskStatus;
using Cognitask.Api.Common;

namespace Cognitask.Api.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskResponse> CreateAsync(
        Guid userId,
        Guid projectId,
        CreateTaskRequest request)
    {
        var project =
            await _taskRepository.GetProjectForUserAsync(
                projectId,
                userId);

        if (project is null)
        {
            throw new KeyNotFoundException(
                "Project not found.");
        }

        if (!Enum.IsDefined(
                typeof(TaskPriority),
                request.Priority))
        {
            throw new ArgumentException(
                "Invalid priority.");
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = TaskStatus.Pending,
            Priority = (TaskPriority)request.Priority,
            ProjectId = projectId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();

        return MapToResponse(task);
    }

    public async Task<PagedResult<TaskResponse>> GetByProjectAsync(
    Guid userId,
    Guid projectId,
    int pageNumber,
    int pageSize)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentException(
                "Page number must be greater than 0.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException(
                "Page size must be between 1 and 100.");
        }

        var project =
            await _taskRepository.GetProjectForUserAsync(
                projectId,
                userId);

        if (project is null)
        {
            throw new KeyNotFoundException(
                "Project not found.");
        }

        var (tasks, totalCount) =
            await _taskRepository.GetByProjectAsync(
                projectId,
                userId,
                pageNumber,
                pageSize);

        var totalPages =
            (int)Math.Ceiling(
                totalCount / (double)pageSize);

        return new PagedResult<TaskResponse>
        {
            Items = tasks
                .Select(task => new TaskResponse
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    Status = task.Status,
                    Priority = task.Priority,
                    ProjectId = task.ProjectId,
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt
                })
                .ToList(),

            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<TaskResponse> GetByIdAsync(
        Guid userId,
        Guid taskId)
    {
        var task =
            await _taskRepository.GetByIdAsync(
                taskId,
                userId);

        if (task is null)
        {
            throw new KeyNotFoundException(
                "Task not found.");
        }

        return MapToResponse(task);
    }

    public async Task<TaskResponse> UpdateAsync(
        Guid userId,
        Guid taskId,
        UpdateTaskRequest request)
    {
        var task =
            await _taskRepository.GetByIdAsync(
                taskId,
                userId);

        if (task is null)
        {
            throw new KeyNotFoundException(
                "Task not found.");
        }

        task.Title = request.Title.Trim();
        task.Description = request.Description.Trim();
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task);
        await _taskRepository.SaveChangesAsync();

        return MapToResponse(task);
    }

    public async Task DeleteAsync(
        Guid userId,
        Guid taskId)
    {
        var task =
            await _taskRepository.GetByIdAsync(
                taskId,
                userId);

        if (task is null)
        {
            throw new KeyNotFoundException(
                "Task not found.");
        }

        await _taskRepository.DeleteAsync(task);
        await _taskRepository.SaveChangesAsync();
    }

    public async Task<TaskResponse> UpdateStatusAsync(
        Guid userId,
        Guid taskId,
        UpdateTaskStatusRequest request)
    {
        if (!Enum.IsDefined(
                typeof(TaskStatus),
                request.Status))
        {
            throw new ArgumentException(
                "Invalid status.");
        }

        var task =
            await _taskRepository.GetByIdAsync(
                taskId,
                userId);

        if (task is null)
        {
            throw new KeyNotFoundException(
                "Task not found.");
        }

        task.Status = (TaskStatus)request.Status;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task);
        await _taskRepository.SaveChangesAsync();

        return MapToResponse(task);
    }

    public async Task<TaskResponse> UpdatePriorityAsync(
        Guid userId,
        Guid taskId,
        UpdateTaskPriorityRequest request)
    {
        if (!Enum.IsDefined(
                typeof(TaskPriority),
                request.Priority))
        {
            throw new ArgumentException(
                "Invalid priority.");
        }

        var task =
            await _taskRepository.GetByIdAsync(
                taskId,
                userId);

        if (task is null)
        {
            throw new KeyNotFoundException(
                "Task not found.");
        }

        task.Priority = (TaskPriority)request.Priority;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task);
        await _taskRepository.SaveChangesAsync();

        return MapToResponse(task);
    }

    private static TaskResponse MapToResponse(
        TaskItem task)
    {
        return new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            ProjectId = task.ProjectId,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}