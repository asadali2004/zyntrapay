using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using UserService.DTOs;
using UserService.Models;
using UserService.Repositories;
using UserService.Services;

namespace UserService.Tests;

[TestFixture]
public class UserServiceTests
{
    private Mock<IUserRepository> _repoMock = null!;
    private Mock<ILogger<UserServiceImpl>> _loggerMock = null!;
    private UserServiceImpl _userService = null!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<UserServiceImpl>>();
        _userService = new UserServiceImpl(_repoMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task CreateProfile_NewUser_Success()
    {
        _repoMock.Setup(r => r.ProfileExistsAsync(1)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddProfileAsync(It.IsAny<UserProfile>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new CreateProfileDto
        {
            AuthUserId = 1,
            FullName = "John Doe",
            DateOfBirth = new DateTime(2000, 1, 1),
            Address = "Street 1",
            City = "Mumbai",
            State = "MH",
            PinCode = "400001"
        };

        var (success, message) = await _userService.CreateProfileAsync(dto);

        Assert.That(success, Is.True);
        Assert.That(message, Does.Contain("created"));
    }

    [Test]
    public async Task CreateProfile_AlreadyExists_ReturnsFalse()
    {
        _repoMock.Setup(r => r.ProfileExistsAsync(1)).ReturnsAsync(true);

        var dto = new CreateProfileDto
        {
            AuthUserId = 1,
            FullName = "John Doe",
            DateOfBirth = new DateTime(2000, 1, 1),
            Address = "Street 1",
            City = "Mumbai",
            State = "MH",
            PinCode = "400001"
        };

        var (success, message) = await _userService.CreateProfileAsync(dto);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("already exists"));
    }

    [Test]
    public async Task GetProfile_ExistingUser_ReturnsProfile()
    {
        _repoMock.Setup(r => r.GetProfileByAuthUserIdAsync(1)).ReturnsAsync(new UserProfile
        {
            Id = 10,
            AuthUserId = 1,
            FullName = "John Doe",
            DateOfBirth = new DateTime(2000, 1, 1),
            Address = "Street 1",
            City = "Mumbai",
            State = "MH",
            PinCode = "400001"
        });

        var (success, data, message) = await _userService.GetProfileAsync(1);

        Assert.That(success, Is.True);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.FullName, Is.EqualTo("John Doe"));
        Assert.That(message, Does.Contain("fetched"));
    }

    [Test]
    public async Task GetProfile_NotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetProfileByAuthUserIdAsync(99)).ReturnsAsync((UserProfile?)null);

        var (success, data, message) = await _userService.GetProfileAsync(99);

        Assert.That(success, Is.False);
        Assert.That(data, Is.Null);
        Assert.That(message, Does.Contain("not found"));
    }

    [Test]
    public async Task SubmitKyc_NewSubmission_Success()
    {
        _repoMock.Setup(r => r.KycExistsAsync(1)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddKycAsync(It.IsAny<KycSubmission>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new SubmitKycDto
        {
            AuthUserId = 1,
            DocumentType = "Aadhaar",
            DocumentNumber = "123456789012"
        };

        var (success, message) = await _userService.SubmitKycAsync(dto);

        Assert.That(success, Is.True);
        Assert.That(message, Does.Contain("submitted"));
    }

    [Test]
    public async Task SubmitKyc_AlreadySubmitted_ReturnsFalse()
    {
        _repoMock.Setup(r => r.KycExistsAsync(1)).ReturnsAsync(true);

        var dto = new SubmitKycDto
        {
            AuthUserId = 1,
            DocumentType = "Aadhaar",
            DocumentNumber = "123456789012"
        };

        var (success, message) = await _userService.SubmitKycAsync(dto);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("already submitted"));
    }

    [Test]
    public async Task GetKycStatus_ExistingSubmission_ReturnsData()
    {
        _repoMock.Setup(r => r.GetKycByAuthUserIdAsync(1)).ReturnsAsync(new KycSubmission
        {
            Id = 5,
            AuthUserId = 1,
            DocumentType = "PAN",
            DocumentNumber = "ABCDE1234F",
            Status = "Pending",
            SubmittedAt = DateTime.UtcNow
        });

        var (success, data, message) = await _userService.GetKycStatusAsync(1);

        Assert.That(success, Is.True);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.Status, Is.EqualTo("Pending"));
        Assert.That(message, Does.Contain("status fetched"));
    }

    [Test]
    public async Task GetKycStatus_NotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetKycByAuthUserIdAsync(1)).ReturnsAsync((KycSubmission?)null);

        var (success, data, message) = await _userService.GetKycStatusAsync(1);

        Assert.That(success, Is.False);
        Assert.That(data, Is.Null);
        Assert.That(message, Does.Contain("No KYC submission"));
    }

    [Test]
    public async Task GetPendingKycs_ReturnsMappedList()
    {
        _repoMock.Setup(r => r.GetPendingKycsAsync()).ReturnsAsync(new List<KycSubmission>
        {
            new()
            {
                Id = 1,
                AuthUserId = 10,
                DocumentType = "PAN",
                DocumentNumber = "ABCDE1234F",
                Status = "Pending",
                SubmittedAt = DateTime.UtcNow
            }
        });

        var (success, data, message) = await _userService.GetPendingKycsAsync();

        Assert.That(success, Is.True);
        Assert.That(message, Does.Contain("Pending KYC list fetched"));
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.Count, Is.EqualTo(1));
        Assert.That(data[0].DocumentType, Is.EqualTo("PAN"));
    }

    [Test]
    public async Task ReviewKyc_InvalidStatus_ReturnsFalse()
    {
        var dto = new ReviewKycDto { Status = "InReview" };

        var (success, message) = await _userService.ReviewKycAsync(1, dto);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Approved or Rejected"));
    }

    [Test]
    public async Task ReviewKyc_RejectedWithoutReason_ReturnsFalse()
    {
        var dto = new ReviewKycDto { Status = "Rejected", RejectionReason = "" };

        var (success, message) = await _userService.ReviewKycAsync(1, dto);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Rejection reason"));
    }

    [Test]
    public async Task ReviewKyc_AlreadyReviewed_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetKycByIdAsync(1)).ReturnsAsync(new KycSubmission
        {
            Id = 1,
            AuthUserId = 1,
            Status = "Approved"
        });

        var dto = new ReviewKycDto { Status = "Rejected", RejectionReason = "Mismatch" };

        var (success, message) = await _userService.ReviewKycAsync(1, dto);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("already"));
    }

    [Test]
    public async Task ReviewKyc_PendingApproved_Success()
    {
        var kyc = new KycSubmission
        {
            Id = 1,
            AuthUserId = 1,
            Status = "Pending"
        };

        _repoMock.Setup(r => r.GetKycByIdAsync(1)).ReturnsAsync(kyc);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new ReviewKycDto { Status = "Approved" };

        var (success, message) = await _userService.ReviewKycAsync(1, dto);

        Assert.That(success, Is.True);
        Assert.That(kyc.Status, Is.EqualTo("Approved"));
        Assert.That(message, Does.Contain("successfully"));
    }

    [Test]
    public async Task ReviewKyc_PendingRejected_WithReason_Success()
    {
        var kyc = new KycSubmission
        {
            Id = 2,
            AuthUserId = 1,
            Status = "Pending"
        };

        _repoMock.Setup(r => r.GetKycByIdAsync(2)).ReturnsAsync(kyc);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new ReviewKycDto { Status = "Rejected", RejectionReason = "Document mismatch" };

        var (success, message) = await _userService.ReviewKycAsync(2, dto);

        Assert.That(success, Is.True);
        Assert.That(kyc.Status, Is.EqualTo("Rejected"));
        Assert.That(kyc.RejectionReason, Is.EqualTo("Document mismatch"));
        Assert.That(message, Does.Contain("successfully"));
    }

    // ...existing tests...
}
