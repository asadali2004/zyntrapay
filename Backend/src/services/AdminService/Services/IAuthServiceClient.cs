using AdminService.DTOs;

namespace AdminService.Services;

public interface IAuthServiceClient
{
    Task<List<UserSummaryDto>> GetAllUsersAsync();
    Task<bool> ToggleUserStatusAsync(int userId);
    Task<string?> GetUserEmailAsync(int authUserId);
}