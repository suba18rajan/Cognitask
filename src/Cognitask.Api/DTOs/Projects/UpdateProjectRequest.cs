using System.ComponentModel.DataAnnotations;
using Cognitask.Api.Common.Validation;

namespace Cognitask.Api.DTOs.Projects;

public class UpdateProjectRequest
{
    [Required]
    [NotEmpty]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
}