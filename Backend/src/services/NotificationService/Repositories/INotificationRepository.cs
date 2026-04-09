using NotificationService.Models;

namespace NotificationService.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<List<Notification>> GetByAuthUserIdAsync(int authUserId);
    Task<Notification?> GetByIdAsync(int id);
    Task SaveChangesAsync();
}