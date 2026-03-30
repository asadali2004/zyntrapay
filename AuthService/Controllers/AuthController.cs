using AuthService.DTOs;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
    {
        var (success, data, message) = await _authService.GoogleLoginAsync(dto);
        if (!success) return Unauthorized(new { message });
        return Ok(data);
    }

    [HttpPut("update-phone")]
    [Authorize]
    public async Task<IActionResult> UpdatePhone([FromBody] UpdatePhoneDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, message) = await _authService.UpdatePhoneAsync(userId, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
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

    // Internal endpoint for AdminService — get user email by AuthUserId
    [HttpGet("users/{authUserId}/email")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserEmail(int authUserId)
    {
        var (success, data, message) = await _authService.GetUserEmailAsync(authUserId);
        if (!success) return NotFound(new { message });
        return Ok(data);
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequestDto dto)
    {
        var (success, message) = await _authService.SendOtpAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto dto)
    {
        var (success, message) = await _authService.VerifyOtpAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
        var (success, message) = await _authService.ForgotPasswordAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
    {
        var (success, message) = await _authService.ResetPasswordAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        var (success, data, message) = await _authService.RefreshTokenAsync(dto);
        if (!success) return Unauthorized(new { message });
        return Ok(data);
    }
}