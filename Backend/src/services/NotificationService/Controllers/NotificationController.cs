using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NotificationService.DTOs;
using NotificationService.Services;

namespace NotificationService.Controllers;

/// <summary>
/// Exposes authenticated endpoints for viewing and updating user notifications.
/// </summary>
[ApiController]
[Route("api/notification")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationController(INotificationService service)
    {
        _service = service;
    }

    /// <summary>
    /// Extracts authenticated user id from JWT claims.
    /// </summary>
    private int GetAuthUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Builds a standardized API error payload for notification operations.
    /// </summary>
    private static NotificationErrorResponseDto BuildErrorResponse(string message)
        => new()
        {
            Message = message,
            ErrorCode = GetErrorCode(message)
        };

    /// <summary>
    /// Converts service failure messages into stable machine-readable error codes.
    /// </summary>
    private static string GetErrorCode(string message)
    {
        if (message.Contains("notification not found", StringComparison.OrdinalIgnoreCase))
            return "NOTIFICATION_NOT_FOUND";

        return "NOTIFICATION_VALIDATION_FAILED";
    }

    // ─── Notification Endpoints ───────────────────────────────────────────────

    /// <summary>
    /// Returns all notifications for the currently authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotificationErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll()
    {
        var authUserId = GetAuthUserId();
        var (success, data, message) = await _service.GetAllAsync(authUserId);

        if (!success)
            return BadRequest(BuildErrorResponse(message));

        return Ok(data);
    }

    /// <summary>
    /// Marks a specific notification as read for the authenticated user.
    /// </summary>
    [HttpPut("{notificationId:int}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotificationErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var authUserId = GetAuthUserId();
        var (success, message) = await _service.MarkAsReadAsync(authUserId, notificationId);

        if (!success)
            return NotFound(BuildErrorResponse(message));

        return Ok(new { message });
    }
}
