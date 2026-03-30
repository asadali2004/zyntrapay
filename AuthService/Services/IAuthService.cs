using AuthService.DTOs;

namespace AuthService.Services;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto dto);
    Task<(bool Success, string Message)> RegisterAdminAsync(AdminRegisterRequestDto dto);
    Task<(bool Success, AuthResponseDto? Data, string Message)> LoginAsync(LoginRequestDto dto);
    Task<(bool Success, AuthResponseDto? Data, string Message)> GoogleLoginAsync(GoogleLoginRequestDto dto);
    Task<List<UserSummaryDto>> GetAllUsersAsync();
    Task<(bool Success, string Message)> ToggleUserStatusAsync(int userId);
    Task<(bool Success, string? Data, string Message)> GetUserEmailAsync(int authUserId);
    Task<(bool Success, string Message)> SendOtpAsync(SendOtpRequestDto dto);
    Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpRequestDto dto);
    Task<(bool Success, string Message)> UpdatePhoneAsync(int userId, UpdatePhoneDto dto);
    Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequestDto dto);
    Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequestDto dto);
    Task<(bool Success, AuthResponseDto? Data, string Message)> RefreshTokenAsync(RefreshTokenRequestDto dto);
}