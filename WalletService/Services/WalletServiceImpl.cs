using Shared.Events;
using WalletService.DTOs;
using WalletService.Models;
using WalletService.Repositories;

namespace WalletService.Services;

public class WalletServiceImpl : IWalletService
{
    private readonly IWalletRepository _repo;
    private readonly ILogger<WalletServiceImpl> _logger;
    private readonly IRabbitMqPublisher _publisher;

    public WalletServiceImpl(
        IWalletRepository repo,
        ILogger<WalletServiceImpl> logger,
        IRabbitMqPublisher publisher)
    {
        _repo = repo;
        _logger = logger;
        _publisher = publisher;
    }

    public async Task<(bool Success, string Message)> CreateWalletAsync(int authUserId)
    {
        _logger.LogInformation("Creating wallet for AuthUserId: {Id}", authUserId);

        if (await _repo.WalletExistsAsync(authUserId))
            return (false, "Wallet already exists for this user.");

        var wallet = new Wallet
        {
            AuthUserId = authUserId,
            Balance = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddWalletAsync(wallet);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("Wallet created for AuthUserId: {Id}", authUserId);
        return (true, "Wallet created successfully.");
    }

    public async Task<(bool Success, WalletResponseDto? Data, string Message)> GetBalanceAsync(int authUserId)
    {
        var wallet = await _repo.GetWalletByAuthUserIdAsync(authUserId);
        if (wallet == null)
            return (false, null, "Wallet not found.");

        return (true, new WalletResponseDto
        {
            Id = wallet.Id,
            AuthUserId = wallet.AuthUserId,
            Balance = wallet.Balance,
            IsActive = wallet.IsActive,
            CreatedAt = wallet.CreatedAt
        }, "Balance fetched successfully.");
    }

    public async Task<(bool Success, string Message)> TopUpAsync(int authUserId, TopUpRequestDto dto)
    {
        _logger.LogInformation("TopUp request for AuthUserId: {Id}, Amount: {Amount}", authUserId, dto.Amount);

        var wallet = await _repo.GetWalletByAuthUserIdAsync(authUserId);
        if (wallet == null)
            return (false, "Wallet not found. Please create a wallet first.");

        if (!wallet.IsActive)
            return (false, "Wallet is deactivated.");

        // Create ledger entry
        var entry = new LedgerEntry
        {
            WalletId = wallet.Id,
            Type = "CREDIT",
            Amount = dto.Amount,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddLedgerEntryAsync(entry);

        // Update snapshot balance
        wallet.Balance += dto.Amount;
        await _repo.SaveChangesAsync();

        // Publish event to RabbitMQ
        _publisher.Publish(new WalletTopUpCompletedEvent
        {
            AuthUserId = authUserId,
            Amount = dto.Amount,
            NewBalance = wallet.Balance,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("TopUp successful for AuthUserId: {Id}, NewBalance: {Balance}", authUserId, wallet.Balance);
        return (true, $"Top-up of {dto.Amount:C} successful. New balance: {wallet.Balance:C}");
    }

    public async Task<(bool Success, string Message)> TransferAsync(int authUserId, TransferRequestDto dto)
    {
        _logger.LogInformation("Transfer request from AuthUserId: {From} to {To}, Amount: {Amount}",
            authUserId, dto.ReceiverAuthUserId, dto.Amount);

        if (authUserId == dto.ReceiverAuthUserId)
            return (false, "Cannot transfer to your own wallet.");

        var senderWallet = await _repo.GetWalletByAuthUserIdAsync(authUserId);
        if (senderWallet == null)
            return (false, "Sender wallet not found.");

        if (!senderWallet.IsActive)
            return (false, "Your wallet is deactivated.");

        if (senderWallet.Balance < dto.Amount)
            return (false, "Insufficient balance.");

        var receiverWallet = await _repo.GetWalletByAuthUserIdAsync(dto.ReceiverAuthUserId);
        if (receiverWallet == null)
            return (false, "Receiver wallet not found.");

        if (!receiverWallet.IsActive)
            return (false, "Receiver wallet is deactivated.");

        // Debit sender
        await _repo.AddLedgerEntryAsync(new LedgerEntry
        {
            WalletId = senderWallet.Id,
            Type = "DEBIT",
            Amount = dto.Amount,
            Description = dto.Description,
            ReferenceId = dto.ReceiverAuthUserId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        senderWallet.Balance -= dto.Amount;

        // Credit receiver
        await _repo.AddLedgerEntryAsync(new LedgerEntry
        {
            WalletId = receiverWallet.Id,
            Type = "CREDIT",
            Amount = dto.Amount,
            Description = $"Transfer received from user {authUserId}",
            ReferenceId = authUserId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        receiverWallet.Balance += dto.Amount;

        await _repo.SaveChangesAsync();

        // Publish event
        _publisher.Publish(new WalletTransferCompletedEvent
        {
            SenderAuthUserId = authUserId,
            ReceiverAuthUserId = dto.ReceiverAuthUserId,
            Amount = dto.Amount,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("Transfer successful from {From} to {To}", authUserId, dto.ReceiverAuthUserId);
        return (true, $"Transfer of {dto.Amount:C} successful.");
    }

    public async Task<(bool Success, List<LedgerEntryDto>? Data, string Message)> GetTransactionsAsync(int authUserId)
    {
        var wallet = await _repo.GetWalletByAuthUserIdAsync(authUserId);
        if (wallet == null)
            return (false, null, "Wallet not found.");

        var entries = await _repo.GetLedgerByWalletIdAsync(wallet.Id);

        var result = entries.Select(e => new LedgerEntryDto
        {
            Id = e.Id,
            Type = e.Type,
            Amount = e.Amount,
            Description = e.Description,
            ReferenceId = e.ReferenceId,
            CreatedAt = e.CreatedAt
        }).ToList();

        return (true, result, "Transactions fetched successfully.");
    }

    public async Task<(bool Success, LedgerEntryDto? Data, string Message)> GetTransactionByIdAsync(int authUserId, int entryId)
    {
        var wallet = await _repo.GetWalletByAuthUserIdAsync(authUserId);
        if (wallet == null)
            return (false, null, "Wallet not found.");

        var entry = await _repo.GetLedgerEntryByIdAsync(entryId);
        if (entry == null || entry.WalletId != wallet.Id)
            return (false, null, "Transaction not found.");

        return (true, new LedgerEntryDto
        {
            Id = entry.Id,
            Type = entry.Type,
            Amount = entry.Amount,
            Description = entry.Description,
            ReferenceId = entry.ReferenceId,
            CreatedAt = entry.CreatedAt
        }, "Transaction fetched.");
    }
}