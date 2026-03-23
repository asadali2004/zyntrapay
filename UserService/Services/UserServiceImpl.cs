using UserService.DTOs;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services;

public class UserServiceImpl : IUserService
{
    private readonly IUserRepository _repo;
    private readonly ILogger<UserServiceImpl> _logger;

    public UserServiceImpl(IUserRepository repo, ILogger<UserServiceImpl> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<(bool Success, string Message)> CreateProfileAsync(CreateProfileDto dto)
    {
        _logger.LogInformation("Creating profile for AuthUserId: {Id}", dto.AuthUserId); 

        if (await _repo.ProfileExistsAsync(dto.AuthUserId))
            return (false, "Profile already exists for this user.");

        var profile = new UserProfile
        {
            AuthUserId = dto.AuthUserId,
            FullName = dto.FullName,
            DateOfBirth = dto.DateOfBirth,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            PinCode = dto.PinCode,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddProfileAsync(profile);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("Profile created for AuthUserId: {Id}", dto.AuthUserId);
        return (true, "Profile created successfully.");
    }

    public async Task<(bool Success, ProfileResponseDto? Data, string Message)> GetProfileAsync(int authUserId)
    {
        var profile = await _repo.GetProfileByAuthUserIdAsync(authUserId);
        if (profile == null)
            return (false, null, "Profile not found.");

        var dto = new ProfileResponseDto
        {
            Id = profile.Id,
            AuthUserId = profile.AuthUserId,
            FullName = profile.FullName,
            DateOfBirth = profile.DateOfBirth,
            Address = profile.Address,
            City = profile.City,
            State = profile.State,
            PinCode = profile.PinCode
        };

        return (true, dto, "Profile fetched successfully.");
    }

    public async Task<(bool Success, string Message)> SubmitKycAsync(SubmitKycDto dto)
    {
        _logger.LogInformation("KYC submission attempt for AuthUserId: {Id}", dto.AuthUserId); 

        if (await _repo.KycExistsAsync(dto.AuthUserId))
            return (false, "KYC already submitted for this user.");

        var kyc = new KycSubmission
        {
            AuthUserId = dto.AuthUserId,
            DocumentType = dto.DocumentType,
            DocumentNumber = dto.DocumentNumber,
            Status = "Pending",
            SubmittedAt = DateTime.UtcNow
        };

        await _repo.AddKycAsync(kyc);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("KYC submitted for AuthUserId: {Id}", dto.AuthUserId);
        return (true, "KYC submitted successfully. Awaiting review.");
    }

    public async Task<(bool Success, KycResponseDto? Data, string Message)> GetKycStatusAsync(int authUserId)
    {
        var kyc = await _repo.GetKycByAuthUserIdAsync(authUserId);
        if (kyc == null)
            return (false, null, "No KYC submission found.");

        var dto = new KycResponseDto
        {
            Id = kyc.Id,
            AuthUserId = kyc.AuthUserId,
            DocumentType = kyc.DocumentType,
            DocumentNumber = kyc.DocumentNumber,
            Status = kyc.Status,
            RejectionReason = kyc.RejectionReason,
            SubmittedAt = kyc.SubmittedAt
        };

        return (true, dto, "KYC status fetched.");
    }
}