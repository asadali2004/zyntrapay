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
}