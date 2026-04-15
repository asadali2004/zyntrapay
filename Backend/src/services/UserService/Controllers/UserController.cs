using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers;

/// <summary>
/// Exposes profile and KYC endpoints for authenticated users and administrators.
/// </summary>
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

    /// <summary>
    /// Extracts authenticated user id from JWT claims.
    /// </summary>
    private int GetAuthUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Builds a standardized API error payload for user operations.
    /// </summary>
    private static UserErrorResponseDto BuildErrorResponse(string message)
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
        if (message.Contains("profile already exists", StringComparison.OrdinalIgnoreCase))
            return "PROFILE_ALREADY_EXISTS";

        if (message.Contains("profile not found", StringComparison.OrdinalIgnoreCase))
            return "PROFILE_NOT_FOUND";

        if (message.Contains("kyc already submitted", StringComparison.OrdinalIgnoreCase))
            return "KYC_ALREADY_SUBMITTED";

        if (message.Contains("no kyc submission found", StringComparison.OrdinalIgnoreCase))
            return "KYC_NOT_FOUND";

        if (message.Contains("status must be approved or rejected", StringComparison.OrdinalIgnoreCase))
            return "KYC_STATUS_INVALID";

        if (message.Contains("rejection reason is required", StringComparison.OrdinalIgnoreCase))
            return "KYC_REJECTION_REASON_REQUIRED";

        if (message.Contains("kyc not found", StringComparison.OrdinalIgnoreCase))
            return "KYC_NOT_FOUND";

        if (message.Contains("already approved", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("already rejected", StringComparison.OrdinalIgnoreCase))
            return "KYC_ALREADY_REVIEWED";

        return "USER_VALIDATION_FAILED";
    }

    // ─── Profile Endpoints ────────────────────────────────────────────────────

    /// <summary>
    /// Returns the profile of the currently authenticated user.
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var authUserId = GetAuthUserId();
        var (success, data, message) = await _userService.GetProfileAsync(authUserId);

        if (!success)
            return NotFound(BuildErrorResponse(message));

        return Ok(data);
    }

    /// <summary>
    /// Creates a profile for the currently authenticated user.
    /// </summary>
    [HttpPost("profile")]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileDto dto)
    {
        dto.AuthUserId = GetAuthUserId();
        var (success, message) = await _userService.CreateProfileAsync(dto);

        if (!success)
            return BadRequest(BuildErrorResponse(message));

        return Ok(new UserActionResponseDto { Message = message });
    }

    /// <summary>
    /// Returns lightweight identity info (name) for the authenticated user.
    /// Used internally by other services.
    /// </summary>
    [HttpGet("identity")]
    public async Task<IActionResult> GetIdentity()
    {
        var authUserId = GetAuthUserId();
        var (success, data, message) = await _userService.GetIdentityAsync(authUserId);

        if (!success)
            return NotFound(BuildErrorResponse(message));

        return Ok(data);
    }

    // ─── KYC Endpoints ───────────────────────────────────────────────────────

    /// <summary>
    /// Submits KYC documents for the currently authenticated user.
    /// </summary>
    [HttpPost("kyc")]
    public async Task<IActionResult> SubmitKyc([FromBody] SubmitKycDto dto)
    {
        dto.AuthUserId = GetAuthUserId();
        var (success, message) = await _userService.SubmitKycAsync(dto);

        if (!success)
            return BadRequest(BuildErrorResponse(message));

        return Ok(new UserActionResponseDto { Message = message });
    }

    /// <summary>
    /// Returns the KYC status of the currently authenticated user.
    /// </summary>
    [HttpGet("kyc")]
    public async Task<IActionResult> GetKycStatus()
    {
        var authUserId = GetAuthUserId();
        var (success, data, message) = await _userService.GetKycStatusAsync(authUserId);

        if (!success)
            return NotFound(BuildErrorResponse(message));

        return Ok(data);
    }

    // ─── Admin Endpoints ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns all pending KYC submissions. Admin only.
    /// </summary>
    [HttpGet("kyc/pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingKycs()
    {
        var (success, data, message) = await _userService.GetPendingKycsAsync();

        if (!success)
            return BadRequest(BuildErrorResponse(message));

        return Ok(data);
    }

    /// <summary>
    /// Reviews (approves or rejects) a KYC submission. Admin only.
    /// </summary>
    [HttpPut("kyc/{kycId}/review")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReviewKyc(int kycId, [FromBody] ReviewKycDto dto)
    {
        var (success, message) = await _userService.ReviewKycAsync(kycId, dto);

        if (!success)
            return BadRequest(BuildErrorResponse(message));

        return Ok(new UserActionResponseDto { Message = message });
    }
}
