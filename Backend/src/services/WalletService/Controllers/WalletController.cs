using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WalletService.DTOs;
using WalletService.Services;

namespace WalletService.Controllers;

/// <summary>
/// Exposes wallet creation, balance, transfer, and transaction endpoints.
/// </summary>
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

    /// <summary>
    /// Extracts authenticated user id from JWT claims.
    /// </summary>
    private int GetAuthUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Extracts authenticated user email from JWT claims.
    /// </summary>
    private string GetUserEmail()
        => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    /// <summary>
    /// Builds a standardized API error payload for wallet operations.
    /// </summary>
    private static WalletErrorResponseDto BuildErrorResponse(string message)
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

    // ─── Wallet Endpoints ─────────────────────────────────────────────────────

    /// <summary>
    /// Creates a wallet for the currently authenticated user.
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreateWallet()
    {
        var authUserId = GetAuthUserId();
        var userEmail = GetUserEmail();

        var (success, message) = await _walletService.CreateWalletAsync(authUserId, userEmail);

        if (!success)
            return BadRequest(BuildErrorResponse(message));

        return Ok(new WalletActionResponseDto { Message = message });
    }

    /// <summary>
    /// Returns the wallet balance for the currently authenticated user.
    /// </summary>
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var authUserId = GetAuthUserId();
        var (success, data, message) = await _walletService.GetBalanceAsync(authUserId);

        if (!success)
            return NotFound(BuildErrorResponse(message));

        return Ok(data);
    }

    /// <summary>
    /// Tops up the wallet of the currently authenticated user.
    /// </summary>
    [HttpPost("topup")]
    public async Task<IActionResult> TopUp([FromBody] TopUpRequestDto dto)
    {
        var authUserId = GetAuthUserId();
        var userEmail = GetUserEmail();

        var (success, message) = await _walletService.TopUpAsync(authUserId, userEmail, dto);

        if (!success)
            return BadRequest(BuildErrorResponse(message));

        return Ok(new WalletActionResponseDto { Message = message });
    }

    /// <summary>
    /// Transfers funds from the authenticated user's wallet to another wallet.
    /// </summary>
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequestDto dto)
    {
        var authUserId = GetAuthUserId();
        var senderEmail = GetUserEmail();

        var (success, message) = await _walletService.TransferAsync(authUserId, senderEmail, dto);

        if (!success)
            return BadRequest(BuildErrorResponse(message));

        return Ok(new WalletActionResponseDto { Message = message });
    }

    /// <summary>
    /// Returns all transactions for the currently authenticated user's wallet.
    /// </summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        var authUserId = GetAuthUserId();
        var (success, data, message) = await _walletService.GetTransactionsAsync(authUserId);

        if (!success)
            return NotFound(BuildErrorResponse(message));

        return Ok(data);
    }

    /// <summary>
    /// Returns a specific transaction by ID for the authenticated user's wallet.
    /// </summary>
    [HttpGet("transactions/{entryId:int}")]
    public async Task<IActionResult> GetTransactionById(int entryId)
    {
        var authUserId = GetAuthUserId();
        var (success, data, message) = await _walletService.GetTransactionByIdAsync(authUserId, entryId);

        if (!success)
            return NotFound(BuildErrorResponse(message));

        return Ok(data);
    }
}
