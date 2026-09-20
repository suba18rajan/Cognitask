using System.Security.Claims;
using Cognitask.Api.DTOs.Projects;
using Cognitask.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cognitask.Api.Controllers;

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

    [HttpGet]
    public async Task<ActionResult<List<ProjectResponse>>> GetAll()
    {
        var userId = GetCurrentUserId();

        var response =
            await _projectService.GetAllAsync(userId);

        return Ok(response);
    }

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