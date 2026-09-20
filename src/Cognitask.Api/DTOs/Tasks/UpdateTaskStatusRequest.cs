using System.ComponentModel.DataAnnotations;

namespace Cognitask.Api.DTOs.Tasks;

public class UpdateTaskStatusRequest
{
    [Range(1, 3)]
    public int Status { get; set; }
}