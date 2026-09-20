using System.ComponentModel.DataAnnotations;

namespace Cognitask.Api.DTOs.Tasks;

public class CreateTaskRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public int Priority { get; set; } = 2;
}