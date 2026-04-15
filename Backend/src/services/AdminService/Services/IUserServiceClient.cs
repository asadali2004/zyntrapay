using AdminService.DTOs;

namespace AdminService.Services;

/// <summary>
/// Defines HTTP client operations used to interact with UserService from AdminService.
/// </summary>
public interface IUserServiceClient
{
    Task<List<KycSubmissionDto>> GetPendingKycsAsync();
    Task<bool> ReviewKycAsync(int kycId, ReviewKycDto dto);
    Task<KycSubmissionDto?> GetKycByIdAsync(int kycId);
    Task<AdminUserProfileDto?> GetProfileAsync(int authUserId);
    Task<KycSubmissionDto?> GetKycByAuthUserIdAsync(int authUserId);
}