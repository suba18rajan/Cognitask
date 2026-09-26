using Cognitask.Api.Entities;

namespace Cognitask.Api.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<Project?> GetProjectForUserAsync(
        Guid projectId,
        Guid userId);

    Task<TaskItem?> GetByIdAsync(
        Guid taskId,
        Guid userId);

    Task<(List<TaskItem> Items, int TotalCount)> GetByProjectAsync(
     Guid projectId,
     Guid userId,
     int pageNumber,
     int pageSize);

    Task AddAsync(TaskItem task);

    Task UpdateAsync(TaskItem task);

    Task DeleteAsync(TaskItem task);

    Task SaveChangesAsync();
}