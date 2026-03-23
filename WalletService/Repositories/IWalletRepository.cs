using WalletService.Models;

namespace WalletService.Repositories;

public interface IWalletRepository
{
    Task<Wallet?> GetWalletByAuthUserIdAsync(int authUserId);
    Task<Wallet?> GetWalletByIdAsync(int walletId);
    Task<bool> WalletExistsAsync(int authUserId);
    Task AddWalletAsync(Wallet wallet);
    Task AddLedgerEntryAsync(LedgerEntry entry);
    Task<List<LedgerEntry>> GetLedgerByWalletIdAsync(int walletId);
    Task<LedgerEntry?> GetLedgerEntryByIdAsync(int entryId);
    Task SaveChangesAsync();
}