using RabbitMQ.Client;

namespace Shared.Events;

public static class RabbitMqConnectionFactoryBuilder
{
    public static ConnectionFactory Create(RabbitMqConnectionOptions options)
        => new()
        {
            HostName = string.IsNullOrWhiteSpace(options.Host) ? "localhost" : options.Host,
            UserName = string.IsNullOrWhiteSpace(options.Username) ? "guest" : options.Username,
            Password = string.IsNullOrWhiteSpace(options.Password) ? "guest" : options.Password
        };
}
