using AuthService.Data;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AuthDbContext _context;

    public AuthRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<bool> EmailExistsAsync(string email)
        => await _context.Users.AnyAsync(u => u.Email == email);

    public async Task<bool> PhoneExistsAsync(string phone)
        => await _context.Users.AnyAsync(u => u.PhoneNumber == phone);

    public async Task AddUserAsync(User user)
        => await _context.Users.AddAsync(user);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<List<User>> GetAllUsersAsync()
    => await _context.Users.OrderBy(u => u.Id).ToListAsync();

    public async Task<User?> GetByIdAsync(int id)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
}