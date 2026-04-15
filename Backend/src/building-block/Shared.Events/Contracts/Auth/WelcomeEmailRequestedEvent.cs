namespace Shared.Events;

/// <summary>
/// Represents a welcome email request event for newly registered users.
/// </summary>
public class WelcomeEmailRequestedEvent
{
    public string Email { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
