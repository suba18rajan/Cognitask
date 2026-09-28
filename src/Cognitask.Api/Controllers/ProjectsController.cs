using System.Security.Claims;
using Cognitask.Api.DTOs.Projects;
using Cognitask.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cognitask.Api.Common;

namespace Cognitask.Api.Controllers;

/// <summary>
/// Provides CRUD operations for projects owned by the authenticated user.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// Creates a new project for the authenticated user.
    /// </summary>
    /// <param name="request">Project creation details.</param>
    /// <returns>The newly created project.</returns>
    /// <response code="201">Project created successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">User is not authenticated.</response>

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create(
        CreateProjectRequest request)
    {
        var userId = GetCurrentUserId();

        var response =
            await _projectService.CreateAsync(
                userId,
                request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    /// <summary>
    /// Returns a paginated list of projects owned by the authenticated user.
    /// </summary>
    /// <param name="pageNumber">Page number starting from 1.</param>
    /// <param name="pageSize">Number of projects per page.</param>
    /// <returns>Paginated project results.</returns>
    /// <response code="200">Projects returned successfully.</response>
    /// <response code="400">Invalid pagination parameters.</response>
    /// <response code="401">User is not authenticated.</response>
    
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProjectResponse>>> GetAll(
    int pageNumber = 1,
    int pageSize = 10)
    {
        var userId = GetCurrentUserId();

        var response =
            await _projectService.GetAllAsync(
                userId,
                pageNumber,
                pageSize);

        return Ok(response);
    }

    /// <summary>
    /// Gets a project owned by the authenticated user.
    /// </summary>
    /// <param name="id">Project identifier.</param>
    /// <returns>The requested project.</returns>
    /// <response code="200">Project found.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Project not found.</response>

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> GetById(
        Guid id)
    {
        var userId = GetCurrentUserId();

        var response =
            await _projectService.GetByIdAsync(
                userId,
                id);

        return Ok(response);
    }

    /// <summary>
    /// Updates a project belonging to the authenticated user.
    /// </summary>
    /// <param name="id">Project identifier.</param>
    /// <param name="request">Updated project details.</param>
    /// <returns>The updated project.</returns>
    /// <response code="200">Project updated successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Project was not found.</response>

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> Update(
        Guid id,
        UpdateProjectRequest request)
    {
        var userId = GetCurrentUserId();

        var response =
            await _projectService.UpdateAsync(
                userId,
                id,
                request);

        return Ok(response);
    }

    /// <summary>
    /// Deletes a project belonging to the authenticated user.
    /// </summary>
    /// <param name="id">Project identifier.</param>
    /// <response code="204">Project deleted successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Project was not found.</response>

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();

        await _projectService.DeleteAsync(
            userId,
            id);

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
                userIdValue,
                out var userId))
        {
            throw new UnauthorizedAccessException(
                "Invalid user identity.");
        }

        return userId;
    }
}