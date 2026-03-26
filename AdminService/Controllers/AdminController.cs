using AdminService.DTOs;
using AdminService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminService.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    private int GetAuthUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("kyc/pending")]
    public async Task<IActionResult> GetPendingKycs()
    {
        var (success, data, message) = await _adminService.GetPendingKycsAsync();
        if (!success) return BadRequest(new { message });
        return Ok(data);
    }

    [HttpPut("kyc/{kycId}/review")]
    public async Task<IActionResult> ReviewKyc(int kycId, [FromBody] ReviewKycDto dto)
    {
        // Fetch KYC to get AuthUserId and email before reviewing
        var (success, kycs, _) = await _adminService.GetPendingKycsAsync();
        var kyc = kycs?.FirstOrDefault(k => k.Id == kycId);

        if (kyc != null)
        {
            dto.TargetAuthUserId = kyc.AuthUserId;
            
            // Email lookup — fetch all users and match by AuthUserId
            var (userSuccess, users, _) = await _adminService.GetAllUsersAsync();
            if (userSuccess)
            {
                dto.UserEmail = users?
                    .FirstOrDefault(u => u.Id == kyc.AuthUserId)?.Email ?? string.Empty;
            }
        }

        var (reviewSuccess, message) = await _adminService.ReviewKycAsync(GetAuthUserId(), kycId, dto);
        if (!reviewSuccess) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var (success, data, message) = await _adminService.GetAllUsersAsync();
        if (!success) return BadRequest(new { message });
        return Ok(data);
    }

    [HttpPut("users/{userId}/toggle")]
    public async Task<IActionResult> ToggleUserStatus(int userId)
    {
        var (success, message) = await _adminService.ToggleUserStatusAsync(GetAuthUserId(), userId);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var (success, data, message) = await _adminService.GetDashboardAsync();
        if (!success) return BadRequest(new { message });
        return Ok(data);
    }
}