using Cognitask.Api.DTOs.Tasks;
using Cognitask.Api.Common;

namespace Cognitask.Api.Services.Interfaces;

public interface ITaskService
{
    Task<TaskResponse> CreateAsync(
        Guid userId,
        Guid projectId,
        CreateTaskRequest request);

    Task<PagedResult<TaskResponse>> GetByProjectAsync(
        Guid userId,
        Guid projectId,
        int pageNumber,
        int pageSize);

    Task<TaskResponse> GetByIdAsync(
        Guid userId,
        Guid taskId);

    Task<TaskResponse> UpdateAsync(
        Guid userId,
        Guid taskId,
        UpdateTaskRequest request);

    Task DeleteAsync(
        Guid userId,
        Guid taskId);

    Task<TaskResponse> UpdateStatusAsync(
        Guid userId,
        Guid taskId,
        UpdateTaskStatusRequest request);

    Task<TaskResponse> UpdatePriorityAsync(
        Guid userId,
        Guid taskId,
        UpdateTaskPriorityRequest request);
}