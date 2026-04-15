using System.ComponentModel.DataAnnotations;

namespace AdminService.DTOs;

/// <summary>
/// Carries administrator KYC review decision and related target user context.
/// </summary>
public class ReviewKycDto
{
    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = string.Empty; // Approved / Rejected

    public string? RejectionReason { get; set; }
    
    public int TargetAuthUserId { get; set; }
    
    [MaxLength(150)]
    public string UserEmail { get; set; } = string.Empty;
}

/// <summary>
/// Represents KYC submission data returned for administrative review.
/// </summary>
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

/// <summary>
/// Represents user profile details exposed to administrators.
/// </summary>
public class AdminUserProfileDto
{
    public int Id { get; set; }
    public int AuthUserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
}

/// <summary>
/// Represents a lightweight user summary used in admin user-management views.
/// </summary>
public class UserSummaryDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Represents dashboard aggregates for administrative overview metrics.
/// </summary>
public class DashboardDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int PendingKyc { get; set; }
    public int ApprovedKyc { get; set; }
    public int RejectedKyc { get; set; }
}

/// <summary>
/// Represents a generic successful admin-operation response.
/// </summary>
public class AdminActionResponseDto
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Represents an admin audit action record returned to clients.
/// </summary>
public class AdminActionDto
{
    public int Id { get; set; }
    public int AdminAuthUserId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public int TargetUserId { get; set; }
    public string? Remarks { get; set; }
    public DateTime PerformedAt { get; set; }
}

/// <summary>
/// Represents a standardized error payload for admin endpoints.
/// </summary>
public class AdminErrorResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
}