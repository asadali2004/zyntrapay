namespace AdminService.Services;

/// <summary>
/// Provides a lightweight abstraction for publishing admin integration events to RabbitMQ.
/// </summary>
public interface IRabbitMqPublisher
{
    bool Publish<T>(T message);
}