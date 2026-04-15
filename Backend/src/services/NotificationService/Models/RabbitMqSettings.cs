namespace NotificationService.Models;

/// <summary>
/// Represents RabbitMQ connection settings used by notification consumers.
/// </summary>
public class RabbitMqSettings
{
    public string Host { get; set; } = string.Empty;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}
