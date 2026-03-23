using Microsoft.EntityFrameworkCore;
using WalletService.Data;
using WalletService.Models;

namespace WalletService.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly WalletDbContext _context;

    public WalletRepository(WalletDbContext context)
    {
        _context = context;
    }

    public async Task<Wallet?> GetWalletByAuthUserIdAsync(int authUserId)
        => await _context.Wallets.FirstOrDefaultAsync(w => w.AuthUserId == authUserId);

    public async Task<Wallet?> GetWalletByIdAsync(int walletId)
        => await _context.Wallets.FirstOrDefaultAsync(w => w.Id == walletId);

    public async Task<bool> WalletExistsAsync(int authUserId)
        => await _context.Wallets.AnyAsync(w => w.AuthUserId == authUserId);

    public async Task AddWalletAsync(Wallet wallet)
        => await _context.Wallets.AddAsync(wallet);

    public async Task AddLedgerEntryAsync(LedgerEntry entry)
        => await _context.LedgerEntries.AddAsync(entry);

    public async Task<List<LedgerEntry>> GetLedgerByWalletIdAsync(int walletId)
        => await _context.LedgerEntries
            .Where(l => l.WalletId == walletId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

    public async Task<LedgerEntry?> GetLedgerEntryByIdAsync(int entryId)
        => await _context.LedgerEntries.FirstOrDefaultAsync(l => l.Id == entryId);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}