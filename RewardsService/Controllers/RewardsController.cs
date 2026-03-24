using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> GetSummary()
    {
        var (success, data, message) = await _rewardsService.GetSummaryAsync(GetAuthUserId());
        if (!success) return NotFound(new { message });
        return Ok(data);
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog()
    {
        var (success, data, message) = await _rewardsService.GetCatalogAsync();
        if (!success) return BadRequest(new { message });
        return Ok(data);
    }

    [HttpPost("redeem")]
    public async Task<IActionResult> Redeem([FromBody] RedeemRequestDto dto)
    {
        var (success, message) = await _rewardsService.RedeemAsync(GetAuthUserId(), dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var (success, data, message) = await _rewardsService.GetHistoryAsync(GetAuthUserId());
        if (!success) return NotFound(new { message });
        return Ok(data);
    }
}