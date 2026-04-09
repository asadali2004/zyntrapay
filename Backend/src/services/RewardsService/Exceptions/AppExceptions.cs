namespace RewardsService.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }

    protected AppException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}

public sealed class ValidationAppException(string message)
    : AppException(message, StatusCodes.Status400BadRequest);

public sealed class NotFoundAppException(string message)
    : AppException(message, StatusCodes.Status404NotFound);

public sealed class UnauthorizedAppException(string message)
    : AppException(message, StatusCodes.Status401Unauthorized);

public sealed class ConflictAppException(string message)
    : AppException(message, StatusCodes.Status409Conflict);
