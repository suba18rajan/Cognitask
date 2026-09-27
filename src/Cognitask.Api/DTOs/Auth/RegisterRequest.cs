using System.ComponentModel.DataAnnotations;
using Cognitask.Api.Common.Validation;

namespace Cognitask.Api.DTOs.Auth;

public class RegisterRequest
{
    [Required]
    [NotEmpty]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [NotEmpty]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}