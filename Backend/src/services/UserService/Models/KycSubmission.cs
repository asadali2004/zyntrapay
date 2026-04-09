using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models;

[Table("KycSubmissions")]
public class KycSubmission
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int AuthUserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string DocumentType { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [MaxLength(300)]
    public string? RejectionReason { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }
}