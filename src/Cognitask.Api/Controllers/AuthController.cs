using System.Security.Claims;
using Cognitask.Api.DTOs.Auth;
using Cognitask.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cognitask.Api.Controllers;

/// <summary>
/// Provides authentication and user account endpoints.
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
        
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new Cognitask user account.
    /// </summary>
    /// <param name="request">
    /// User registration details including name, email, and password.
    /// </param>
    /// <returns>
    /// Authentication tokens and the newly created user's information.
    /// </returns>
    /// <response code="200">User registered successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="409">A user with the specified email already exists.</response>

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request)
    {
        var response =
            await _authService.RegisterAsync(request);

        return Ok(response);
    }

    /// <summary>
    /// Authenticates a user and generates access and refresh tokens.
    /// </summary>
    /// <param name="request">
    /// User login credentials.
    /// </param>
    /// <returns>
    /// Authentication tokens and user information.
    /// </returns>
    /// <response code="200">Login successful.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Invalid email or password.</response>

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request)
    {
        var response =
            await _authService.LoginAsync(request);

        return Ok(response);
    }

    /// <summary>
    /// Generates a new access token and rotates the refresh token.
    /// </summary>
    /// <param name="request">
    /// Refresh token request.
    /// </param>
    /// <returns>
    /// New access and refresh tokens.
    /// </returns>
    /// <response code="200">Token refreshed successfully.</response>
    /// <response code="401">Refresh token is invalid, expired, or revoked.</response>

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken(
        RefreshTokenRequest request)
    {
        var response =
            await _authService.RefreshTokenAsync(
                request.RefreshToken);

        return Ok(response);
    }

    /// <summary>
    /// Revokes the user's refresh token and logs the user out.
    /// </summary>
    /// <param name="request">
    /// Refresh token to revoke.
    /// </param>
    /// <response code="204">Logout successful.</response>
    /// <response code="401">User is not authenticated.</response>

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        RefreshTokenRequest request)
    {
        var userId = GetCurrentUserId();

        await _authService.LogoutAsync(
            userId,
            request.RefreshToken);

        return Ok(new
        {
            message = "Logged out successfully."
        });
    }

    /// <summary>
    /// Returns information about the currently authenticated user.
    /// </summary>
    /// <returns>
    /// Current user information.
    /// </returns>
    /// <response code="200">User information returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">User was not found.</response>

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthResponse>> Me()
    {
        var userId = GetCurrentUserId();

        var response =
            await _authService.GetCurrentUserAsync(
                userId);

        return Ok(response);
    }

    private Guid GetCurrentUserId()
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
                userIdValue,
                out var userId))
        {
            throw new UnauthorizedAccessException(
                "Invalid user identity.");
        }

        return userId;
    }
}