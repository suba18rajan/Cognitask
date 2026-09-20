using Cognitask.Api.Entities;

namespace Cognitask.Api.Repositories.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, Guid userId);

    Task<List<Project>> GetAllAsync(Guid userId);

    Task AddAsync(Project project);

    Task UpdateAsync(Project project);

    Task DeleteAsync(Project project);

    Task SaveChangesAsync();
}