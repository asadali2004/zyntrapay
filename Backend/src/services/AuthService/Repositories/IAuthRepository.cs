using AuthService.Models;

namespace AuthService.Repositories;

public interface IAuthRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PhoneExistsAsync(string phone);
    Task AddUserAsync(User user);
    Task SaveChangesAsync();
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetByIdAsync(int id);
}