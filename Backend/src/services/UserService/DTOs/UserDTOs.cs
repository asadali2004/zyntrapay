using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs;

public class CreateProfileDto
{
    [Required]
    public int AuthUserId { get; set; }

    [Required(ErrorMessage = "Full name is required.")]
    [MinLength(3, ErrorMessage = "Name must be at least 3 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required.")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required.")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "State is required.")]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pin code is required.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Pin code must be 6 digits.")]
    public string PinCode { get; set; } = string.Empty;
}

public class ProfileResponseDto
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

public class SubmitKycDto
{
    [Required]
    public int AuthUserId { get; set; }

    [Required(ErrorMessage = "Document type is required.")]
    public string DocumentType { get; set; } = string.Empty;  // Aadhaar, PAN, Passport

    [Required(ErrorMessage = "Document number is required.")]
    [MinLength(8, ErrorMessage = "Document number must be at least 8 characters.")]
    public string DocumentNumber { get; set; } = string.Empty;
}

public class KycResponseDto
{
    public int Id { get; set; }
    public int AuthUserId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class ReviewKycDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
}

public class UserActionResponseDto
{
    public string Message { get; set; } = string.Empty;
}

public class UserErrorResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
}
