namespace RewardsService.Services;

/// <summary>
/// Provides a lightweight abstraction for publishing rewards integration events to RabbitMQ.
/// </summary>
public interface IRabbitMqPublisher
{
    bool Publish<T>(T message);
}