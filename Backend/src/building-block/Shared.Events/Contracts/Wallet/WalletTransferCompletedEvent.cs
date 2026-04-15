namespace Shared.Events;

/// <summary>
/// Represents a completed wallet transfer event for sender and receiver notifications.
/// </summary>
public class WalletTransferCompletedEvent
{
    public int SenderAuthUserId { get; set; }
    public string SenderEmail { get; set; } = string.Empty;
    public int ReceiverAuthUserId { get; set; }
    public string ReceiverEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
}