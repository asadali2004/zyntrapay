using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RewardsService.Models;

namespace RewardsService.Data;

public partial class RewardsDbContext : DbContext
{
    public RewardsDbContext(DbContextOptions<RewardsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Redemption> Redemptions { get; set; }

    public virtual DbSet<RewardAccount> RewardAccounts { get; set; }

    public virtual DbSet<RewardCatalog> RewardCatalogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Redemption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Redempti__3214EC07F9C10426");

            entity.Property(e => e.RedeemedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.RewardCatalog).WithMany(p => p.Redemptions)
                .HasForeignKey(d => d.RewardCatalogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Redemptio__Rewar__6754599E");
        });

        modelBuilder.Entity<RewardAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RewardAc__3214EC07B11D0AA4");

            entity.HasIndex(e => e.AuthUserId, "UQ__RewardAc__7CD892F51BF87EDE").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Tier)
                .HasMaxLength(20)
                .HasDefaultValue("Silver");
        });

        modelBuilder.Entity<RewardCatalog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RewardCa__3214EC071CDA439F");

            entity.ToTable("RewardCatalog");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Stock).HasDefaultValue(-1);
            entity.Property(e => e.Title).HasMaxLength(150);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
