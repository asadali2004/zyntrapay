using UserService.Models;

namespace UserService.Repositories;

/// <summary>
/// Defines persistence operations for user profile and KYC entities.
/// </summary>
public interface IUserRepository
{
    Task<UserProfile?> GetProfileByAuthUserIdAsync(int authUserId);
    Task<bool> ProfileExistsAsync(int authUserId);
    Task AddProfileAsync(UserProfile profile);

    Task<KycSubmission?> GetKycByAuthUserIdAsync(int authUserId);
    Task<bool> KycExistsAsync(int authUserId);
    Task AddKycAsync(KycSubmission kyc);

    Task SaveChangesAsync();

    Task<KycSubmission?> GetKycByIdAsync(int kycId);
    Task<List<KycSubmission>> GetPendingKycsAsync();
}