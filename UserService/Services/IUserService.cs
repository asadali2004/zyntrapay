using UserService.DTOs;

namespace UserService.Services;

public interface IUserService
{
    Task<(bool Success, string Message)> CreateProfileAsync(CreateProfileDto dto);
    Task<(bool Success, ProfileResponseDto? Data, string Message)> GetProfileAsync(int authUserId);
    Task<(bool Success, string Message)> SubmitKycAsync(SubmitKycDto dto);
    Task<(bool Success, KycResponseDto? Data, string Message)> GetKycStatusAsync(int authUserId);
    Task<(bool Success, List<KycResponseDto>? Data, string Message)> GetPendingKycsAsync();
    Task<(bool Success, string Message)> ReviewKycAsync(int kycId, ReviewKycDto dto);
}