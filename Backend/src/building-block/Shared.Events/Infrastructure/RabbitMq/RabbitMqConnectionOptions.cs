namespace Shared.Events;

/// <summary>
/// Represents RabbitMQ connection configuration shared across services.
/// </summary>
public class RabbitMqConnectionOptions
{
    public string Host { get; set; } = "localhost";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}
