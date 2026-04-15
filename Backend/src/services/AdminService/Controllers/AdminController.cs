using AdminService.DTOs;
using AdminService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminService.Controllers;

/// <summary>
/// Exposes admin-only endpoints for KYC moderation, user management, dashboard, and audit views.
/// </summary>
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

    /// <summary>
    /// Extracts authenticated admin user id from JWT claims.
    /// </summary>
    private int GetAuthUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Builds a standardized API error payload for admin operations.
    /// </summary>
    private static AdminErrorResponseDto BuildErrorResponse(string message)
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
        if (message.Contains("kyc not found", StringComparison.OrdinalIgnoreCase))
            return "KYC_NOT_FOUND";

        if (message.Contains("already", StringComparison.OrdinalIgnoreCase))
            return "ADMIN_CONFLICT";

        if (message.Contains("user", StringComparison.OrdinalIgnoreCase) &&
            message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return "USER_NOT_FOUND";

        return "ADMIN_VALIDATION_FAILED";
    }

    [HttpGet("kyc/pending")]
    [ProducesResponseType(typeof(List<KycSubmissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AdminErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPendingKycs()
    {
        var (success, data, message) = await _adminService.GetPendingKycsAsync();
        if (!success) return BadRequest(BuildErrorResponse(message));
        return Ok(data);
    }

    [HttpPut("kyc/{kycId}/review")]
    [ProducesResponseType(typeof(AdminActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AdminErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReviewKyc(int kycId, [FromBody] ReviewKycDto dto)
    {
        var (success, kycs, _) = await _adminService.GetPendingKycsAsync();
        var kyc = kycs?.FirstOrDefault(k => k.Id == kycId);

        if (kyc != null)
        {
            dto.TargetAuthUserId = kyc.AuthUserId;

            var (userSuccess, users, _) = await _adminService.GetAllUsersAsync();
            if (userSuccess)
            {
                dto.UserEmail = users?
                    .FirstOrDefault(u => u.Id == kyc.AuthUserId)?.Email ?? string.Empty;
            }
        }

        var (reviewSuccess, message) = await _adminService.ReviewKycAsync(GetAuthUserId(), kycId, dto);
        if (!reviewSuccess) return BadRequest(BuildErrorResponse(message));
        return Ok(new AdminActionResponseDto { Message = message });
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(List<UserSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AdminErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllUsers()
    {
        var (success, data, message) = await _adminService.GetAllUsersAsync();
        if (!success) return BadRequest(BuildErrorResponse(message));
        return Ok(data);
    }

    [HttpPut("users/{userId}/toggle")]
    [ProducesResponseType(typeof(AdminActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AdminErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ToggleUserStatus(int userId)
    {
        var (success, message) = await _adminService.ToggleUserStatusAsync(GetAuthUserId(), userId);
        if (!success) return BadRequest(BuildErrorResponse(message));
        return Ok(new AdminActionResponseDto { Message = message });
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AdminErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDashboard()
    {
        var (success, data, message) = await _adminService.GetDashboardAsync();
        if (!success) return BadRequest(BuildErrorResponse(message));
        return Ok(data);
    }

    [HttpGet("actions/recent")]
    [ProducesResponseType(typeof(List<AdminActionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AdminErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRecentActions([FromQuery] int take = 10)
    {
        var (success, data, message) = await _adminService.GetRecentActionsAsync(take);
        if (!success) return BadRequest(BuildErrorResponse(message));
        return Ok(data);
    }

    [HttpGet("users/{authUserId}/details")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AdminErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserDetails(int authUserId)
    {
        var (success, profile, kyc, message) = await _adminService.GetUserDetailsAsync(authUserId);
        if (!success) return NotFound(BuildErrorResponse(message));
        return Ok(new
        {
            profile,
            kyc
        });
    }
}

