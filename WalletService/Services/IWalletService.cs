using WalletService.DTOs;

namespace WalletService.Services;

public interface IWalletService
{
    Task<(bool Success, string Message)> CreateWalletAsync(int authUserId);
    Task<(bool Success, WalletResponseDto? Data, string Message)> GetBalanceAsync(int authUserId);
    Task<(bool Success, string Message)> TopUpAsync(int authUserId, TopUpRequestDto dto);
    Task<(bool Success, string Message)> TransferAsync(int authUserId, TransferRequestDto dto);
    Task<(bool Success, List<LedgerEntryDto>? Data, string Message)> GetTransactionsAsync(int authUserId);
    Task<(bool Success, LedgerEntryDto? Data, string Message)> GetTransactionByIdAsync(int authUserId, int entryId);
}