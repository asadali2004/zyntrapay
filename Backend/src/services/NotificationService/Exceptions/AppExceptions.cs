namespace NotificationService.Exceptions;

/// <summary>
/// Base exception type for predictable API error mapping with explicit HTTP status codes.
/// </summary>
public abstract class AppException : Exception
{
    public int StatusCode { get; }

    protected AppException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}

/// <summary>
/// Represents request validation failures.
/// </summary>
public sealed class ValidationAppException(string message)
    : AppException(message, StatusCodes.Status400BadRequest);

/// <summary>
/// Represents missing-resource failures.
/// </summary>
public sealed class NotFoundAppException(string message)
    : AppException(message, StatusCodes.Status404NotFound);

/// <summary>
/// Represents authentication or authorization failures.
/// </summary>
public sealed class UnauthorizedAppException(string message)
    : AppException(message, StatusCodes.Status401Unauthorized);

/// <summary>
/// Represents conflicting state or duplicate-resource failures.
/// </summary>
public sealed class ConflictAppException(string message)
    : AppException(message, StatusCodes.Status409Conflict);
