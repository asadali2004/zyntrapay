using RabbitMQ.Client;

namespace Shared.Events;

public interface IRabbitMqConnectionFactoryBuilder
{
    IConnectionFactory Create(RabbitMqConnectionOptions options);
}
