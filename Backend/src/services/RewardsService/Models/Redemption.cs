using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RewardsService.Models;

/// <summary>
/// Represents a completed reward redemption transaction.
/// </summary>
[Table("Redemptions")]
public class Redemption
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int AuthUserId { get; set; }

    [Required]
    public int RewardCatalogId { get; set; }

    [Required]
    public int PointsSpent { get; set; }

    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("RewardCatalogId")]
    public RewardCatalog RewardCatalog { get; set; } = null!;
}