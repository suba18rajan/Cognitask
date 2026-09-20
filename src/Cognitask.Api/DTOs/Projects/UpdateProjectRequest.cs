using System.ComponentModel.DataAnnotations;

namespace Cognitask.Api.DTOs.Projects;

public class UpdateProjectRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
}