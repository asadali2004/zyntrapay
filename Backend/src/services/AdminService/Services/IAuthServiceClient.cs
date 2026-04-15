using AdminService.DTOs;

namespace AdminService.Services;

/// <summary>
/// Defines HTTP client operations used to interact with AuthService from AdminService.
/// </summary>
public interface IAuthServiceClient
{
    Task<List<UserSummaryDto>> GetAllUsersAsync();
    Task<bool> ToggleUserStatusAsync(int userId);
    Task<string?> GetUserEmailAsync(int authUserId);
}