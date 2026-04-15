namespace AuthService.Services;

/// <summary>
/// Provides a lightweight abstraction for publishing integration events to RabbitMQ.
/// </summary>
public interface IRabbitMqPublisher
{
    bool Publish<T>(T message);
}