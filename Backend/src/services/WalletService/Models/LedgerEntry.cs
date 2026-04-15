using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalletService.Models;

/// <summary>
/// Represents an immutable wallet ledger movement (credit or debit).
/// </summary>
[Table("LedgerEntries")]
public class LedgerEntry
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int WalletId { get; set; }

    [Required]
    [MaxLength(10)]
    public string Type { get; set; } = string.Empty;   // CREDIT / DEBIT

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ReferenceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("WalletId")]
    public Wallet Wallet { get; set; } = null!;
}