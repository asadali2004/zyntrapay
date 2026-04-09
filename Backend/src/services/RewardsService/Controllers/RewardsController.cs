using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RewardsService.DTOs;
using RewardsService.Services;

namespace RewardsService.Controllers;

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

    private int GetAuthUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("summary")]
    [ProducesResponseType(typeof(RewardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RewardsErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSummary()
    {
        var (success, data, message) = await _rewardsService.GetSummaryAsync(GetAuthUserId());
        if (!success) return NotFound(BuildErrorResponse(message));
        return Ok(data);
    }

    [HttpGet("catalog")]
    [ProducesResponseType(typeof(List<CatalogItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RewardsErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCatalog()
    {
        var (success, data, message) = await _rewardsService.GetCatalogAsync();
        if (!success) return BadRequest(BuildErrorResponse(message));
        return Ok(data);
    }

    [HttpPost("redeem")]
    [ProducesResponseType(typeof(RewardsActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RewardsErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Redeem([FromBody] RedeemRequestDto dto)
    {
        var (success, message) = await _rewardsService.RedeemAsync(GetAuthUserId(), dto);
        if (!success) return BadRequest(BuildErrorResponse(message));
        return Ok(new RewardsActionResponseDto { Message = message });
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(List<RedemptionHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RewardsErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory()
    {
        var (success, data, message) = await _rewardsService.GetHistoryAsync(GetAuthUserId());
        if (!success) return NotFound(BuildErrorResponse(message));
        return Ok(data);
    }

    private static RewardsErrorResponseDto BuildErrorResponse(string message)
        => new()
        {
            Message = message,
            ErrorCode = GetErrorCode(message)
        };

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
}
