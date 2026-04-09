using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminService.Models;

[Table("AdminActions")]
public class AdminAction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int AdminAuthUserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ActionType { get; set; } = string.Empty;
    // KYC_APPROVED, KYC_REJECTED, USER_ACTIVATED, USER_DEACTIVATED

    [Required]
    public int TargetUserId { get; set; }

    [MaxLength(300)]
    public string? Remarks { get; set; }

    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
}