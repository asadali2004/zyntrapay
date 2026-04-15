using AuthService.DTOs;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Controllers;

/// <summary>
/// Exposes authentication, registration, token, and account-management endpoints.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user account with the provided email and password.
    /// </summary>
    /// <param name="dto">Registration details including email and password.</param>
    /// <returns>Success message and next step indication.</returns>
    /// <response code="200">Returns the registration success message and next step.</response>
    /// <response code="400">If the registration details are invalid.</response>
    /// <response code="409">If the email is already registered.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(SignupStepResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status409Conflict)]
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
    /// <summary>
    /// Registers a new admin account with the provided details.
    /// </summary>
    /// <param name="dto">Admin registration details.</param>
    /// <returns>Success message.</returns>
    /// <response code="200">Returns the admin registration success message.</response>
    /// <response code="400">If the admin registration details are invalid.</response>
    /// <response code="409">If the admin email is already registered.</response>
    [HttpPost("register-admin")]
    [ProducesResponseType(typeof(AuthActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegisterRequestDto dto)
    {
        var (success, message) = await _authService.RegisterAdminAsync(dto);
        if (!success) return MapAuthFailure(message);
        return Ok(new AuthActionResponseDto { Message = message });
    }
    /// <summary>
    /// Authenticates a user and issues an access token.
    /// </summary>
    /// <param name="dto">Login credentials (email and password).</param>
    /// <returns>Access token for the authenticated user.</returns>
    /// <response code="200">Returns the access token and user details.</response>
    /// <response code="401">If the credentials are invalid.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var (success, data, message) = await _authService.LoginAsync(dto);
        if (!success) return Unauthorized(BuildErrorResponse(message));
        return Ok(data);
    }
    /// <summary>
    /// Authenticates a user via Google and issues an access token.
    /// </summary>
    /// <param name="dto">Google login credentials.</param>
    /// <returns>Access token for the authenticated user.</returns>
    /// <response code="200">Returns the access token and user details.</response>
    /// <response code="401">If the Google token is invalid or expired.</response>
    [HttpPost("google-login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
    {
        var (success, data, message) = await _authService.GoogleLoginAsync(dto);
        if (!success) return Unauthorized(BuildErrorResponse(message));
        return Ok(data);
    }
    /// <summary>
    /// Updates the authenticated user's phone number.
    /// </summary>
    /// <param name="dto">New phone number details.</param>
    /// <returns>Success message.</returns>
    /// <response code="200">Returns the success message.</response>
    /// <response code="400">If the phone number is invalid.</response>
    [HttpPut("update-phone")]
    [Authorize]
    [ProducesResponseType(typeof(AuthActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePhone([FromBody] UpdatePhoneDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, message) = await _authService.UpdatePhoneAsync(userId, dto);
        if (!success) return MapAuthFailure(message);
        return Ok(new AuthActionResponseDto { Message = message });
    }
    /// <summary>
    /// Retrieves all registered users (Admin only).
    /// </summary>
    /// <returns>List of all users.</returns>
    /// <response code="200">Returns the list of users.</response>
    [HttpGet("admin/users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<UserSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }
    /// <summary>
    /// Toggles the activation status of a user (Admin only).
    /// </summary>
    /// <param name="id">ID of the user.</param>
    /// <returns>Success message.</returns>
    /// <response code="200">Returns the success message.</response>
    /// <response code="404">If the user is not found.</response>
    [HttpPut("admin/users/{id}/toggle")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AuthActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        var (success, message) = await _authService.ToggleUserStatusAsync(id);
        if (!success) return NotFound(BuildErrorResponse(message));
        return Ok(new AuthActionResponseDto { Message = message });
    }
    /// <summary>
    /// Retrieves a user's email by their AuthUserId (Internal use).
    /// </summary>
    /// <param name="authUserId">The AuthUserId of the user.</param>
    /// <returns>The user's email.</returns>
    /// <response code="200">Returns the user's email.</response>
    /// <response code="404">If the user is not found.</response>
    [HttpGet("users/{authUserId}/email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserEmail(int authUserId)
    {
        var (success, data, message) = await _authService.GetUserEmailAsync(authUserId);
        if (!success) return NotFound(BuildErrorResponse(message));
        return Ok(data);
    }
    /// <summary>
    /// Looks up a user by their email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The user's information.</returns>
    /// <response code="200">Returns the user's information.</response>
    /// <response code="400">If the email is not provided.</response>
    /// <response code="404">If the user is not found.</response>
    [HttpGet("users/lookup")]
    [Authorize]
    [ProducesResponseType(typeof(UserSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LookupUser([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(BuildErrorResponse("Email is required."));
        }

        var (success, data, message) = await _authService.GetUserByEmailAsync(email);
        if (!success) return NotFound(BuildErrorResponse(message));
        return Ok(data);
    }
    /// <summary>
    /// Sends an OTP to the user's registered email for verification.
    /// </summary>
    /// <param name="dto">Details for sending the OTP.</param>
    /// <returns>Success message and next step indication.</returns>
    /// <response code="200">Returns the success message and next step.</response>
    /// <response code="400">If the request details are invalid.</response>
    /// <response code="409">If there is a conflict in the request.</response>
    /// <response code="503">If the OTP service is unavailable.</response>
    [HttpPost("send-otp")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ProducesResponseType(typeof(SignupStepResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status503ServiceUnavailable)]
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
    /// <summary>
    /// Requests to send an OTP for registration verification.
    /// </summary>
    /// <param name="dto">Details for sending the OTP.</param>
    /// <returns>Success message and next step indication.</returns>
    /// <response code="200">Returns the success message and next step.</response>
    /// <response code="400">If the request details are invalid.</response>
    /// <response code="409">If there is a conflict in the request.</response>
    /// <response code="503">If the OTP service is unavailable.</response>
    [HttpPost("register/request-otp")]
    [ProducesResponseType(typeof(SignupStepResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status503ServiceUnavailable)]
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
    /// <summary>
    /// Verifies the OTP entered by the user.
    /// </summary>
    /// <param name="dto">OTP verification details.</param>
    /// <returns>Success message and next step indication.</returns>
    /// <response code="200">Returns the success message and next step.</response>
    /// <response code="400">If the OTP is invalid.</response>
    /// <response code="409">If there is a conflict in the request.</response>
    [HttpPost("verify-otp")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ProducesResponseType(typeof(SignupStepResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status409Conflict)]
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
    /// <summary>
    /// Verifies the registration OTP and completes the registration process.
    /// </summary>
    /// <param name="dto">OTP verification details.</param>
    /// <returns>Success message and next step indication.</returns>
    /// <response code="200">Returns the success message and next step.</response>
    /// <response code="400">If the OTP is invalid.</response>
    /// <response code="409">If there is a conflict in the request.</response>
    [HttpPost("register/verify-otp")]
    [ProducesResponseType(typeof(SignupStepResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status409Conflict)]
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
    /// <summary>
    /// Initiates the password reset process by sending an OTP to the user's email.
    /// </summary>
    /// <param name="dto">Password reset request details.</param>
    /// <returns>Success message.</returns>
    /// <response code="200">Returns the success message.</response>
    /// <response code="503">If the OTP service is unavailable.</response>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(AuthActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
        var (success, message) = await _authService.ForgotPasswordAsync(dto);
        if (!success) return StatusCode(StatusCodes.Status503ServiceUnavailable, BuildErrorResponse(message));
        return Ok(new AuthActionResponseDto { Message = message });
    }
    /// <summary>
    /// Resets the user's password to a new value.
    /// </summary>
    /// <param name="dto">New password details.</param>
    /// <returns>Success message.</returns>
    /// <response code="200">Returns the success message.</response>
    /// <response code="400">If the new password is invalid.</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(AuthActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
    {
        var (success, message) = await _authService.ResetPasswordAsync(dto);
        if (!success) return MapAuthFailure(message);
        return Ok(new AuthActionResponseDto { Message = message });
    }
    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    /// <param name="dto">Refresh token request details.</param>
    /// <returns>New access token and user details.</returns>
    /// <response code="200">Returns the new access token.</response>
    /// <response code="401">If the refresh token is invalid or expired.</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        var (success, data, message) = await _authService.RefreshTokenAsync(dto);
        if (!success) return Unauthorized(BuildErrorResponse(message));
        return Ok(data);
    }
    /// <summary>
    /// Maps known authentication validation failures to standardized HTTP responses.
    /// </summary>
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
    /// <summary>
    /// Builds a uniform API error payload for authentication failures.
    /// </summary>
    private static AuthErrorResponseDto BuildErrorResponse(string message)
        => new()
        {
            Message = message,
            ErrorCode = GetErrorCode(message)
        };
    /// <summary>
    /// Converts service-layer failure messages into stable machine-readable error codes.
    /// </summary>
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