using RabbitMQ.Client;

namespace Shared.Events;

/// <summary>
/// Defines a factory builder for constructing RabbitMQ connection factories from options.
/// </summary>
public interface IRabbitMqConnectionFactoryBuilder
{
    IConnectionFactory Create(RabbitMqConnectionOptions options);
}
