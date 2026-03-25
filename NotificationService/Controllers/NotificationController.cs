using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
    public async Task<IActionResult> GetAll()
    {
        var (success, data, message) = await _service.GetAllAsync(GetAuthUserId());
        if (!success) return NotFound(new { message });
        return Ok(data);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var (success, message) = await _service.MarkAsReadAsync(GetAuthUserId(), id);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }
}