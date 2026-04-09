using RewardsService.Models;

namespace RewardsService.Repositories;

public interface IRewardsRepository
{
    // Reward Account
    Task<RewardAccount?> GetAccountByAuthUserIdAsync(int authUserId);
    Task<bool> AccountExistsAsync(int authUserId);
    Task AddAccountAsync(RewardAccount account);

    // Catalog
    Task<List<RewardCatalog>> GetActiveCatalogAsync();
    Task<RewardCatalog?> GetCatalogItemByIdAsync(int id);

    // Redemptions
    Task AddRedemptionAsync(Redemption redemption);
    Task<List<Redemption>> GetRedemptionsByAuthUserIdAsync(int authUserId);

    Task SaveChangesAsync();
}