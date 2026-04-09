using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shared.Events;
using WalletService.DTOs;
using WalletService.Models;
using WalletService.Repositories;
using WalletService.Services;

namespace WalletService.Tests;

[TestFixture]
public class WalletServiceTests
{
    private Mock<IWalletRepository> _repoMock = null!;
    private Mock<ILogger<WalletServiceImpl>> _loggerMock = null!;
    private Mock<IRabbitMqPublisher> _publisherMock = null!;
    private IMemoryCache _cache = null!;
    private WalletServiceImpl _walletService = null!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IWalletRepository>();
        _loggerMock = new Mock<ILogger<WalletServiceImpl>>();
        _publisherMock = new Mock<IRabbitMqPublisher>();
        _publisherMock.Setup(p => p.Publish(It.IsAny<object>())).Returns(true);
        _cache = new MemoryCache(new MemoryCacheOptions());

        _walletService = new WalletServiceImpl(
            _repoMock.Object,
            _loggerMock.Object,
            _publisherMock.Object,
            _cache
        );
    }

    // ── CREATE WALLET TESTS ───────────────────────────────────────────────

    [Test]
    public async Task CreateWallet_NewUser_Success()
    {
        // Arrange
        _repoMock.Setup(r => r.WalletExistsAsync(1)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddWalletAsync(It.IsAny<Wallet>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var (success, message) = await _walletService.CreateWalletAsync(1, "john@example.com");

        // Assert
        Assert.That(success, Is.True);
        Assert.That(message, Does.Contain("created"));
    }

    [Test]
    public async Task CreateWallet_AlreadyExists_ReturnsFalse()
    {
        // Arrange
        _repoMock.Setup(r => r.WalletExistsAsync(1)).ReturnsAsync(true);

        // Act
        var (success, message) = await _walletService.CreateWalletAsync(1, "john@example.com");

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("already exists"));
    }

    // ── TOP-UP TESTS ──────────────────────────────────────────────────────

    [Test]
    public async Task TopUp_ValidAmount_UpdatesBalance()
    {
        // Arrange
        var wallet = new Wallet
        {
            Id = 1,
            AuthUserId = 1,
            UserEmail = "john@example.com",
            Balance = 500m,
            IsActive = true
        };

        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(1)).ReturnsAsync(wallet);
        _repoMock.Setup(r => r.AddLedgerEntryAsync(It.IsAny<LedgerEntry>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new TopUpRequestDto { Amount = 1000m, Description = "Top-Up" };

        // Act
        var (success, message) = await _walletService.TopUpAsync(1, "john@example.com", dto);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(wallet.Balance, Is.EqualTo(1500m));
        _publisherMock.Verify(p => p.Publish(It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task TopUp_WalletNotFound_ReturnsFalse()
    {
        // Arrange
        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(1)).ReturnsAsync((Wallet?)null);

        var dto = new TopUpRequestDto { Amount = 1000m };

        // Act
        var (success, message) = await _walletService.TopUpAsync(1, "john@example.com", dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("not found"));
    }

    [Test]
    public async Task TopUp_DeactivatedWallet_ReturnsFalse()
    {
        // Arrange
        var wallet = new Wallet
        {
            Id = 1,
            AuthUserId = 1,
            Balance = 500m,
            IsActive = false  // deactivated
        };

        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(1)).ReturnsAsync(wallet);

        var dto = new TopUpRequestDto { Amount = 1000m };

        // Act
        var (success, message) = await _walletService.TopUpAsync(1, "john@example.com", dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("deactivated"));
    }

    // ── GET BALANCE TESTS ────────────────────────────────────────────────

    [Test]
    public async Task GetBalance_WalletNotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(999))
            .ReturnsAsync((Wallet?)null);

        var (success, data, message) = await _walletService.GetBalanceAsync(999);

        Assert.That(success, Is.False);
        Assert.That(data, Is.Null);
        Assert.That(message, Does.Contain("not found"));
    }

    [Test]
    public async Task GetBalance_FirstCall_FromRepo_SecondCall_FromCache()
    {
        var wallet = new Wallet
        {
            Id = 10,
            AuthUserId = 1,
            Balance = 750m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(1)).ReturnsAsync(wallet);

        var (success1, data1, _) = await _walletService.GetBalanceAsync(1);
        var (success2, data2, _) = await _walletService.GetBalanceAsync(1);

        Assert.That(success1, Is.True);
        Assert.That(success2, Is.True);
        Assert.That(data1, Is.Not.Null);
        Assert.That(data2, Is.Not.Null);
        Assert.That(data2!.Balance, Is.EqualTo(750m));

        _repoMock.Verify(r => r.GetWalletByAuthUserIdAsync(1), Times.Once);
    }

    // ── TRANSACTIONS TESTS ───────────────────────────────────────────────

    [Test]
    public async Task GetTransactions_WalletNotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(1)).ReturnsAsync((Wallet?)null);

        var (success, data, message) = await _walletService.GetTransactionsAsync(1);

        Assert.That(success, Is.False);
        Assert.That(data, Is.Null);
        Assert.That(message, Does.Contain("not found"));
    }

    [Test]
    public async Task GetTransactions_WalletExists_MapsEntries()
    {
        var wallet = new Wallet { Id = 7, AuthUserId = 1, Balance = 100m, IsActive = true };
        var entries = new List<LedgerEntry>
        {
            new() { Id = 1, WalletId = 7, Type = "CREDIT", Amount = 200m, Description = "TopUp", ReferenceId = null, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, WalletId = 7, Type = "DEBIT", Amount = 50m, Description = "Transfer", ReferenceId = "2", CreatedAt = DateTime.UtcNow }
        };

        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(1)).ReturnsAsync(wallet);
        _repoMock.Setup(r => r.GetLedgerByWalletIdAsync(7)).ReturnsAsync(entries);

        var (success, data, message) = await _walletService.GetTransactionsAsync(1);

        Assert.That(success, Is.True);
        Assert.That(message, Does.Contain("Transactions fetched"));
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.Count, Is.EqualTo(2));
        Assert.That(data[0].Type, Is.EqualTo("CREDIT"));
        Assert.That(data[1].ReferenceId, Is.EqualTo("2"));
    }

    [Test]
    public async Task GetTransactionById_WalletNotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(1)).ReturnsAsync((Wallet?)null);

        var (success, data, message) = await _walletService.GetTransactionByIdAsync(1, 100);

        Assert.That(success, Is.False);
        Assert.That(data, Is.Null);
        Assert.That(message, Does.Contain("not found"));
    }

    [Test]
    public async Task GetTransactionById_EntryMissingOrDifferentWallet_ReturnsFalse()
    {
        var wallet = new Wallet { Id = 5, AuthUserId = 1, IsActive = true };
        var otherWalletEntry = new LedgerEntry { Id = 10, WalletId = 99, Type = "DEBIT", Amount = 20m, CreatedAt = DateTime.UtcNow };

        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(1)).ReturnsAsync(wallet);
        _repoMock.Setup(r => r.GetLedgerEntryByIdAsync(10)).ReturnsAsync(otherWalletEntry);

        var (success, data, message) = await _walletService.GetTransactionByIdAsync(1, 10);

        Assert.That(success, Is.False);
        Assert.That(data, Is.Null);
        Assert.That(message, Does.Contain("Transaction not found"));
    }

    [Test]
    public async Task GetTransactionById_ValidEntry_ReturnsMappedData()
    {
        var wallet = new Wallet { Id = 5, AuthUserId = 1, IsActive = true };
        var entry = new LedgerEntry
        {
            Id = 10,
            WalletId = 5,
            Type = "DEBIT",
            Amount = 25m,
            Description = "Coffee",
            ReferenceId = "merchant-1",
            CreatedAt = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(1)).ReturnsAsync(wallet);
        _repoMock.Setup(r => r.GetLedgerEntryByIdAsync(10)).ReturnsAsync(entry);

        var (success, data, message) = await _walletService.GetTransactionByIdAsync(1, 10);

        Assert.That(success, Is.True);
        Assert.That(message, Does.Contain("Transaction fetched"));
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.Id, Is.EqualTo(10));
        Assert.That(data.Type, Is.EqualTo("DEBIT"));
        Assert.That(data.ReferenceId, Is.EqualTo("merchant-1"));
    }

    // ── TRANSFER TESTS ────────────────────────────────────────────────────

    [Test]
    public async Task Transfer_SenderWalletNotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(1)).ReturnsAsync((Wallet?)null);

        var dto = new TransferRequestDto
        {
            ReceiverAuthUserId = 2,
            Amount = 100m
        };

        var (success, message) = await _walletService.TransferAsync(1, "john@example.com", dto);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Sender wallet not found"));
    }

    [Test]
    public async Task Transfer_SenderWalletDeactivated_ReturnsFalse()
    {
        var senderWallet = new Wallet
        {
            Id = 1,
            AuthUserId = 1,
            Balance = 1000m,
            IsActive = false
        };

        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(1)).ReturnsAsync(senderWallet);

        var dto = new TransferRequestDto
        {
            ReceiverAuthUserId = 2,
            Amount = 100m
        };

        var (success, message) = await _walletService.TransferAsync(1, "john@example.com", dto);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("deactivated"));
    }

    [Test]
    public async Task Transfer_ReceiverWalletDeactivated_ReturnsFalse()
    {
        var senderWallet = new Wallet
        {
            Id = 1,
            AuthUserId = 1,
            Balance = 1000m,
            IsActive = true,
            UserEmail = "john@example.com"
        };

        var receiverWallet = new Wallet
        {
            Id = 2,
            AuthUserId = 2,
            Balance = 100m,
            IsActive = false,
            UserEmail = "jane@example.com"
        };

        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(1)).ReturnsAsync(senderWallet);
        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(2)).ReturnsAsync(receiverWallet);

        var dto = new TransferRequestDto
        {
            ReceiverAuthUserId = 2,
            Amount = 100m
        };

        var (success, message) = await _walletService.TransferAsync(1, "john@example.com", dto);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Receiver wallet is deactivated"));
    }

    [Test]
    public async Task Transfer_InsufficientBalance_ReturnsFalse()
    {
        // Arrange
        var senderWallet = new Wallet
        {
            Id = 1,
            AuthUserId = 1,
            Balance = 100m,  // less than transfer amount
            IsActive = true,
            UserEmail = "john@example.com"
        };

        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(1)).ReturnsAsync(senderWallet);

        var dto = new TransferRequestDto
        {
            ReceiverAuthUserId = 2,
            Amount = 500m  // more than balance
        };

        // Act
        var (success, message) = await _walletService.TransferAsync(1, "john@example.com", dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Insufficient"));
    }

    [Test]
    public async Task Transfer_ValidTransfer_DebitsSenderCreditReceiver()
    {
        // Arrange
        var senderWallet = new Wallet
        {
            Id = 1,
            AuthUserId = 1,
            Balance = 1000m,
            IsActive = true,
            UserEmail = "john@example.com"
        };

        var receiverWallet = new Wallet
        {
            Id = 2,
            AuthUserId = 2,
            Balance = 200m,
            IsActive = true,
            UserEmail = "jane@example.com"
        };

        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(1)).ReturnsAsync(senderWallet);
        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(2)).ReturnsAsync(receiverWallet);
        _repoMock.Setup(r => r.AddLedgerEntryAsync(It.IsAny<LedgerEntry>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new TransferRequestDto
        {
            ReceiverAuthUserId = 2,
            Amount = 300m,
            Description = "Test transfer"
        };

        // Act
        var (success, message) = await _walletService.TransferAsync(1, "john@example.com", dto);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(senderWallet.Balance, Is.EqualTo(700m));
        Assert.That(receiverWallet.Balance, Is.EqualTo(500m));
        _publisherMock.Verify(p => p.Publish(It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task Transfer_ReceiverWalletNotFound_ReturnsFalse()
    {
        // Arrange
        var senderWallet = new Wallet
        {
            Id = 1,
            AuthUserId = 1,
            Balance = 1000m,
            IsActive = true,
            UserEmail = "john@example.com"
        };

        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(1)).ReturnsAsync(senderWallet);
        _repoMock.Setup(r => r.GetWalletByAuthUserIdAsync(2)).ReturnsAsync((Wallet?)null);

        var dto = new TransferRequestDto
        {
            ReceiverAuthUserId = 2,
            Amount = 300m
        };

        // Act
        var (success, message) = await _walletService.TransferAsync(1, "john@example.com", dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Receiver wallet not found"));
    }
}
