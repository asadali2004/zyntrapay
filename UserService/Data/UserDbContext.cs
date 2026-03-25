using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options) { }

    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<KycSubmission> KycSubmissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasIndex(e => e.AuthUserId)
                  .IsUnique()
                  .HasDatabaseName("IX_UserProfiles_AuthUserId");

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<KycSubmission>(entity =>
        {
            entity.HasIndex(e => e.AuthUserId)
                  .IsUnique()
                  .HasDatabaseName("IX_KycSubmissions_AuthUserId");

            entity.Property(e => e.Status)
                  .HasDefaultValue("Pending");

            entity.Property(e => e.SubmittedAt)
                  .HasDefaultValueSql("GETDATE()");
        });
    }
}