using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RewardsService.Models;

[Table("RewardCatalog")]
public class RewardCatalog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }

    [Required]
    public int PointsCost { get; set; }

    public int Stock { get; set; } = -1;  // -1 = unlimited

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();
}