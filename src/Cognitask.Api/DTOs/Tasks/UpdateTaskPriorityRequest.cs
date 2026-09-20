using System.ComponentModel.DataAnnotations;

namespace Cognitask.Api.DTOs.Tasks;

public class UpdateTaskPriorityRequest
{
    [Range(1, 3)]
    public int Priority { get; set; }
}