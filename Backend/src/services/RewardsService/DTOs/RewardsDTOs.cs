using System.ComponentModel.DataAnnotations;

namespace RewardsService.DTOs;

public class RewardSummaryDto
{
    public int AuthUserId { get; set; }
    public int TotalPoints { get; set; }
    public string Tier { get; set; } = string.Empty;
}

public class CatalogItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PointsCost { get; set; }
    public int Stock { get; set; }
}

public class RedeemRequestDto
{
    [Required(ErrorMessage = "Catalog item ID is required.")]
    public int RewardCatalogId { get; set; }
}

public class RedemptionHistoryDto
{
    public int Id { get; set; }
    public string RewardTitle { get; set; } = string.Empty;
    public int PointsSpent { get; set; }
    public DateTime RedeemedAt { get; set; }
}

public class RewardsActionResponseDto
{
    public string Message { get; set; } = string.Empty;
}

public class RewardsErrorResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
}
