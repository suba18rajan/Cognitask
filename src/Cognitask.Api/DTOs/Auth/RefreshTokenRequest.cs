using System.ComponentModel.DataAnnotations;

namespace Cognitask.Api.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}