namespace Shared.Events;

public class WalletTransferCompletedEvent
{
    public int SenderAuthUserId { get; set; }
    public string SenderEmail { get; set; } = string.Empty;
    public int ReceiverAuthUserId { get; set; }
    public string ReceiverEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
}