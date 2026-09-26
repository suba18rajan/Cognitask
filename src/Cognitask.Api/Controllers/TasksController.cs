using Cognitask.Api.Common;
using Cognitask.Api.DTOs.Tasks;
using Cognitask.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Cognitask.Api.Common;

namespace Cognitask.Api.Controllers;

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

    [HttpDelete("tasks/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();

        await _taskService.DeleteAsync(
            userId,
            id);

        return NoContent();
    }

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