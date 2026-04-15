using RabbitMQ.Client;

namespace Shared.Events;

/// <summary>
/// Builds RabbitMQ connection factories using normalized option defaults.
/// </summary>
public static class RabbitMqConnectionFactoryBuilder
{
    /// <summary>
    /// Creates a RabbitMQ connection factory from supplied connection options.
    /// </summary>
    public static ConnectionFactory Create(RabbitMqConnectionOptions options)
        => new()
        {
            HostName = string.IsNullOrWhiteSpace(options.Host) ? "localhost" : options.Host,
            UserName = string.IsNullOrWhiteSpace(options.Username) ? "guest" : options.Username,
            Password = string.IsNullOrWhiteSpace(options.Password) ? "guest" : options.Password
        };
}
