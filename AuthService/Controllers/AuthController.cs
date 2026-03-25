using AuthService.DTOs;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var (success, message) = await _authService.RegisterAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpPost("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegisterRequestDto dto)
    {
        var (success, message) = await _authService.RegisterAdminAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var (success, data, message) = await _authService.LoginAsync(dto);
        if (!success) return Unauthorized(new { message });
        return Ok(data);
    }

    // Admin-only endpoints called by AdminService internally
    [HttpGet("admin/users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPut("admin/users/{id}/toggle")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        var (success, message) = await _authService.ToggleUserStatusAsync(id);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }
}