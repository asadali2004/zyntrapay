using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NotificationService.Consumers;
using NotificationService.Services;
using NUnit.Framework;
using Shared.Events;

namespace NotificationService.Tests;

[TestFixture]
public class ConsumerProcessTests
{
    private static RabbitMqConnectionOptions CreateOptions()
        => new()
        {
            Host = "localhost",
            Username = "guest",
            Password = "guest"
        };

    private static IServiceScopeFactory CreateScopeFactory(
        IEmailService? emailService = null,
        INotificationService? notificationService = null)
    {
        var providerMock = new Mock<IServiceProvider>();
        if (emailService != null)
        {
            providerMock.Setup(p => p.GetService(typeof(IEmailService))).Returns(emailService);
        }

        if (notificationService != null)
        {
            providerMock.Setup(p => p.GetService(typeof(INotificationService))).Returns(notificationService);
        }

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.SetupGet(s => s.ServiceProvider).Returns(providerMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);
        return scopeFactoryMock.Object;
    }

    [Test]
    public async Task OtpRequestedConsumer_ProcessAsync_SendsEmail()
    {
        var emailMock = new Mock<IEmailService>();
        var consumer = new OtpRequestedConsumer(
            Options.Create(CreateOptions()),
            new Mock<ILogger<OtpRequestedConsumer>>().Object,
            CreateScopeFactory(emailService: emailMock.Object));

        await consumer.ProcessAsync(new OtpRequestedEvent
        {
            Email = "user@example.com",
            Otp = "123456"
        });

        emailMock.Verify(e => e.SendAsync(
            "user@example.com",
            It.Is<string>(s => s.Contains("OTP")),
            It.Is<string>(body => body.Contains("123456"))), Times.Once);
    }

    [Test]
    public async Task WelcomeEmailConsumer_ProcessAsync_SendsWelcomeEmail()
    {
        var emailMock = new Mock<IEmailService>();
        var consumer = new WelcomeEmailConsumer(
            Options.Create(CreateOptions()),
            new Mock<ILogger<WelcomeEmailConsumer>>().Object,
            CreateScopeFactory(emailService: emailMock.Object));

        await consumer.ProcessAsync(new WelcomeEmailRequestedEvent
        {
            Email = "user@example.com",
            Timestamp = DateTime.UtcNow
        });

        emailMock.Verify(e => e.SendAsync(
            "user@example.com",
            "Welcome to ZyntraPay!",
            It.Is<string>(body => body.Contains("Welcome"))), Times.Once);
    }

    [Test]
    public async Task WalletTopUpNotificationConsumer_ProcessAsync_CreatesNotificationAndEmail()
    {
        var emailMock = new Mock<IEmailService>();
        var notificationMock = new Mock<INotificationService>();
        var consumer = new WalletTopUpNotificationConsumer(
            Options.Create(CreateOptions()),
            new Mock<ILogger<WalletTopUpNotificationConsumer>>().Object,
            CreateScopeFactory(emailMock.Object, notificationMock.Object));

        await consumer.ProcessAsync(new WalletTopUpCompletedEvent
        {
            AuthUserId = 1,
            UserEmail = "user@example.com",
            Amount = 500m,
            NewBalance = 1500m,
            Timestamp = DateTime.UtcNow
        });

        notificationMock.Verify(n => n.CreateAsync(
            1,
            "Wallet Top-Up Successful",
            It.Is<string>(m => m.Contains("500.00") && m.Contains("1500.00"))), Times.Once);
        emailMock.Verify(e => e.SendAsync(
            "user@example.com",
            It.Is<string>(s => s.Contains("Top-Up")),
            It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task WalletTopUpNotificationConsumer_ProcessAsync_WithoutEmail_SkipsEmail()
    {
        var emailMock = new Mock<IEmailService>();
        var notificationMock = new Mock<INotificationService>();
        var consumer = new WalletTopUpNotificationConsumer(
            Options.Create(CreateOptions()),
            new Mock<ILogger<WalletTopUpNotificationConsumer>>().Object,
            CreateScopeFactory(emailMock.Object, notificationMock.Object));

        await consumer.ProcessAsync(new WalletTopUpCompletedEvent
        {
            AuthUserId = 1,
            UserEmail = string.Empty,
            Amount = 500m,
            NewBalance = 1500m,
            Timestamp = DateTime.UtcNow
        });

        notificationMock.Verify(n => n.CreateAsync(
            1,
            "Wallet Top-Up Successful",
            It.IsAny<string>()), Times.Once);
        emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task WalletTransferNotificationConsumer_ProcessAsync_CreatesBothNotificationsAndEmails()
    {
        var emailMock = new Mock<IEmailService>();
        var notificationMock = new Mock<INotificationService>();
        var consumer = new WalletTransferNotificationConsumer(
            Options.Create(CreateOptions()),
            new Mock<ILogger<WalletTransferNotificationConsumer>>().Object,
            CreateScopeFactory(emailMock.Object, notificationMock.Object));

        await consumer.ProcessAsync(new WalletTransferCompletedEvent
        {
            SenderAuthUserId = 1,
            SenderEmail = "sender@example.com",
            ReceiverAuthUserId = 2,
            ReceiverEmail = "receiver@example.com",
            Amount = 250m,
            Timestamp = DateTime.UtcNow
        });

        notificationMock.Verify(n => n.CreateAsync(1, "Transfer Successful", It.IsAny<string>()), Times.Once);
        notificationMock.Verify(n => n.CreateAsync(2, "Money Received", It.IsAny<string>()), Times.Once);
        emailMock.Verify(e => e.SendAsync("sender@example.com", It.Is<string>(s => s.Contains("Transfer")), It.IsAny<string>()), Times.Once);
        emailMock.Verify(e => e.SendAsync("receiver@example.com", It.Is<string>(s => s.Contains("Money Received")), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task WalletTransferNotificationConsumer_ProcessAsync_WithoutEmails_SkipsEmail()
    {
        var emailMock = new Mock<IEmailService>();
        var notificationMock = new Mock<INotificationService>();
        var consumer = new WalletTransferNotificationConsumer(
            Options.Create(CreateOptions()),
            new Mock<ILogger<WalletTransferNotificationConsumer>>().Object,
            CreateScopeFactory(emailMock.Object, notificationMock.Object));

        await consumer.ProcessAsync(new WalletTransferCompletedEvent
        {
            SenderAuthUserId = 1,
            SenderEmail = string.Empty,
            ReceiverAuthUserId = 2,
            ReceiverEmail = string.Empty,
            Amount = 250m,
            Timestamp = DateTime.UtcNow
        });

        notificationMock.Verify(n => n.CreateAsync(1, "Transfer Successful", It.IsAny<string>()), Times.Once);
        notificationMock.Verify(n => n.CreateAsync(2, "Money Received", It.IsAny<string>()), Times.Once);
        emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task KycStatusChangedConsumer_ProcessAsync_Approved_SendsNotificationAndEmail()
    {
        var emailMock = new Mock<IEmailService>();
        var notificationMock = new Mock<INotificationService>();
        var consumer = new KycStatusChangedConsumer(
            Options.Create(CreateOptions()),
            new Mock<ILogger<KycStatusChangedConsumer>>().Object,
            CreateScopeFactory(emailMock.Object, notificationMock.Object));

        await consumer.ProcessAsync(new KycStatusChangedEvent
        {
            AuthUserId = 1,
            UserEmail = "user@example.com",
            Status = "Approved",
            Timestamp = DateTime.UtcNow
        });

        notificationMock.Verify(n => n.CreateAsync(
            1,
            "KYC Approved",
            It.Is<string>(m => m.Contains("approved"))), Times.Once);
        emailMock.Verify(e => e.SendAsync(
            "user@example.com",
            It.Is<string>(s => s.Contains("KYC Approved")),
            It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task KycStatusChangedConsumer_ProcessAsync_RejectedWithoutEmail_SkipsEmail()
    {
        var emailMock = new Mock<IEmailService>();
        var notificationMock = new Mock<INotificationService>();
        var consumer = new KycStatusChangedConsumer(
            Options.Create(CreateOptions()),
            new Mock<ILogger<KycStatusChangedConsumer>>().Object,
            CreateScopeFactory(emailMock.Object, notificationMock.Object));

        await consumer.ProcessAsync(new KycStatusChangedEvent
        {
            AuthUserId = 1,
            UserEmail = string.Empty,
            Status = "Rejected",
            Reason = "Document mismatch",
            Timestamp = DateTime.UtcNow
        });

        notificationMock.Verify(n => n.CreateAsync(
            1,
            "KYC Rejected",
            It.Is<string>(m => m.Contains("Document mismatch"))), Times.Once);
        emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task PointsAwardedNotificationConsumer_ProcessAsync_CreatesNotification()
    {
        var notificationMock = new Mock<INotificationService>();
        var consumer = new PointsAwardedNotificationConsumer(
            Options.Create(CreateOptions()),
            new Mock<ILogger<PointsAwardedNotificationConsumer>>().Object,
            CreateScopeFactory(notificationService: notificationMock.Object));

        await consumer.ProcessAsync(new PointsAwardedEvent
        {
            AuthUserId = 1,
            PointsEarned = 25,
            TotalPoints = 200,
            Tier = "Silver",
            Timestamp = DateTime.UtcNow
        });

        notificationMock.Verify(n => n.CreateAsync(
            1,
            "Reward Points Earned",
            It.Is<string>(m => m.Contains("25") && m.Contains("200") && m.Contains("Silver"))), Times.Once);
    }
}
