using TaskPriority = Cognitask.Api.Enums.TaskPriority;
using TaskStatus = Cognitask.Api.Enums.TaskStatus;

namespace Cognitask.Api.DTOs.Tasks;

public class TaskResponse
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TaskStatus Status { get; set; }

    public TaskPriority Priority { get; set; }

    public Guid ProjectId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}