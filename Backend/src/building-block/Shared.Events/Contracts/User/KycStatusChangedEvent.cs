namespace Shared.Events;

/// <summary>
/// Represents a KYC status update event emitted after admin review.
/// </summary>
public class KycStatusChangedEvent
{
    public int AuthUserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime Timestamp { get; set; }
}