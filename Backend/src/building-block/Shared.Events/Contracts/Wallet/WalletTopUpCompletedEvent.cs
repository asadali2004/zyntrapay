namespace Shared.Events;

/// <summary>
/// Represents a completed wallet top-up event with balance snapshot.
/// </summary>
public class WalletTopUpCompletedEvent
{
    public int AuthUserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal NewBalance { get; set; }
    public DateTime Timestamp { get; set; }
}