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

    // ── TRANSFER TESTS ────────────────────────────────────────────────────

    [Test]
    public async Task Transfer_SelfTransfer_ReturnsFalse()
    {
        // Arrange
        var dto = new TransferRequestDto
        {
            ReceiverAuthUserId = 1,  // same as sender
            Amount = 200m
        };

        // Act
        var (success, message) = await _walletService.TransferAsync(1, "john@example.com", dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("own wallet"));
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