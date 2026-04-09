using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;

namespace UserService.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetProfileByAuthUserIdAsync(int authUserId)
        => await _context.UserProfiles.FirstOrDefaultAsync(p => p.AuthUserId == authUserId);

    public async Task<bool> ProfileExistsAsync(int authUserId)
        => await _context.UserProfiles.AnyAsync(p => p.AuthUserId == authUserId);

    public async Task AddProfileAsync(UserProfile profile)
        => await _context.UserProfiles.AddAsync(profile);

    public async Task<KycSubmission?> GetKycByAuthUserIdAsync(int authUserId)
        => await _context.KycSubmissions.FirstOrDefaultAsync(k => k.AuthUserId == authUserId);

    public async Task<bool> KycExistsAsync(int authUserId)
        => await _context.KycSubmissions.AnyAsync(k => k.AuthUserId == authUserId);

    public async Task AddKycAsync(KycSubmission kyc)
        => await _context.KycSubmissions.AddAsync(kyc);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<KycSubmission?> GetKycByIdAsync(int kycId)
    => await _context.KycSubmissions.FirstOrDefaultAsync(k => k.Id == kycId);

    public async Task<List<KycSubmission>> GetPendingKycsAsync()
        => await _context.KycSubmissions
            .Where(k => k.Status == "Pending")
            .ToListAsync();
}