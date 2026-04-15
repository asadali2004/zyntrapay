namespace Shared.Events;

/// <summary>
/// Represents an OTP delivery request event for email verification flows.
/// </summary>
public class OtpRequestedEvent
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
