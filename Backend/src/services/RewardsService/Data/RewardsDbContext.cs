using Microsoft.EntityFrameworkCore;
using RewardsService.Models;

namespace RewardsService.Data;

/// <summary>
/// Entity Framework Core context for rewards accounts, catalog items, and redemptions.
/// </summary>
public class RewardsDbContext : DbContext
{
    public RewardsDbContext(DbContextOptions<RewardsDbContext> options)
        : base(options) { }

    public DbSet<RewardAccount> RewardAccounts { get; set; }
    public DbSet<RewardCatalog> RewardCatalogs { get; set; }
    public DbSet<Redemption> Redemptions { get; set; }

    /// <summary>
    /// Configures rewards entity constraints, defaults, relationships, and seed data.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RewardAccount>(entity =>
        {
            entity.HasIndex(e => e.AuthUserId)
                  .IsUnique()
                  .HasDatabaseName("IX_RewardAccounts_AuthUserId");

            entity.Property(e => e.Tier)
                  .HasDefaultValue("Silver");

            entity.Property(e => e.TotalPoints)
                  .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<RewardCatalog>(entity =>
        {
            entity.Property(e => e.Stock)
                  .HasDefaultValue(-1);

            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Redemption>(entity =>
        {
            entity.HasOne(e => e.RewardCatalog)
                  .WithMany(c => c.Redemptions)
                  .HasForeignKey(e => e.RewardCatalogId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.RedeemedAt)
                  .HasDefaultValueSql("GETDATE()");
        });

        // Seed catalog data
        modelBuilder.Entity<RewardCatalog>().HasData(
            new RewardCatalog
            {
                Id = 1,
                Title = "Amazon Voucher Rs.100",
                Description = "Amazon gift voucher worth Rs.100",
                PointsCost = 100,
                Stock = -1,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new RewardCatalog
            {
                Id = 2,
                Title = "Free Movie Ticket",
                Description = "BookMyShow voucher for 1 ticket",
                PointsCost = 500,
                Stock = 50,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new RewardCatalog
            {
                Id = 3,
                Title = "Cashback Rs.50",
                Description = "Rs.50 cashback to your wallet",
                PointsCost = 200,
                Stock = -1,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new RewardCatalog
            {
                Id = 4,
                Title = "Premium Membership",
                Description = "1 month premium access",
                PointsCost = 1000,
                Stock = 20,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }
}