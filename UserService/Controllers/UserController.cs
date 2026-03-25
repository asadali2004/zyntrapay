using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.DTOs;
using UserService.Services;

namespace AuthService.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // Helper — extracts AuthUserId from the JWT token
    private int GetAuthUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("profile")]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileDto dto)
    {
        dto.AuthUserId = GetAuthUserId(); // always take from token, not request body
        var (success, message) = await _userService.CreateProfileAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var authUserId = GetAuthUserId();
        var (success, data, message) = await _userService.GetProfileAsync(authUserId);
        if (!success) return NotFound(new { message });
        return Ok(data);
    }

    [HttpPost("kyc")]
    public async Task<IActionResult> SubmitKyc([FromBody] SubmitKycDto dto)
    {
        dto.AuthUserId = GetAuthUserId();
        var (success, message) = await _userService.SubmitKycAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpGet("kyc")]
    public async Task<IActionResult> GetKycStatus()
    {
        var authUserId = GetAuthUserId();
        var (success, data, message) = await _userService.GetKycStatusAsync(authUserId);
        if (!success) return NotFound(new { message });
        return Ok(data);
    }

    // Admin-only endpoints called by AdminService internally
    [HttpGet("admin/kyc/pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingKycs()
    {
        var (success, data, message) = await _userService.GetPendingKycsAsync();
        if (!success) return BadRequest(new { message });
        return Ok(data);
    }

    [HttpPut("admin/kyc/{kycId}/review")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReviewKyc(int kycId, [FromBody] ReviewKycDto dto)
    {
        var (success, message) = await _userService.ReviewKycAsync(kycId, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}