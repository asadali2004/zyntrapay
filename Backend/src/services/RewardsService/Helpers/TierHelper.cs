namespace RewardsService.Helpers;

/// <summary>
/// Provides utility rules for loyalty tier calculation and point earning conversion.
/// </summary>
public static class TierHelper
{
    /// <summary>
    /// Calculates loyalty tier based on total reward points.
    /// </summary>
    public static string CalculateTier(int totalPoints) => totalPoints switch
    {
        >= 5000 => "Platinum",
        >= 1000 => "Gold",
        _ => "Silver"
    };

    /// <summary>
    /// Converts wallet top-up amount into reward points.
    /// </summary>
    public static int CalculatePointsToEarn(decimal amount)
        => (int)(amount / 100); // 1 point per Rs.100
}