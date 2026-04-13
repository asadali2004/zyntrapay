using RabbitMQ.Client;

namespace Shared.Events;

public class RabbitMqConnectionFactoryBuilderImpl : IRabbitMqConnectionFactoryBuilder
{
    public IConnectionFactory Create(RabbitMqConnectionOptions options)
        => RabbitMqConnectionFactoryBuilder.Create(options);
}
