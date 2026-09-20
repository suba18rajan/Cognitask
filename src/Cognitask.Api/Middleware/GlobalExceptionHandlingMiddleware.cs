using System.Text.Json;

namespace Cognitask.Api.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unhandled exception occurred.");

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,

            ArgumentException => StatusCodes.Status400BadRequest,

            UnauthorizedAccessException =>
                StatusCodes.Status401Unauthorized,

            InvalidOperationException =>
                StatusCodes.Status409Conflict,

            _ => StatusCodes.Status500InternalServerError
        };

        var message = exception switch
        {
            KeyNotFoundException =>
                exception.Message,

            ArgumentException =>
                exception.Message,

            UnauthorizedAccessException =>
                exception.Message,

            InvalidOperationException =>
                exception.Message,

            _ => "An unexpected error occurred."
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            statusCode,
            message
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}