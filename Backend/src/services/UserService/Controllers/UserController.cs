using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers;

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

    private int GetAuthUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("profile")]
    [ProducesResponseType(typeof(UserActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileDto dto)
    {
        dto.AuthUserId = GetAuthUserId();
        var (success, message) = await _userService.CreateProfileAsync(dto);
        if (!success) return BadRequest(BuildErrorResponse(message));
        return Ok(new UserActionResponseDto { Message = message });
    }

    [HttpGet("profile")]
    [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var authUserId = GetAuthUserId();
        var (success, data, message) = await _userService.GetProfileAsync(authUserId);
        if (!success) return NotFound(BuildErrorResponse(message));
        return Ok(data);
    }

    [HttpPost("kyc")]
    [ProducesResponseType(typeof(UserActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitKyc([FromBody] SubmitKycDto dto)
    {
        dto.AuthUserId = GetAuthUserId();
        var (success, message) = await _userService.SubmitKycAsync(dto);
        if (!success) return BadRequest(BuildErrorResponse(message));
        return Ok(new UserActionResponseDto { Message = message });
    }

    [HttpGet("kyc")]
    [ProducesResponseType(typeof(KycResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetKycStatus()
    {
        var authUserId = GetAuthUserId();
        var (success, data, message) = await _userService.GetKycStatusAsync(authUserId);
        if (!success) return NotFound(BuildErrorResponse(message));
        return Ok(data);
    }

    [HttpGet("admin/kyc/pending")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<KycResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPendingKycs()
    {
        var (success, data, message) = await _userService.GetPendingKycsAsync();
        if (!success) return BadRequest(BuildErrorResponse(message));
        return Ok(data);
    }

    [HttpPut("admin/kyc/{kycId}/review")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReviewKyc(int kycId, [FromBody] ReviewKycDto dto)
    {
        var (success, message) = await _userService.ReviewKycAsync(kycId, dto);
        if (!success) return BadRequest(BuildErrorResponse(message));
        return Ok(new UserActionResponseDto { Message = message });
    }

    private static UserErrorResponseDto BuildErrorResponse(string message)
        => new()
        {
            Message = message,
            ErrorCode = GetErrorCode(message)
        };

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
}
