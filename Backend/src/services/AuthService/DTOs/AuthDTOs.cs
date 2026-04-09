using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be exactly 10 digits.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$",
        ErrorMessage = "Password must contain at least one letter and one number.")]
    public string Password { get; set; } = string.Empty;
}

public class AdminRegisterRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be exactly 10 digits.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Admin secret key is required.")]
    public string AdminSecretKey { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool PhoneUpdateRequired { get; set; }
}

public class AuthActionResponseDto
{
    public string Message { get; set; } = string.Empty;
}

public class SignupStepResponseDto : AuthActionResponseDto
{
    public string NextStep { get; set; } = string.Empty;
}

public class AuthErrorResponseDto : AuthActionResponseDto
{
    public string ErrorCode { get; set; } = string.Empty;
}

public class UserSummaryDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendOtpRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;
}

public class VerifyOtpRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "OTP is required.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits.")]
    public string Otp { get; set; } = string.Empty;
}

public class GoogleLoginRequestDto
{
    [Required(ErrorMessage = "Google ID token is required.")]
    public string IdToken { get; set; } = string.Empty;
}

public class UpdatePhoneDto
{
    [Required]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be exactly 10 digits.")]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "OTP is required.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits.")]
    public string Otp { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$",
        ErrorMessage = "Password must contain at least one letter and one number.")]
    public string NewPassword { get; set; } = string.Empty;
}

public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Refresh token is required.")]
    public string RefreshToken { get; set; } = string.Empty;
}
