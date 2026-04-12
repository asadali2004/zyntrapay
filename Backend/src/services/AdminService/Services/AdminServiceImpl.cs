using AdminService.DTOs;
using AdminService.Models;
using AdminService.Repositories;
using Shared.Events;

namespace AdminService.Services;

public class AdminServiceImpl : IAdminService
{
    private readonly IAdminRepository _repo;
    private readonly IUserServiceClient _userClient;
    private readonly IAuthServiceClient _authClient;
    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<AdminServiceImpl> _logger;

    public AdminServiceImpl(
        IAdminRepository repo,
        IUserServiceClient userClient,
        IAuthServiceClient authClient,
        IRabbitMqPublisher publisher,
        ILogger<AdminServiceImpl> logger)
    {
        _repo = repo;
        _userClient = userClient;
        _authClient = authClient;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<(bool Success, List<KycSubmissionDto>? Data, string Message)> GetPendingKycsAsync()
    {
        var list = await _userClient.GetPendingKycsAsync();
        return (true, list, "Pending KYC list fetched.");
    }

    public async Task<(bool Success, string Message)> ReviewKycAsync(
        int adminId, int kycId, ReviewKycDto dto)
    {
        _logger.LogInformation("Admin {AdminId} reviewing KYC {KycId} as {Status}",
            adminId, kycId, dto.Status);

        if (dto.Status != "Approved" && dto.Status != "Rejected")
            return (false, "Status must be Approved or Rejected.");

        if (dto.Status == "Rejected" && string.IsNullOrWhiteSpace(dto.RejectionReason))
            return (false, "Rejection reason is required when rejecting KYC.");

        var success = await _userClient.ReviewKycAsync(kycId, dto);
        if (!success)
            return (false, "Failed to update KYC status.");

        // KYC review is already committed downstream, so publish failure should not undo the review.
        var published = _publisher.Publish(new KycStatusChangedEvent
        {
            AuthUserId = dto.TargetAuthUserId,
            UserEmail = dto.UserEmail,
            Status = dto.Status,
            Reason = dto.RejectionReason
        });

        // Log audit trail
        await _repo.AddActionAsync(new AdminAction
        {
            AdminAuthUserId = adminId,
            ActionType = dto.Status == "Approved" ? "KYC_APPROVED" : "KYC_REJECTED",
            TargetUserId = kycId,
            Remarks = dto.RejectionReason,
            PerformedAt = DateTime.UtcNow
        });
        await _repo.SaveChangesAsync();

        if (!published)
        {
            _logger.LogWarning("KYC status changed event publish failed for target user {TargetAuthUserId}", dto.TargetAuthUserId);
            return (true, $"KYC {dto.Status} successfully. Notification may be delayed.");
        }

        return (true, $"KYC {dto.Status} successfully.");
    }

    public async Task<(bool Success, List<UserSummaryDto>? Data, string Message)> GetAllUsersAsync()
    {
        var users = await _authClient.GetAllUsersAsync();
        return (true, users, "Users fetched.");
    }

    public async Task<(bool Success, string Message)> ToggleUserStatusAsync(int adminId, int userId)
    {
        _logger.LogInformation("Admin {AdminId} toggling status for UserId {UserId}",
            adminId, userId);

        var success = await _authClient.ToggleUserStatusAsync(userId);
        if (!success)
            return (false, "Failed to toggle user status.");

        await _repo.AddActionAsync(new AdminAction
        {
            AdminAuthUserId = adminId,
            ActionType = "USER_STATUS_TOGGLED",
            TargetUserId = userId,
            PerformedAt = DateTime.UtcNow
        });
        await _repo.SaveChangesAsync();

        return (true, "User status toggled successfully.");
    }

    public async Task<(bool Success, DashboardDto? Data, string Message)> GetDashboardAsync()
    {
        var users = await _authClient.GetAllUsersAsync();
        var kycs = await _userClient.GetPendingKycsAsync();

        var dashboard = new DashboardDto
        {
            TotalUsers = users.Count,
            ActiveUsers = users.Count(u => u.IsActive),
            PendingKyc = kycs.Count
        };

        return (true, dashboard, "Dashboard fetched.");
    }

    public async Task<(bool Success, List<AdminActionDto>? Data, string Message)> GetRecentActionsAsync(int take)
    {
        var actions = await _repo.GetRecentActionsAsync(Math.Clamp(take, 1, 50));
        var result = actions.Select(action => new AdminActionDto
        {
            Id = action.Id,
            AdminAuthUserId = action.AdminAuthUserId,
            ActionType = action.ActionType,
            TargetUserId = action.TargetUserId,
            Remarks = action.Remarks,
            PerformedAt = action.PerformedAt
        }).ToList();

        return (true, result, "Recent admin actions fetched.");
    }

    public async Task<(bool Success, AdminUserProfileDto? Profile, KycSubmissionDto? Kyc, string Message)> GetUserDetailsAsync(int authUserId)
    {
        var profile = await _userClient.GetProfileAsync(authUserId);
        var kyc = await _userClient.GetKycByAuthUserIdAsync(authUserId);

        if (profile == null && kyc == null)
            return (false, null, null, "User details not found.");

        return (true, profile, kyc, "User details fetched.");
    }
}
