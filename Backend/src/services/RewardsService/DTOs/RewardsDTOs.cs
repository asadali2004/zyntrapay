using System.ComponentModel.DataAnnotations;

namespace RewardsService.DTOs;

/// <summary>
/// Represents a user's rewards summary including total points and tier.
/// </summary>
public class RewardSummaryDto
{
    public int AuthUserId { get; set; }
    public int TotalPoints { get; set; }
    public string Tier { get; set; } = string.Empty;
}

/// <summary>
/// Represents a redeemable item exposed in the rewards catalog.
/// </summary>
public class CatalogItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PointsCost { get; set; }
    public int Stock { get; set; }
}

/// <summary>
/// Carries reward redemption input for a selected catalog item.
/// </summary>
public class RedeemRequestDto
{
    [Required(ErrorMessage = "Catalog item ID is required.")]
    public int RewardCatalogId { get; set; }
}

/// <summary>
/// Represents a historical redemption record returned to the user.
/// </summary>
public class RedemptionHistoryDto
{
    public int Id { get; set; }
    public string RewardTitle { get; set; } = string.Empty;
    public int PointsSpent { get; set; }
    public DateTime RedeemedAt { get; set; }
}

/// <summary>
/// Represents a generic successful rewards-operation response.
/// </summary>
public class RewardsActionResponseDto
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Represents a standardized error payload for rewards endpoints.
/// </summary>
public class RewardsErrorResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
}