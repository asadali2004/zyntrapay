using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public partial class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0759045AD1");

            entity.HasIndex(e => e.PhoneNumber, "UQ__Users__85FB4E383EB2E71C").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105344358441F").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(300);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
