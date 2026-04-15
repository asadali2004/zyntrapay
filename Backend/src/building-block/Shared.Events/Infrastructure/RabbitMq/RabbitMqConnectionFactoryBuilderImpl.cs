using RabbitMQ.Client;

namespace Shared.Events;

/// <summary>
/// Default implementation that delegates RabbitMQ factory creation to shared builder logic.
/// </summary>
public class RabbitMqConnectionFactoryBuilderImpl : IRabbitMqConnectionFactoryBuilder
{
    public IConnectionFactory Create(RabbitMqConnectionOptions options)
        => RabbitMqConnectionFactoryBuilder.Create(options);
}
