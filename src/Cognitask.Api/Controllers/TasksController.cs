using System.Security.Claims;
using Cognitask.Api.DTOs.Tasks;
using Cognitask.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        try
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
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("projects/{projectId:guid}/tasks")]
    public async Task<ActionResult<List<TaskResponse>>> GetByProject(
        Guid projectId)
    {
        try
        {
            var userId = GetCurrentUserId();

            var response =
                await _taskService.GetByProjectAsync(
                    userId,
                    projectId);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("tasks/{id:guid}")]
    public async Task<ActionResult<TaskResponse>> GetById(
        Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();

            var response =
                await _taskService.GetByIdAsync(
                    userId,
                    id);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("tasks/{id:guid}")]
    public async Task<ActionResult<TaskResponse>> Update(
        Guid id,
        UpdateTaskRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();

            var response =
                await _taskService.UpdateAsync(
                    userId,
                    id,
                    request);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete("tasks/{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();

            await _taskService.DeleteAsync(
                userId,
                id);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPatch("tasks/{id:guid}/status")]
    public async Task<ActionResult<TaskResponse>> UpdateStatus(
        Guid id,
        UpdateTaskStatusRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();

            var response =
                await _taskService.UpdateStatusAsync(
                    userId,
                    id,
                    request);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPatch("tasks/{id:guid}/priority")]
    public async Task<ActionResult<TaskResponse>> UpdatePriority(
        Guid id,
        UpdateTaskPriorityRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();

            var response =
                await _taskService.UpdatePriorityAsync(
                    userId,
                    id,
                    request);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
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