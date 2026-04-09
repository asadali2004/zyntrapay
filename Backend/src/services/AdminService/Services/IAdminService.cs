using AdminService.DTOs;

namespace AdminService.Services;

public interface IAdminService
{
    Task<(bool Success, List<KycSubmissionDto>? Data, string Message)> GetPendingKycsAsync();
    Task<(bool Success, string Message)> ReviewKycAsync(int adminId, int kycId, ReviewKycDto dto);
    Task<(bool Success, List<UserSummaryDto>? Data, string Message)> GetAllUsersAsync();
    Task<(bool Success, string Message)> ToggleUserStatusAsync(int adminId, int userId);
    Task<(bool Success, DashboardDto? Data, string Message)> GetDashboardAsync();
}