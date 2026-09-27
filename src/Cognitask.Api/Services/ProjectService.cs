using Cognitask.Api.Common;
using Cognitask.Api.DTOs.Projects;
using Cognitask.Api.Entities;
using Cognitask.Api.Repositories.Interfaces;
using Cognitask.Api.Services.Interfaces;

namespace Cognitask.Api.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        IProjectRepository projectRepository,
        ILogger<ProjectService> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<ProjectResponse> CreateAsync(
        Guid userId,
        CreateProjectRequest request)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project);
        await _projectRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Project created. ProjectId: {ProjectId}, UserId: {UserId}",
            project.Id,
            userId);

        return MapToResponse(project);
    }

    public async Task<PagedResult<ProjectResponse>> GetAllAsync(
        Guid userId,
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

        var (projects, totalCount) =
            await _projectRepository.GetAllAsync(
                userId,
                pageNumber,
                pageSize);

        var totalPages =
            (int)Math.Ceiling(
                totalCount / (double)pageSize);

        return new PagedResult<ProjectResponse>
        {
            Items = projects
                .Select(MapToResponse)
                .ToList(),

            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<ProjectResponse> GetByIdAsync(
        Guid userId,
        Guid projectId)
    {
        var project =
            await _projectRepository.GetByIdAsync(
                projectId,
                userId);

        if (project is null)
        {
            throw new KeyNotFoundException(
                "Project not found.");
        }

        return MapToResponse(project);
    }

    public async Task<ProjectResponse> UpdateAsync(
        Guid userId,
        Guid projectId,
        UpdateProjectRequest request)
    {
        var project =
            await _projectRepository.GetByIdAsync(
                projectId,
                userId);

        if (project is null)
        {
            throw new KeyNotFoundException(
                "Project not found.");
        }

        project.Name = request.Name.Trim();
        project.Description = request.Description.Trim();
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project);
        await _projectRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Project updated. ProjectId: {ProjectId}, UserId: {UserId}",
            projectId,
            userId);

        return MapToResponse(project);
    }

    public async Task DeleteAsync(
        Guid userId,
        Guid projectId)
    {
        var project =
            await _projectRepository.GetByIdAsync(
                projectId,
                userId);

        if (project is null)
        {
            throw new KeyNotFoundException(
                "Project not found.");
        }

        await _projectRepository.DeleteAsync(project);
        await _projectRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Project deleted. ProjectId: {ProjectId}, UserId: {UserId}",
            projectId,
            userId);
    }

    private static ProjectResponse MapToResponse(
        Project project)
    {
        return new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            UserId = project.UserId,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }
}