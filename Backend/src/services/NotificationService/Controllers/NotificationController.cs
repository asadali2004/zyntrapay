using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NotificationService.DTOs;
using NotificationService.Services;

namespace NotificationService.Controllers;

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

    private int GetAuthUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotificationErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll()
    {
        var (success, data, message) = await _service.GetAllAsync(GetAuthUserId());
        if (!success) return NotFound(BuildErrorResponse(message));
        return Ok(data);
    }

    [HttpPut("{id}/read")]
    [ProducesResponseType(typeof(NotificationActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotificationErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var (success, message) = await _service.MarkAsReadAsync(GetAuthUserId(), id);
        if (!success) return NotFound(BuildErrorResponse(message));
        return Ok(new NotificationActionResponseDto { Message = message });
    }

    private static NotificationErrorResponseDto BuildErrorResponse(string message)
        => new()
        {
            Message = message,
            ErrorCode = GetErrorCode(message)
        };

    private static string GetErrorCode(string message)
    {
        if (message.Contains("notification not found", StringComparison.OrdinalIgnoreCase))
            return "NOTIFICATION_NOT_FOUND";

        return "NOTIFICATION_VALIDATION_FAILED";
    }
}
