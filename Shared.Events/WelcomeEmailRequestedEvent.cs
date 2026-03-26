namespace Shared.Events;

public class WelcomeEmailRequestedEvent
{
    public string Email { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
