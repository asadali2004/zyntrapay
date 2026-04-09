using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Models;

namespace NotificationService.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Notification notification)
        => await _context.Notifications.AddAsync(notification);

    public async Task<List<Notification>> GetByAuthUserIdAsync(int authUserId)
        => await _context.Notifications
            .Where(n => n.AuthUserId == authUserId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<Notification?> GetByIdAsync(int id)
        => await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}