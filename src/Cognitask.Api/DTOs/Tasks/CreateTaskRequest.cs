using System.ComponentModel.DataAnnotations;
using Cognitask.Api.Common.Validation;

namespace Cognitask.Api.DTOs.Tasks;

public class CreateTaskRequest
{
    [Required]
    [NotEmpty]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(1, 3)]
    public int Priority { get; set; } = 2;
}