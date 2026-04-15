using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

/// <summary>
/// Entity Framework Core context for authentication service persistence.
/// </summary>
public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Configures schema constraints, defaults, and indexes for auth entities.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            // Unique constraints
            entity.HasIndex(e => e.Email)
                  .IsUnique()
                  .HasDatabaseName("IX_Users_Email");

            entity.HasIndex(e => e.PhoneNumber)
                  .IsUnique()
                  .HasDatabaseName("IX_Users_PhoneNumber");

            // Column defaults
            entity.Property(e => e.Role)
                  .HasDefaultValue("User");

            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETDATE()");

            // Precision for strings
            entity.Property(e => e.Email)
                  .HasMaxLength(150);

            entity.Property(e => e.PhoneNumber)
                  .HasMaxLength(20);

            entity.Property(e => e.PasswordHash)
                  .HasMaxLength(300);

            entity.Property(e => e.Role)
                  .HasMaxLength(20);
        });
    }
}