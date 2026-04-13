namespace Shared.Events;

public class PointsAwardedEvent
{
    public int AuthUserId { get; set; }
    public int PointsEarned { get; set; }
    public int TotalPoints { get; set; }
    public string Tier { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}