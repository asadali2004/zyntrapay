namespace RewardsService.Helpers;

public static class TierHelper
{
    public static string CalculateTier(int totalPoints) => totalPoints switch
    {
        >= 5000 => "Platinum",
        >= 1000 => "Gold",
        _ => "Silver"
    };

    public static int CalculatePointsToEarn(decimal amount)
        => (int)(amount / 100); // 1 point per Rs.100
}