using Cognitask.Api.DTOs.Projects;

namespace Cognitask.Api.Services.Interfaces;

public interface IProjectService
{
    Task<ProjectResponse> CreateAsync(
        Guid userId,
        CreateProjectRequest request);

    Task<List<ProjectResponse>> GetAllAsync(
        Guid userId);

    Task<ProjectResponse> GetByIdAsync(
        Guid userId,
        Guid projectId);

    Task<ProjectResponse> UpdateAsync(
        Guid userId,
        Guid projectId,
        UpdateProjectRequest request);

    Task DeleteAsync(
        Guid userId,
        Guid projectId);
}
