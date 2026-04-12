using System.ComponentModel.DataAnnotations;

namespace WalletService.DTOs;

public class TopUpRequestDto
{
    [Required]
    [Range(1, 50000, ErrorMessage = "Top-up amount must be between 1 and 50,000.")]
    public decimal Amount { get; set; }

    [Required]
    public string Description { get; set; } = "Wallet Top-Up";
}

public class TransferRequestDto
{
    public int? ReceiverAuthUserId { get; set; }

    [EmailAddress(ErrorMessage = "Receiver email must be valid.")]
    public string? ReceiverEmail { get; set; }

    [Required]
    [Range(1, 25000, ErrorMessage = "Transfer amount must be between 1 and 25,000.")]
    public decimal Amount { get; set; }

    [Required]
    public string Description { get; set; } = "Fund Transfer";
}

public class WalletResponseDto
{
    public int Id { get; set; }
    public int AuthUserId { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LedgerEntryDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WalletActionResponseDto
{
    public string Message { get; set; } = string.Empty;
}

public class WalletErrorResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
}
