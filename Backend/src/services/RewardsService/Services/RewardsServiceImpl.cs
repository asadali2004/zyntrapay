using Microsoft.Extensions.Caching.Memory;
using RewardsService.DTOs;
using RewardsService.Helpers;
using RewardsService.Models;
using RewardsService.Repositories;
using Shared.Events;

namespace RewardsService.Services;

public class RewardsServiceImpl : IRewardsService
{
    private readonly IRewardsRepository _repo;
    private readonly ILogger<RewardsServiceImpl> _logger;
    private readonly IMemoryCache _cache;
    private readonly IRabbitMqPublisher _publisher;

    private const string CatalogCacheKey = "rewards_catalog";

    public RewardsServiceImpl(
        IRewardsRepository repo,
        ILogger<RewardsServiceImpl> logger,
        IMemoryCache cache,
        IRabbitMqPublisher publisher)
    {
        _repo = repo;
        _logger = logger;
        _cache = cache;
        _publisher = publisher;
    }

    public async Task<(bool Success, RewardSummaryDto? Data, string Message)> GetSummaryAsync(int authUserId)
    {
        var account = await _repo.GetAccountByAuthUserIdAsync(authUserId);
        if (account == null)
            return (false, null, "Rewards account not found.");

        return (true, new RewardSummaryDto
        {
            AuthUserId = account.AuthUserId,
            TotalPoints = account.TotalPoints,
            Tier = account.Tier
        }, "Summary fetched.");
    }

    public async Task<(bool Success, List<CatalogItemDto>? Data, string Message)> GetCatalogAsync()
    {
        if (_cache.TryGetValue(CatalogCacheKey, out List<CatalogItemDto>? cached))
        {
            _logger.LogInformation("Catalog served from cache");
            return (true, cached, "Catalog fetched from cache.");
        }

        var items = await _repo.GetActiveCatalogAsync();

        var result = items.Select(c => new CatalogItemDto
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            PointsCost = c.PointsCost,
            Stock = c.Stock
        }).ToList();

        _cache.Set(CatalogCacheKey, result, TimeSpan.FromMinutes(5));

        _logger.LogInformation("Catalog fetched from DB and cached");
        return (true, result, "Catalog fetched.");
    }

    public async Task<(bool Success, string Message)> RedeemAsync(int authUserId, RedeemRequestDto dto)
    {
        _logger.LogInformation("Redeem request from AuthUserId: {Id} for CatalogItem: {ItemId}",
            authUserId, dto.RewardCatalogId);

        var account = await _repo.GetAccountByAuthUserIdAsync(authUserId);
        if (account == null)
            return (false, "Rewards account not found.");

        var item = await _repo.GetCatalogItemByIdAsync(dto.RewardCatalogId);
        if (item == null || !item.IsActive)
            return (false, "Reward item not found or inactive.");

        if (item.Stock == 0)
            return (false, "This reward is out of stock.");

        if (account.TotalPoints < item.PointsCost)
            return (false, $"Insufficient points. You have {account.TotalPoints} points but need {item.PointsCost}.");

        // Deduct points
        account.TotalPoints -= item.PointsCost;
        account.Tier = TierHelper.CalculateTier(account.TotalPoints);

        // Reduce stock if not unlimited
        if (item.Stock > 0)
            item.Stock -= 1;

        // Record redemption
        await _repo.AddRedemptionAsync(new Redemption
        {
            AuthUserId = authUserId,
            RewardCatalogId = item.Id,
            PointsSpent = item.PointsCost,
            RedeemedAt = DateTime.UtcNow
        });

        await _repo.SaveChangesAsync();
        _cache.Remove(CatalogCacheKey);

        _logger.LogInformation("Redemption successful for AuthUserId: {Id}, Points spent: {Points}",
            authUserId, item.PointsCost);

        return (true, $"Successfully redeemed '{item.Title}'. Remaining points: {account.TotalPoints}.");
    }

    public async Task<(bool Success, List<RedemptionHistoryDto>? Data, string Message)> GetHistoryAsync(int authUserId)
    {
        var redemptions = await _repo.GetRedemptionsByAuthUserIdAsync(authUserId);

        var result = redemptions.Select(r => new RedemptionHistoryDto
        {
            Id = r.Id,
            RewardTitle = r.RewardCatalog?.Title ?? "Unknown",
            PointsSpent = r.PointsSpent,
            RedeemedAt = r.RedeemedAt
        }).ToList();

        return (true, result, "History fetched.");
    }

    public async Task AwardPointsAsync(int authUserId, decimal amount)
    {
        _logger.LogInformation("Awarding points to AuthUserId: {Id} for amount: {Amount}",
            authUserId, amount);

        var pointsToAward = TierHelper.CalculatePointsToEarn(amount);
        if (pointsToAward <= 0) return;

        var account = await _repo.GetAccountByAuthUserIdAsync(authUserId);

        if (account == null)
        {
            // Auto-create rewards account on first top-up
            account = new RewardAccount
            {
                AuthUserId = authUserId,
                TotalPoints = pointsToAward,
                Tier = TierHelper.CalculateTier(pointsToAward),
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAccountAsync(account);
        }
        else
        {
            account.TotalPoints += pointsToAward;
            account.Tier = TierHelper.CalculateTier(account.TotalPoints);
        }

        await _repo.SaveChangesAsync();

        var published = _publisher.Publish(new PointsAwardedEvent
        {
            AuthUserId = authUserId,
            PointsEarned = pointsToAward,
            TotalPoints = account.TotalPoints,
            Tier = account.Tier,
            Timestamp = DateTime.UtcNow
        });

        if (!published)
        {
            _logger.LogWarning("Points awarded event publish failed for AuthUserId: {Id}", authUserId);
        }

        _logger.LogInformation("Awarded {Points} points to AuthUserId: {Id}. New total: {Total}",
            pointsToAward, authUserId, account.TotalPoints);
    }
}
