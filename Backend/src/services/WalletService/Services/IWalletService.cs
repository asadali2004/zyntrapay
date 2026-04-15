using WalletService.DTOs;

namespace WalletService.Services;

/// <summary>
/// Defines wallet business operations for balance, top-up, transfer, and ledger retrieval.
/// </summary>
public interface IWalletService
{
    Task<(bool Success, string Message)> CreateWalletAsync(int authUserId, string userEmail);
    Task<(bool Success, WalletResponseDto? Data, string Message)> GetBalanceAsync(int authUserId);
    Task<(bool Success, string Message)> TopUpAsync(int authUserId, string userEmail, TopUpRequestDto dto);
    Task<(bool Success, string Message)> TransferAsync(int authUserId, string senderEmail, TransferRequestDto dto);
    Task<(bool Success, List<LedgerEntryDto>? Data, string Message)> GetTransactionsAsync(int authUserId);
    Task<(bool Success, LedgerEntryDto? Data, string Message)> GetTransactionByIdAsync(int authUserId, int entryId);
}