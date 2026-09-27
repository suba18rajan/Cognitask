using System.ComponentModel.DataAnnotations;
using Cognitask.Api.Common.Validation;

namespace Cognitask.Api.DTOs.Auth;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [NotEmpty]
    public string Password { get; set; } = string.Empty;
}