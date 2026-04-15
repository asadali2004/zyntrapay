using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RewardsService.DTOs;
using RewardsService.Services;

namespace RewardsService.Controllers;

/// <summary>
/// Exposes rewards summary, catalog, redemption, and history endpoints.
/// </summary>
[ApiController]
[Route("api/rewards")]
[Authorize]
public class RewardsController : ControllerBase
{
    private readonly IRewardsService _rewardsService;

    public RewardsController(IRewardsService rewardsService)
    {
        _rewardsService = rewardsService;
    }

    /// <summary>
    /// Extracts authenticated user id from JWT claims.
    /// </summary>
    private int GetAuthUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Builds a standardized API error payload for rewards operations.
    /// </summary>
    private static RewardsErrorResponseDto BuildErrorResponse(string message)
        => new()
        {
            Message = message,
            ErrorCode = GetErrorCode(message)
        };

    /// <summary>
    /// Converts service failure messages into stable machine-readable error codes.
    /// </summary>
    private static string GetErrorCode(string message)
    {
        if (message.Contains("rewards account not found", StringComparison.OrdinalIgnoreCase))
            return "REWARDS_ACCOUNT_NOT_FOUND";

        if (message.Contains("reward item not found", StringComparison.OrdinalIgnoreCase))
            return "REWARD_ITEM_NOT_FOUND";

        if (message.Contains("out of stock", StringComparison.OrdinalIgnoreCase))
            return "REWARD_OUT_OF_STOCK";

        if (message.Contains("insufficient points", StringComparison.OrdinalIgnoreCase))
            return "INSUFFICIENT_REWARD_POINTS";

        return "REWARDS_VALIDATION_FAILED";
    }

    // ─── Rewards Endpoints ────────────────────────────────────────────────────

    /// <summary>
    /// Returns the rewards summary (total points, tier) for the authenticated user.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(RewardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RewardsErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSummary()
    {
        var authUserId = GetAuthUserId();
        var (success, data, message) = await _rewardsService.GetSummaryAsync(authUserId);

        if (!success)
            return NotFound(BuildErrorResponse(message));

        return Ok(data);
    }

    /// <summary>
    /// Returns all available rewards catalog items.
    /// </summary>
    [HttpGet("catalog")]
    [ProducesResponseType(typeof(List<CatalogItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RewardsErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCatalog()
    {
        var (success, data, message) = await _rewardsService.GetCatalogAsync();

        if (!success)
            return BadRequest(BuildErrorResponse(message));

        return Ok(data);
    }

    /// <summary>
    /// Redeems a reward catalog item using the authenticated user's points.
    /// </summary>
    [HttpPost("redeem")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RewardsErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Redeem([FromBody] RedeemRequestDto dto)
    {
        var authUserId = GetAuthUserId();
        var (success, message) = await _rewardsService.RedeemAsync(authUserId, dto);

        if (!success)
            return BadRequest(BuildErrorResponse(message));

        return Ok(new { message });
    }

    /// <summary>
    /// Returns the redemption history for the authenticated user.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<RedemptionHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RewardsErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory()
    {
        var authUserId = GetAuthUserId();
        var (success, data, message) = await _rewardsService.GetHistoryAsync(authUserId);

        if (!success)
            return NotFound(BuildErrorResponse(message));

        return Ok(data);
    }
}
