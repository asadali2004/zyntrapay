using AdminService.DTOs;
using AdminService.Models;
using AdminService.Repositories;
using AdminService.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shared.Events;

namespace AdminService.Tests;

[TestFixture]
public class AdminServiceTests
{
    private Mock<IAdminRepository> _repoMock = null!;
    private Mock<IUserServiceClient> _userClientMock = null!;
    private Mock<IAuthServiceClient> _authClientMock = null!;
    private Mock<IRabbitMqPublisher> _publisherMock = null!;
    private Mock<ILogger<AdminServiceImpl>> _loggerMock = null!;
    private AdminServiceImpl _adminService = null!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IAdminRepository>();
        _userClientMock = new Mock<IUserServiceClient>();
        _authClientMock = new Mock<IAuthServiceClient>();
        _publisherMock = new Mock<IRabbitMqPublisher>();
        _publisherMock.Setup(p => p.Publish(It.IsAny<object>())).Returns(true);
        _loggerMock = new Mock<ILogger<AdminServiceImpl>>();

        _adminService = new AdminServiceImpl(
            _repoMock.Object,
            _userClientMock.Object,
            _authClientMock.Object,
            _publisherMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task GetPendingKycs_ReturnsList()
    {
        _userClientMock.Setup(x => x.GetPendingKycsAsync()).ReturnsAsync(new List<KycSubmissionDto>
        {
            new() { Id = 1, AuthUserId = 10, Status = "Pending" }
        });

        var (success, data, message) = await _adminService.GetPendingKycsAsync();

        Assert.That(success, Is.True);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.Count, Is.EqualTo(1));
        Assert.That(message, Does.Contain("fetched"));
    }

    [Test]
    public async Task ReviewKyc_InvalidStatus_ReturnsFalse()
    {
        var dto = new ReviewKycDto
        {
            Status = "InReview",
            TargetAuthUserId = 1,
            UserEmail = "john@example.com"
        };

        var (success, message) = await _adminService.ReviewKycAsync(99, 1, dto);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Approved or Rejected"));
    }

    [Test]
    public async Task ReviewKyc_RejectedWithoutReason_ReturnsFalse()
    {
        var dto = new ReviewKycDto
        {
            Status = "Rejected",
            RejectionReason = "",
            TargetAuthUserId = 1,
            UserEmail = "john@example.com"
        };

        var (success, message) = await _adminService.ReviewKycAsync(99, 1, dto);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Rejection reason"));
    }

    [Test]
    public async Task ReviewKyc_DownstreamFailure_ReturnsFalse()
    {
        _userClientMock
            .Setup(x => x.ReviewKycAsync(1, It.IsAny<ReviewKycDto>()))
            .ReturnsAsync(false);

        var dto = new ReviewKycDto
        {
            Status = "Approved",
            TargetAuthUserId = 1,
            UserEmail = "john@example.com"
        };

        var (success, message) = await _adminService.ReviewKycAsync(99, 1, dto);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Failed"));
        _publisherMock.Verify(p => p.Publish(It.IsAny<KycStatusChangedEvent>()), Times.Never);
        _repoMock.Verify(r => r.AddActionAsync(It.IsAny<AdminAction>()), Times.Never);
    }

    [Test]
    public async Task ReviewKyc_Approved_Success()
    {
        _userClientMock
            .Setup(x => x.ReviewKycAsync(1, It.IsAny<ReviewKycDto>()))
            .ReturnsAsync(true);
        _repoMock.Setup(r => r.AddActionAsync(It.IsAny<AdminAction>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new ReviewKycDto
        {
            Status = "Approved",
            TargetAuthUserId = 10,
            UserEmail = "john@example.com"
        };

        var (success, message) = await _adminService.ReviewKycAsync(99, 1, dto);

        Assert.That(success, Is.True);
        Assert.That(message, Does.Contain("successfully"));

        _publisherMock.Verify(p => p.Publish(It.Is<KycStatusChangedEvent>(e =>
            e.AuthUserId == 10 &&
            e.UserEmail == "john@example.com" &&
            e.Status == "Approved")), Times.Once);

        _repoMock.Verify(r => r.AddActionAsync(It.Is<AdminAction>(a =>
            a.AdminAuthUserId == 99 &&
            a.ActionType == "KYC_APPROVED" &&
            a.TargetUserId == 1)), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task ToggleUserStatus_DownstreamFailure_ReturnsFalse()
    {
        _authClientMock.Setup(x => x.ToggleUserStatusAsync(10)).ReturnsAsync(false);

        var (success, message) = await _adminService.ToggleUserStatusAsync(99, 10);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Failed"));
        _repoMock.Verify(r => r.AddActionAsync(It.IsAny<AdminAction>()), Times.Never);
    }

    [Test]
    public async Task ToggleUserStatus_Success()
    {
        _authClientMock.Setup(x => x.ToggleUserStatusAsync(10)).ReturnsAsync(true);
        _repoMock.Setup(r => r.AddActionAsync(It.IsAny<AdminAction>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var (success, message) = await _adminService.ToggleUserStatusAsync(99, 10);

        Assert.That(success, Is.True);
        Assert.That(message, Does.Contain("successfully"));
        _repoMock.Verify(r => r.AddActionAsync(It.Is<AdminAction>(a =>
            a.AdminAuthUserId == 99 &&
            a.ActionType == "USER_STATUS_TOGGLED" &&
            a.TargetUserId == 10)), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetDashboard_ReturnsCounts()
    {
        _authClientMock.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(new List<UserSummaryDto>
        {
            new() { Id = 1, IsActive = true },
            new() { Id = 2, IsActive = false }
        });

        _userClientMock.Setup(x => x.GetPendingKycsAsync()).ReturnsAsync(new List<KycSubmissionDto>
        {
            new() { Id = 1, Status = "Pending" },
            new() { Id = 2, Status = "Pending" }
        });

        var (success, data, message) = await _adminService.GetDashboardAsync();

        Assert.That(success, Is.True);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.TotalUsers, Is.EqualTo(2));
        Assert.That(data.ActiveUsers, Is.EqualTo(1));
        Assert.That(data.PendingKyc, Is.EqualTo(2));
        Assert.That(message, Does.Contain("fetched"));
    }
}
