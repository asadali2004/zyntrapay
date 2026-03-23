namespace Shared.Events;

public class WalletTopUpCompletedEvent
{
    public int AuthUserId { get; set; }
    public decimal Amount { get; set; }
    public decimal NewBalance { get; set; }
    public DateTime Timestamp { get; set; }
}