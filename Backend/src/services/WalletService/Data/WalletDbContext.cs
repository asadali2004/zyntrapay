using Microsoft.EntityFrameworkCore;
using WalletService.Models;

namespace WalletService.Data;

/// <summary>
/// Entity Framework Core context for wallets and ledger entries.
/// </summary>
public class WalletDbContext : DbContext
{
    public WalletDbContext(DbContextOptions<WalletDbContext> options)
        : base(options) { }

    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<LedgerEntry> LedgerEntries { get; set; }

    /// <summary>
    /// Configures relational constraints, defaults, and wallet-ledger relationships.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasIndex(e => e.AuthUserId)
                  .IsUnique()
                  .HasDatabaseName("IX_Wallets_AuthUserId");

            entity.Property(e => e.Balance)
                  .HasDefaultValue(0m);

            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<LedgerEntry>(entity =>
        {
            entity.HasOne(e => e.Wallet)
                  .WithMany(w => w.LedgerEntries)
                  .HasForeignKey(e => e.WalletId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETDATE()");
        });
    }
}