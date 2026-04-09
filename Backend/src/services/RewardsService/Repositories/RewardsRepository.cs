using Microsoft.EntityFrameworkCore;
using RewardsService.Data;
using RewardsService.Models;

namespace RewardsService.Repositories;

public class RewardsRepository : IRewardsRepository
{
    private readonly RewardsDbContext _context;

    public RewardsRepository(RewardsDbContext context)
    {
        _context = context;
    }

    public async Task<RewardAccount?> GetAccountByAuthUserIdAsync(int authUserId)
        => await _context.RewardAccounts
            .FirstOrDefaultAsync(a => a.AuthUserId == authUserId);

    public async Task<bool> AccountExistsAsync(int authUserId)
        => await _context.RewardAccounts
            .AnyAsync(a => a.AuthUserId == authUserId);

    public async Task AddAccountAsync(RewardAccount account)
        => await _context.RewardAccounts.AddAsync(account);

    public async Task<List<RewardCatalog>> GetActiveCatalogAsync()
        => await _context.RewardCatalogs
            .Where(c => c.IsActive)
            .OrderBy(c => c.PointsCost)
            .ToListAsync();

    public async Task<RewardCatalog?> GetCatalogItemByIdAsync(int id)
        => await _context.RewardCatalogs
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task AddRedemptionAsync(Redemption redemption)
        => await _context.Redemptions.AddAsync(redemption);

    public async Task<List<Redemption>> GetRedemptionsByAuthUserIdAsync(int authUserId)
        => await _context.Redemptions
            .Include(r => r.RewardCatalog)
            .Where(r => r.AuthUserId == authUserId)
            .OrderByDescending(r => r.RedeemedAt)
            .ToListAsync();

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}