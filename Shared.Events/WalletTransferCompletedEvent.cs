namespace Shared.Events;

public class WalletTransferCompletedEvent
{
    public int SenderAuthUserId { get; set; }
    public int ReceiverAuthUserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
}