using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RewardsService.Models;

[Table("RewardAccounts")]
public class RewardAccount
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int AuthUserId { get; set; }

    public int TotalPoints { get; set; } = 0;

    [Required]
    [MaxLength(20)]
    public string Tier { get; set; } = "Silver";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}