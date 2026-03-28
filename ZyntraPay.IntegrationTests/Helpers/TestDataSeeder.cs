using AuthService.Data;
using AuthService.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ZyntraPay.IntegrationTests.Helpers;

public static class TestDataSeeder
{
    public static void SeedUser(IServiceProvider services,
        int id, string email, string phone,
        string password = "Test@123", string role = "User")
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        if (!db.Users.Any(u => u.Email == email))
        {
            db.Users.Add(new User
            {
                Id = id,
                Email = email,
                PhoneNumber = phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }
    }
}