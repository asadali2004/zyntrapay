using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [ProducesResponseType(typeof(WalletActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WalletErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWallet()
    {
        var (success, message) = await _walletService.CreateWalletAsync(GetAuthUserId(), GetUserEmail());
        if (!success) return BadRequest(BuildErrorResponse(message));
        return Ok(new WalletActionResponseDto { Message = message });
    }

    [HttpGet("balance")]
    [ProducesResponseType(typeof(WalletResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WalletErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBalance()
    {
        var (success, data, message) = await _walletService.GetBalanceAsync(GetAuthUserId());
        if (!success) return NotFound(BuildErrorResponse(message));
        return Ok(data);
    }

    [HttpPost("topup")]
    [ProducesResponseType(typeof(WalletActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WalletErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TopUp([FromBody] TopUpRequestDto dto)
    {
        var (success, message) = await _walletService.TopUpAsync(GetAuthUserId(), GetUserEmail(), dto);
        if (!success) return BadRequest(BuildErrorResponse(message));
        return Ok(new WalletActionResponseDto { Message = message });
    }

    [HttpPost("transfer")]
    [ProducesResponseType(typeof(WalletActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WalletErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transfer([FromBody] TransferRequestDto dto)
    {
        var (success, message) = await _walletService.TransferAsync(GetAuthUserId(), GetUserEmail(), dto);
        if (!success) return BadRequest(BuildErrorResponse(message));
        return Ok(new WalletActionResponseDto { Message = message });
    }

    [HttpGet("transactions")]
    [ProducesResponseType(typeof(List<LedgerEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WalletErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactions()
    {
        var (success, data, message) = await _walletService.GetTransactionsAsync(GetAuthUserId());
        if (!success) return NotFound(BuildErrorResponse(message));
        return Ok(data);
    }

    [HttpGet("transactions/{id}")]
    [ProducesResponseType(typeof(LedgerEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WalletErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactionById(int id)
    {
        var (success, data, message) = await _walletService.GetTransactionByIdAsync(GetAuthUserId(), id);
        if (!success) return NotFound(BuildErrorResponse(message));
        return Ok(data);
    }

    private static WalletErrorResponseDto BuildErrorResponse(string message)
        => new()
        {
            Message = message,
            ErrorCode = GetErrorCode(message)
        };

    private static string GetErrorCode(string message)
    {
        if (message.Contains("wallet already exists", StringComparison.OrdinalIgnoreCase))
            return "WALLET_ALREADY_EXISTS";

        if (message.Contains("wallet not found", StringComparison.OrdinalIgnoreCase))
            return "WALLET_NOT_FOUND";

        if (message.Contains("deactivated", StringComparison.OrdinalIgnoreCase))
            return "WALLET_DEACTIVATED";

        if (message.Contains("insufficient balance", StringComparison.OrdinalIgnoreCase))
            return "INSUFFICIENT_BALANCE";

        if (message.Contains("cannot transfer to your own wallet", StringComparison.OrdinalIgnoreCase))
            return "SELF_TRANSFER_NOT_ALLOWED";

        if (message.Contains("receiver wallet not found", StringComparison.OrdinalIgnoreCase))
            return "RECEIVER_WALLET_NOT_FOUND";

        if (message.Contains("transaction not found", StringComparison.OrdinalIgnoreCase))
            return "TRANSACTION_NOT_FOUND";

        return "WALLET_VALIDATION_FAILED";
    }
}
