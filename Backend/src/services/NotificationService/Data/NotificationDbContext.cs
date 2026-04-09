using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options) { }

    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => e.AuthUserId)
                  .HasDatabaseName("IX_Notifications_AuthUserId");

            entity.Property(e => e.IsRead)
                  .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETDATE()");
        });
    }
}