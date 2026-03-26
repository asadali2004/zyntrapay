namespace Shared.Events;

public class WalletTopUpCompletedEvent
{
    public int AuthUserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal NewBalance { get; set; }
    public DateTime Timestamp { get; set; }
}