using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NotificationService.Models;
using NotificationService.Repositories;
using NotificationService.Services;

namespace NotificationService.Tests;

[TestFixture]
public class NotificationServiceTests
{
    private Mock<INotificationRepository> _repoMock = null!;
    private Mock<ILogger<NotificationServiceImpl>> _loggerMock = null!;
    private NotificationServiceImpl _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<INotificationRepository>();
        _loggerMock = new Mock<ILogger<NotificationServiceImpl>>();
        _service = new NotificationServiceImpl(_repoMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task GetAll_ReturnsList()
    {
        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = 1,
                AuthUserId = 10,
                Title = "Welcome",
                Message = "Hello",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _repoMock.Setup(r => r.GetByAuthUserIdAsync(10))
            .ReturnsAsync(notifications);

        var (success, data, message) = await _service.GetAllAsync(10);

        Assert.That(success, Is.True);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.Count, Is.EqualTo(1));
        Assert.That(data[0].Title, Is.EqualTo("Welcome"));
        Assert.That(message, Does.Contain("Notifications fetched"));
    }

    [Test]
    public async Task MarkAsRead_WhenNotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Notification?)null);

        var (success, message) = await _service.MarkAsReadAsync(10, 99);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("not found").IgnoreCase);
    }

    [Test]
    public async Task MarkAsRead_WhenDifferentUser_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(new Notification { Id = 5, AuthUserId = 20 });

        var (success, message) = await _service.MarkAsReadAsync(10, 5);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("not found").IgnoreCase);
    }

    [Test]
    public async Task MarkAsRead_SetsIsReadAndSaves()
    {
        var notification = new Notification
        {
            Id = 7,
            AuthUserId = 10,
            IsRead = false
        };

        _repoMock.Setup(r => r.GetByIdAsync(7))
            .ReturnsAsync(notification);

        var (success, message) = await _service.MarkAsReadAsync(10, 7);

        Assert.That(success, Is.True);
        Assert.That(notification.IsRead, Is.True);
        Assert.That(message, Does.Contain("Marked as read"));
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task Create_AddsNotificationAndSaves()
    {
        var authUserId = 10;
        var title = "Test";
        var body = "Test message";

        await _service.CreateAsync(authUserId, title, body);

        _repoMock.Verify(r => r.AddAsync(It.Is<Notification>(n =>
            n.AuthUserId == authUserId &&
            n.Title == title &&
            n.Message == body &&
            n.IsRead == false
        )), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
