namespace Shared.Events;

/// <summary>
/// Represents reward points awarded to a user after eligible activity.
/// </summary>
public class PointsAwardedEvent
{
    public int AuthUserId { get; set; }
    public int PointsEarned { get; set; }
    public int TotalPoints { get; set; }
    public string Tier { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}