namespace NotificationService.DTOs;

/// <summary>
/// Represents a user notification returned to API clients.
/// </summary>
public class NotificationDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the notification.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the notification.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message content of the notification.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the notification has been read.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Gets or sets the creation date and time of the notification.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Represents a generic successful notification-operation response.
/// </summary>
public class NotificationActionResponseDto
{
    /// <summary>
    /// Gets or sets the success message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Represents a standardized error payload for notification endpoints.
/// </summary>
public class NotificationErrorResponseDto
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;
}