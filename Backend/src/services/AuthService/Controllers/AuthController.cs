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
        if (!success) return MapAuthFailure(message);
        return Ok(new SignupStepResponseDto
        {
            Message = message,
            NextStep = "login"
        });
    }

    [HttpPost("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegisterRequestDto dto)
    {
        var (success, message) = await _authService.RegisterAdminAsync(dto);
        if (!success) return MapAuthFailure(message);
        return Ok(new AuthActionResponseDto { Message = message });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var (success, data, message) = await _authService.LoginAsync(dto);
        if (!success) return Unauthorized(BuildErrorResponse(message));
        return Ok(data);
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
    {
        var (success, data, message) = await _authService.GoogleLoginAsync(dto);
        if (!success) return Unauthorized(BuildErrorResponse(message));
        return Ok(data);
    }

    [HttpPut("update-phone")]
    [Authorize]
    public async Task<IActionResult> UpdatePhone([FromBody] UpdatePhoneDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, message) = await _authService.UpdatePhoneAsync(userId, dto);
        if (!success) return MapAuthFailure(message);
        return Ok(new AuthActionResponseDto { Message = message });
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
        if (!success) return NotFound(BuildErrorResponse(message));
        return Ok(new AuthActionResponseDto { Message = message });
    }

    // Internal endpoint for AdminService — get user email by AuthUserId
    [HttpGet("users/{authUserId}/email")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserEmail(int authUserId)
    {
        var (success, data, message) = await _authService.GetUserEmailAsync(authUserId);
        if (!success) return NotFound(BuildErrorResponse(message));
        return Ok(data);
    }

    [HttpPost("send-otp")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequestDto dto)
    {
        var (success, message) = await _authService.SendOtpAsync(dto);
        if (!success) return MapAuthFailure(message);
        return Ok(new SignupStepResponseDto
        {
            Message = message,
            NextStep = "verify-otp"
        });
    }

    [HttpPost("register/request-otp")]
    public async Task<IActionResult> RequestRegistrationOtp([FromBody] SendOtpRequestDto dto)
    {
        var (success, message) = await _authService.SendOtpAsync(dto);
        if (!success) return MapAuthFailure(message);
        return Ok(new SignupStepResponseDto
        {
            Message = message,
            NextStep = "verify-otp"
        });
    }

    [HttpPost("verify-otp")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto dto)
    {
        var (success, message) = await _authService.VerifyOtpAsync(dto);
        if (!success) return MapAuthFailure(message);
        return Ok(new SignupStepResponseDto
        {
            Message = message,
            NextStep = "complete-registration"
        });
    }

    [HttpPost("register/verify-otp")]
    public async Task<IActionResult> VerifyRegistrationOtp([FromBody] VerifyOtpRequestDto dto)
    {
        var (success, message) = await _authService.VerifyOtpAsync(dto);
        if (!success) return MapAuthFailure(message);
        return Ok(new SignupStepResponseDto
        {
            Message = message,
            NextStep = "complete-registration"
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
        var (success, message) = await _authService.ForgotPasswordAsync(dto);
        if (!success) return StatusCode(StatusCodes.Status503ServiceUnavailable, BuildErrorResponse(message));
        return Ok(new AuthActionResponseDto { Message = message });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
    {
        var (success, message) = await _authService.ResetPasswordAsync(dto);
        if (!success) return MapAuthFailure(message);
        return Ok(new AuthActionResponseDto { Message = message });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        var (success, data, message) = await _authService.RefreshTokenAsync(dto);
        if (!success) return Unauthorized(BuildErrorResponse(message));
        return Ok(data);
    }

    private IActionResult MapAuthFailure(string message)
    {
        var error = BuildErrorResponse(message);

        if (message.Contains("already registered", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("already in use", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(error);
        }

        if (message.Contains("unable to send otp", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, error);
        }

        if (message.Contains("user not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(error);
        }

        return BadRequest(error);
    }

    private static AuthErrorResponseDto BuildErrorResponse(string message)
        => new()
        {
            Message = message,
            ErrorCode = GetErrorCode(message)
        };

    private static string GetErrorCode(string message)
    {
        if (message.Contains("already registered", StringComparison.OrdinalIgnoreCase))
            return "EMAIL_ALREADY_REGISTERED";

        if (message.Contains("phone number already", StringComparison.OrdinalIgnoreCase))
            return "PHONE_ALREADY_REGISTERED";

        if (message.Contains("not verified", StringComparison.OrdinalIgnoreCase))
            return "EMAIL_NOT_VERIFIED";

        if (message.Contains("otp expired", StringComparison.OrdinalIgnoreCase))
            return "OTP_EXPIRED";

        if (message.Contains("invalid otp", StringComparison.OrdinalIgnoreCase))
            return "OTP_INVALID";

        if (message.Contains("unable to send otp", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("unable to send reset otp", StringComparison.OrdinalIgnoreCase))
            return "OTP_DELIVERY_FAILED";

        if (message.Contains("invalid email or password", StringComparison.OrdinalIgnoreCase))
            return "INVALID_CREDENTIALS";

        if (message.Contains("deactivated", StringComparison.OrdinalIgnoreCase))
            return "ACCOUNT_DEACTIVATED";

        if (message.Contains("invalid or expired refresh token", StringComparison.OrdinalIgnoreCase))
            return "REFRESH_TOKEN_INVALID";

        if (message.Contains("user not found", StringComparison.OrdinalIgnoreCase))
            return "USER_NOT_FOUND";

        return "AUTH_VALIDATION_FAILED";
    }
}
