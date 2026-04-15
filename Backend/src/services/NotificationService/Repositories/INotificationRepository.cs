using NotificationService.Models;

namespace NotificationService.Repositories;

/// <summary>
/// Defines persistence operations for notification entities.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Adds a notification entity to the data store.
    /// </summary>
    /// <param name="notification">The notification entity to add.</param>
    Task AddAsync(Notification notification);

    /// <summary>
    /// Retrieves a list of notification entities associated with the specified authenticated user.
    /// </summary>
    /// <param name="authUserId">The ID of the authenticated user.</param>
    /// <returns>A list of notification entities.</returns>
    Task<List<Notification>> GetByAuthUserIdAsync(int authUserId);

    /// <summary>
    /// Retrieves a notification entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the notification.</param>
    /// <returns>The notification entity, or null if not found.</returns>
    Task<Notification?> GetByIdAsync(int id);

    /// <summary>
    /// Persist changes to the data store.
    /// </summary>
    Task SaveChangesAsync();
}