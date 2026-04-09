using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WalletService.DTOs;
using WalletService.Services;

namespace WalletService.Controllers;

[ApiController]
[Route("api/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    private int GetAuthUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string GetUserEmail()
        => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    [HttpPost("create")]
    public async Task<IActionResult> CreateWallet()
    {
        var (success, message) = await _walletService.CreateWalletAsync(GetAuthUserId(), GetUserEmail());
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var (success, data, message) = await _walletService.GetBalanceAsync(GetAuthUserId());
        if (!success) return NotFound(new { message });
        return Ok(data);
    }

    [HttpPost("topup")]
    public async Task<IActionResult> TopUp([FromBody] TopUpRequestDto dto)
    {
        var (success, message) = await _walletService.TopUpAsync(
            GetAuthUserId(), GetUserEmail(), dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequestDto dto)
    {
        var (success, message) = await _walletService.TransferAsync(
            GetAuthUserId(), GetUserEmail(), dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        var (success, data, message) = await _walletService.GetTransactionsAsync(GetAuthUserId());
        if (!success) return NotFound(new { message });
        return Ok(data);
    }

    [HttpGet("transactions/{id}")]
    public async Task<IActionResult> GetTransactionById(int id)
    {
        var (success, data, message) = await _walletService.GetTransactionByIdAsync(GetAuthUserId(), id);
        if (!success) return NotFound(new { message });
        return Ok(data);
    }
}