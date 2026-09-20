using TaskPriority = Cognitask.Api.Enums.TaskPriority;
using TaskStatus = Cognitask.Api.Enums.TaskStatus;

namespace Cognitask.Api.Entities;

public class TaskItem
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TaskStatus Status { get; set; } = TaskStatus.Pending;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}