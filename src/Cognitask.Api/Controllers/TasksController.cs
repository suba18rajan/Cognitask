using Cognitask.Api.Common;
using Cognitask.Api.DTOs.Tasks;
using Cognitask.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Cognitask.Api.Common;

namespace Cognitask.Api.Controllers;

/// <summary>
/// Provides endpoints for managing tasks within user-owned projects.
/// </summary>

[ApiController]
[Route("api")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Creates a new task inside a user-owned project.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="request">Task creation details.</param>
    /// <returns>The newly created task.</returns>
    /// <response code="200">Task created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Project was not found.</response>

    [HttpPost("projects/{projectId:guid}/tasks")]
    public async Task<ActionResult<TaskResponse>> Create(
        Guid projectId,
        CreateTaskRequest request)
    {
        var userId = GetCurrentUserId();

        var response =
            await _taskService.CreateAsync(
                userId,
                projectId,
                request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    /// <summary>
    /// Returns a paginated list of tasks belonging to a project.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="pageNumber">Page number starting from 1.</param>
    /// <param name="pageSize">Number of tasks per page.</param>
    /// <returns>A paginated list of project tasks.</returns>
    /// <response code="200">Tasks returned successfully.</response>
    /// <response code="400">Invalid pagination parameters.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Project was not found.</response>

    [HttpGet("projects/{projectId:guid}/tasks")]
    public async Task<ActionResult<PagedResult<TaskResponse>>> GetByProject(
    Guid projectId,
    int pageNumber = 1,
    int pageSize = 10)
    {
        var userId = GetCurrentUserId();

        var response =
            await _taskService.GetByProjectAsync(
                userId,
                projectId,
                pageNumber,
                pageSize);

        return Ok(response);
    }

    /// <summary>
    /// Returns a task belonging to the authenticated user's project.
    /// </summary>
    /// <param name="id">Task identifier.</param>
    /// <returns>The requested task.</returns>
    /// <response code="200">Task returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Task was not found.</response>

    [HttpGet("tasks/{id:guid}")]
    public async Task<ActionResult<TaskResponse>> GetById(
        Guid id)
    {
        var userId = GetCurrentUserId();

        var response =
            await _taskService.GetByIdAsync(
                userId,
                id);

        return Ok(response);
    }

    /// <summary>
    /// Updates a task belonging to the authenticated user.
    /// </summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="request">Updated task details.</param>
    /// <returns>The updated task.</returns>
    /// <response code="200">Task updated successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Task was not found.</response>

    [HttpPut("tasks/{id:guid}")]
    public async Task<ActionResult<TaskResponse>> Update(
        Guid id,
        UpdateTaskRequest request)
    {
        var userId = GetCurrentUserId();

        var response =
            await _taskService.UpdateAsync(
                userId,
                id,
                request);

        return Ok(response);
    }

    /// <summary>
    /// Deletes a task belonging to the authenticated user.
    /// </summary>
    /// <param name="id">Task identifier.</param>
    /// <response code="204">Task deleted successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Task was not found.</response>

    [HttpDelete("tasks/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();

        await _taskService.DeleteAsync(
            userId,
            id);

        return NoContent();
    }

    /// <summary>
    /// Updates the status of a task.
    /// </summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="request">New task status.</param>
    /// <returns>The updated task.</returns>
    /// <response code="200">Task status updated successfully.</response>
    /// <response code="400">Invalid task status.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Task was not found.</response>

    [HttpPatch("tasks/{id:guid}/status")]
    public async Task<ActionResult<TaskResponse>> UpdateStatus(
        Guid id,
        UpdateTaskStatusRequest request)
    {
        var userId = GetCurrentUserId();

        var response =
            await _taskService.UpdateStatusAsync(
                userId,
                id,
                request);

        return Ok(response);
    }

    /// <summary>
    /// Updates the priority of a task.
    /// </summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="request">New task priority.</param>
    /// <returns>The updated task.</returns>
    /// <response code="200">Task priority updated successfully.</response>
    /// <response code="400">Invalid task priority.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Task was not found.</response>

    [HttpPatch("tasks/{id:guid}/priority")]
    public async Task<ActionResult<TaskResponse>> UpdatePriority(
        Guid id,
        UpdateTaskPriorityRequest request)
    {
        var userId = GetCurrentUserId();

        var response =
            await _taskService.UpdatePriorityAsync(
                userId,
                id,
                request);

        return Ok(response);
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