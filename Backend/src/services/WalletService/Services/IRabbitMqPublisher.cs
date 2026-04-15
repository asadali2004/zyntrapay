namespace WalletService.Services;

/// <summary>
/// Provides a lightweight abstraction for publishing wallet integration events to RabbitMQ.
/// </summary>
public interface IRabbitMqPublisher
{
    bool Publish<T>(T message);
}