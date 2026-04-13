namespace Shared.Events;

public class OtpRequestedEvent
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
