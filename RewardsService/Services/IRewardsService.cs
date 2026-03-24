using RewardsService.DTOs;

namespace RewardsService.Services;

public interface IRewardsService
{
    Task<(bool Success, RewardSummaryDto? Data, string Message)> GetSummaryAsync(int authUserId);
    Task<(bool Success, List<CatalogItemDto>? Data, string Message)> GetCatalogAsync();
    Task<(bool Success, string Message)> RedeemAsync(int authUserId, RedeemRequestDto dto);
    Task<(bool Success, List<RedemptionHistoryDto>? Data, string Message)> GetHistoryAsync(int authUserId);
    Task AwardPointsAsync(int authUserId, decimal amount); // called by RabbitMQ consumer
}