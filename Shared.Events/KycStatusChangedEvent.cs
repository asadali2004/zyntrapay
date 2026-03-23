namespace Shared.Events;

public class KycStatusChangedEvent
{
    public int AuthUserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime Timestamp { get; set; }
}