using AuthService.Models;

namespace AuthService.Repositories;

/// <summary>
/// Defines persistence operations required by authentication workflows.
/// </summary>
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