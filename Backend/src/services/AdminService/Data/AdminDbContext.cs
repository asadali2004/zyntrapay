using AdminService.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Data;

/// <summary>
/// Entity Framework Core context for admin audit persistence.
/// </summary>
public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options)
        : base(options) { }

    public DbSet<AdminAction> AdminActions { get; set; }

    /// <summary>
    /// Configures indexes and default values for admin action entities.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminAction>(entity =>
        {
            entity.HasIndex(e => e.AdminAuthUserId)
                  .HasDatabaseName("IX_AdminActions_AdminAuthUserId");

            entity.HasIndex(e => e.TargetUserId)
                  .HasDatabaseName("IX_AdminActions_TargetUserId");

            entity.Property(e => e.PerformedAt)
                  .HasDefaultValueSql("GETDATE()");
        });
    }
}