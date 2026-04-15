using NotificationService.Exceptions;
using System.Net;
using System.Text.Json;

namespace NotificationService.Middleware;

/// <summary>
/// Captures unhandled exceptions and returns standardized JSON error responses.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception. Path: {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Maps runtime exceptions into API-friendly status codes and payloads.
    /// </summary>
    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var statusCode = ex switch
        {
            AppException appException => appException.StatusCode,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            message = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : ex.Message,
            errorCode = GetErrorCode(statusCode)
        }));
    }

    /// <summary>
    /// Converts response status codes into stable machine-readable error identifiers.
    /// </summary>
    private static string GetErrorCode(int statusCode)
        => statusCode switch
        {
            StatusCodes.Status400BadRequest => "VALIDATION_FAILED",
            StatusCodes.Status401Unauthorized => "UNAUTHORIZED",
            StatusCodes.Status404NotFound => "RESOURCE_NOT_FOUND",
            StatusCodes.Status409Conflict => "CONFLICT",
            _ => "INTERNAL_SERVER_ERROR"
        };
}
