using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalletService.Models;

/// <summary>
/// Represents a persisted wallet account linked to an authenticated user.
/// </summary>
[Table("Wallets")]
public class Wallet
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int AuthUserId { get; set; }

    [MaxLength(150)]
    public string UserEmail { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
}