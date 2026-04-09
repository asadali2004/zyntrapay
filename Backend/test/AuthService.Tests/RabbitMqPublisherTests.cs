using AuthService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RabbitMQ.Client;
using Shared.Events;

namespace AuthService.Tests;

[TestFixture]
public class RabbitMqPublisherTests
{
    private static RabbitMqPublisher CreatePublisher(
        Mock<IRabbitMqConnectionFactoryBuilder> builderMock)
    {
        var options = Options.Create(new RabbitMqConnectionOptions
        {
            Host = "localhost",
            Username = "guest",
            Password = "guest"
        });

        var logger = new Mock<ILogger<RabbitMqPublisher>>();
        return new RabbitMqPublisher(options, builderMock.Object, logger.Object);
    }

    [Test]
    public void Publish_Success_ReturnsTrue()
    {
        var builderMock = new Mock<IRabbitMqConnectionFactoryBuilder>();
        var factoryMock = new Mock<IConnectionFactory>();
        var connectionMock = new Mock<IConnection>();
        var channelMock = new Mock<IModel>();
        var propsMock = new Mock<IBasicProperties>();

        channelMock.Setup(c => c.CreateBasicProperties()).Returns(propsMock.Object);
        channelMock.Setup(c => c.QueueDeclare(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>()))
            .Returns((QueueDeclareOk)null!);
        channelMock.Setup(c => c.BasicPublish(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<IBasicProperties>(),
            It.IsAny<ReadOnlyMemory<byte>>()));

        connectionMock.Setup(c => c.CreateModel()).Returns(channelMock.Object);
        factoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
        builderMock.Setup(b => b.Create(It.IsAny<RabbitMqConnectionOptions>()))
            .Returns(factoryMock.Object);

        var publisher = CreatePublisher(builderMock);

        var result = publisher.Publish(new OtpRequestedEvent
        {
            Email = "john@example.com",
            Otp = "123456"
        });

        Assert.That(result, Is.True);
        channelMock.Verify(c => c.BasicPublish(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<IBasicProperties>(),
            It.IsAny<ReadOnlyMemory<byte>>()), Times.Once);
    }

    [Test]
    public void Publish_WhenConnectionFails_ReturnsFalse()
    {
        var builderMock = new Mock<IRabbitMqConnectionFactoryBuilder>();
        var factoryMock = new Mock<IConnectionFactory>();

        factoryMock.Setup(f => f.CreateConnection())
            .Throws(new Exception("Connection failed"));
        builderMock.Setup(b => b.Create(It.IsAny<RabbitMqConnectionOptions>()))
            .Returns(factoryMock.Object);

        var publisher = CreatePublisher(builderMock);

        var result = publisher.Publish(new OtpRequestedEvent
        {
            Email = "john@example.com",
            Otp = "123456"
        });

        Assert.That(result, Is.False);
    }
}
