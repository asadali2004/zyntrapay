using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RewardsService.DTOs;
using RewardsService.Helpers;
using RewardsService.Models;
using RewardsService.Repositories;
using RewardsService.Services;
using Shared.Events;

namespace RewardsService.Tests;

[TestFixture]
public class RewardsServiceTests
{
    private Mock<IRewardsRepository> _repoMock = null!;
    private Mock<ILogger<RewardsServiceImpl>> _loggerMock = null!;
    private Mock<IRabbitMqPublisher> _publisherMock = null!;
    private IMemoryCache _cache = null!;
    private RewardsServiceImpl _rewardsService = null!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IRewardsRepository>();
        _loggerMock = new Mock<ILogger<RewardsServiceImpl>>();
        _publisherMock = new Mock<IRabbitMqPublisher>();
        _cache = new MemoryCache(new MemoryCacheOptions());

        _rewardsService = new RewardsServiceImpl(
            _repoMock.Object,
            _loggerMock.Object,
            _cache,
            _publisherMock.Object
        );
    }

    // ── TIER HELPER TESTS ─────────────────────────────────────────────────

    [Test]
    public void TierHelper_0Points_ReturnsSilver()
    {
        Assert.That(TierHelper.CalculateTier(0), Is.EqualTo("Silver"));
    }

    [Test]
    public void TierHelper_999Points_ReturnsSilver()
    {
        Assert.That(TierHelper.CalculateTier(999), Is.EqualTo("Silver"));
    }

    [Test]
    public void TierHelper_1000Points_ReturnsGold()
    {
        Assert.That(TierHelper.CalculateTier(1000), Is.EqualTo("Gold"));
    }

    [Test]
    public void TierHelper_4999Points_ReturnsGold()
    {
        Assert.That(TierHelper.CalculateTier(4999), Is.EqualTo("Gold"));
    }

    [Test]
    public void TierHelper_5000Points_ReturnsPlatinum()
    {
        Assert.That(TierHelper.CalculateTier(5000), Is.EqualTo("Platinum"));
    }

    [Test]
    public void TierHelper_PointsCalculation_100RupeesEquals1Point()
    {
        Assert.That(TierHelper.CalculatePointsToEarn(100m), Is.EqualTo(1));
        Assert.That(TierHelper.CalculatePointsToEarn(1000m), Is.EqualTo(10));
        Assert.That(TierHelper.CalculatePointsToEarn(500m), Is.EqualTo(5));
    }

    // ── SUMMARY TESTS ─────────────────────────────────────────────────────

    [Test]
    public async Task GetSummary_AccountNotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetAccountByAuthUserIdAsync(99))
            .ReturnsAsync((RewardAccount?)null);

        var (success, data, message) = await _rewardsService.GetSummaryAsync(99);

        Assert.That(success, Is.False);
        Assert.That(data, Is.Null);
        Assert.That(message, Does.Contain("not found"));
    }

    [Test]
    public async Task GetSummary_AccountExists_ReturnsData()
    {
        _repoMock.Setup(r => r.GetAccountByAuthUserIdAsync(1))
            .ReturnsAsync(new RewardAccount { AuthUserId = 1, TotalPoints = 1200, Tier = "Gold" });

        var (success, data, _) = await _rewardsService.GetSummaryAsync(1);

        Assert.That(success, Is.True);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.AuthUserId, Is.EqualTo(1));
        Assert.That(data.TotalPoints, Is.EqualTo(1200));
        Assert.That(data.Tier, Is.EqualTo("Gold"));
    }

    // ── CATALOG TESTS ─────────────────────────────────────────────────────

    [Test]
    public async Task GetCatalog_FirstCall_FetchesFromRepo_SecondCall_UsesCache()
    {
        var items = new List<RewardCatalog>
        {
            new() { Id = 1, Title = "Voucher", Description = "Gift", PointsCost = 100, Stock = 10, IsActive = true }
        };

        _repoMock.Setup(r => r.GetActiveCatalogAsync()).ReturnsAsync(items);

        var (success1, data1, _) = await _rewardsService.GetCatalogAsync();
        var (success2, data2, _) = await _rewardsService.GetCatalogAsync();

        Assert.That(success1, Is.True);
        Assert.That(success2, Is.True);
        Assert.That(data1, Is.Not.Null);
        Assert.That(data2, Is.Not.Null);
        Assert.That(data2!.Count, Is.EqualTo(1));

        _repoMock.Verify(r => r.GetActiveCatalogAsync(), Times.Once);
    }

    // ── HISTORY TESTS ─────────────────────────────────────────────────────

    [Test]
    public async Task GetHistory_MapsRewardTitle_WithFallbackForNullCatalog()
    {
        var redemptions = new List<Redemption>
        {
            new()
            {
                Id = 1,
                AuthUserId = 1,
                PointsSpent = 100,
                RedeemedAt = DateTime.UtcNow,
                RewardCatalog = new RewardCatalog { Title = "Amazon Voucher" }
            },
            new()
            {
                Id = 2,
                AuthUserId = 1,
                PointsSpent = 50,
                RedeemedAt = DateTime.UtcNow,
                RewardCatalog = null
            }
        };

        _repoMock.Setup(r => r.GetRedemptionsByAuthUserIdAsync(1)).ReturnsAsync(redemptions);

        var (success, data, message) = await _rewardsService.GetHistoryAsync(1);

        Assert.That(success, Is.True);
        Assert.That(message, Does.Contain("History fetched"));
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.Count, Is.EqualTo(2));
        Assert.That(data[0].RewardTitle, Is.EqualTo("Amazon Voucher"));
        Assert.That(data[1].RewardTitle, Is.EqualTo("Unknown"));
    }

    // ── AWARD POINTS TESTS ────────────────────────────────────────────────

    [Test]
    public async Task AwardPoints_NewUser_CreatesAccountWithPoints()
    {
        // Arrange — no existing account
        _repoMock.Setup(r => r.GetAccountByAuthUserIdAsync(1))
            .ReturnsAsync((RewardAccount?)null);
        _repoMock.Setup(r => r.AddAccountAsync(It.IsAny<RewardAccount>()))
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act — top up Rs.1000 = 10 points
        await _rewardsService.AwardPointsAsync(1, 1000m);

        // Assert — AddAccountAsync was called once with correct points
        _repoMock.Verify(r => r.AddAccountAsync(
            It.Is<RewardAccount>(a => a.TotalPoints == 10 && a.Tier == "Silver")),
            Times.Once);
    }

    [Test]
    public async Task AwardPoints_ExistingUser_AddsToExistingPoints()
    {
        // Arrange
        var account = new RewardAccount
        {
            Id = 1,
            AuthUserId = 1,
            TotalPoints = 900,
            Tier = "Silver"
        };

        _repoMock.Setup(r => r.GetAccountByAuthUserIdAsync(1)).ReturnsAsync(account);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act — Rs.10000 = 100 points, total = 1000 → should become Gold
        await _rewardsService.AwardPointsAsync(1, 10000m);

        // Assert
        Assert.That(account.TotalPoints, Is.EqualTo(1000));
        Assert.That(account.Tier, Is.EqualTo("Gold"));
    }

    [Test]
    public async Task AwardPoints_ZeroAmount_DoesNothing()
    {
        await _rewardsService.AwardPointsAsync(1, 0m);

        _repoMock.Verify(r => r.GetAccountByAuthUserIdAsync(It.IsAny<int>()), Times.Never);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        _publisherMock.Verify(p => p.Publish(It.IsAny<PointsAwardedEvent>()), Times.Never);
    }

    // ── REDEEM TESTS ──────────────────────────────────────────────────────

    [Test]
    public async Task Redeem_InsufficientPoints_ReturnsFalse()
    {
        // Arrange
        var account = new RewardAccount
        {
            Id = 1,
            AuthUserId = 1,
            TotalPoints = 50  // less than required
        };

        var catalogItem = new RewardCatalog
        {
            Id = 1,
            Title = "Amazon Voucher",
            PointsCost = 100,
            Stock = -1,
            IsActive = true
        };

        _repoMock.Setup(r => r.GetAccountByAuthUserIdAsync(1)).ReturnsAsync(account);
        _repoMock.Setup(r => r.GetCatalogItemByIdAsync(1)).ReturnsAsync(catalogItem);

        var dto = new RedeemRequestDto { RewardCatalogId = 1 };

        // Act
        var (success, message) = await _rewardsService.RedeemAsync(1, dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Insufficient points"));
    }

    [Test]
    public async Task Redeem_OutOfStock_ReturnsFalse()
    {
        // Arrange
        var account = new RewardAccount
        {
            Id = 1,
            AuthUserId = 1,
            TotalPoints = 500
        };

        var catalogItem = new RewardCatalog
        {
            Id = 1,
            Title = "Movie Ticket",
            PointsCost = 100,
            Stock = 0,  // out of stock
            IsActive = true
        };

        _repoMock.Setup(r => r.GetAccountByAuthUserIdAsync(1)).ReturnsAsync(account);
        _repoMock.Setup(r => r.GetCatalogItemByIdAsync(1)).ReturnsAsync(catalogItem);

        var dto = new RedeemRequestDto { RewardCatalogId = 1 };

        // Act
        var (success, message) = await _rewardsService.RedeemAsync(1, dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("out of stock"));
    }

    [Test]
    public async Task Redeem_ValidRequest_DeductsPoints()
    {
        // Arrange
        var account = new RewardAccount
        {
            Id = 1,
            AuthUserId = 1,
            TotalPoints = 500,
            Tier = "Silver"
        };

        var catalogItem = new RewardCatalog
        {
            Id = 1,
            Title = "Amazon Voucher",
            PointsCost = 100,
            Stock = -1,  // unlimited
            IsActive = true
        };

        _repoMock.Setup(r => r.GetAccountByAuthUserIdAsync(1)).ReturnsAsync(account);
        _repoMock.Setup(r => r.GetCatalogItemByIdAsync(1)).ReturnsAsync(catalogItem);
        _repoMock.Setup(r => r.AddRedemptionAsync(It.IsAny<Redemption>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new RedeemRequestDto { RewardCatalogId = 1 };

        // Act
        var (success, message) = await _rewardsService.RedeemAsync(1, dto);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(account.TotalPoints, Is.EqualTo(400));
        Assert.That(message, Does.Contain("Successfully redeemed"));
    }

    [Test]
    public async Task Redeem_InactiveItem_ReturnsFalse()
    {
        // Arrange
        var account = new RewardAccount
        {
            Id = 1,
            AuthUserId = 1,
            TotalPoints = 500
        };

        var catalogItem = new RewardCatalog
        {
            Id = 1,
            Title = "Inactive Reward",
            PointsCost = 100,
            Stock = 10,
            IsActive = false  // inactive
        };

        _repoMock.Setup(r => r.GetAccountByAuthUserIdAsync(1)).ReturnsAsync(account);
        _repoMock.Setup(r => r.GetCatalogItemByIdAsync(1)).ReturnsAsync(catalogItem);

        var dto = new RedeemRequestDto { RewardCatalogId = 1 };

        // Act
        var (success, message) = await _rewardsService.RedeemAsync(1, dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("not found or inactive"));
    }

    [Test]
    public async Task Redeem_AccountNotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetAccountByAuthUserIdAsync(1))
            .ReturnsAsync((RewardAccount?)null);

        var dto = new RedeemRequestDto { RewardCatalogId = 1 };

        var (success, message) = await _rewardsService.RedeemAsync(1, dto);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("account not found"));
    }

    [Test]
    public async Task Redeem_LimitedStock_DecrementsStock()
    {
        var account = new RewardAccount
        {
            Id = 1,
            AuthUserId = 1,
            TotalPoints = 500,
            Tier = "Silver"
        };

        var catalogItem = new RewardCatalog
        {
            Id = 1,
            Title = "Movie Ticket",
            PointsCost = 100,
            Stock = 3,
            IsActive = true
        };

        _repoMock.Setup(r => r.GetAccountByAuthUserIdAsync(1)).ReturnsAsync(account);
        _repoMock.Setup(r => r.GetCatalogItemByIdAsync(1)).ReturnsAsync(catalogItem);
        _repoMock.Setup(r => r.AddRedemptionAsync(It.IsAny<Redemption>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new RedeemRequestDto { RewardCatalogId = 1 };

        var (success, _) = await _rewardsService.RedeemAsync(1, dto);

        Assert.That(success, Is.True);
        Assert.That(catalogItem.Stock, Is.EqualTo(2));
        Assert.That(account.TotalPoints, Is.EqualTo(400));
    }
}