using System.ComponentModel.DataAnnotations;

namespace AdminService.DTOs;

public class ReviewKycDto
{
    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = string.Empty; // Approved / Rejected

    public string? RejectionReason { get; set; }
}

public class KycSubmissionDto
{
    public int Id { get; set; }
    public int AuthUserId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class UserSummaryDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DashboardDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int PendingKyc { get; set; }
    public int ApprovedKyc { get; set; }
    public int RejectedKyc { get; set; }
}