using NotificationService.DTOs;
using NotificationService.Models;
using NotificationService.Repositories;

namespace NotificationService.Services;

/// <summary>
/// Implements notification query and lifecycle operations backed by repository persistence.
/// </summary>
public class NotificationServiceImpl : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly ILogger<NotificationServiceImpl> _logger;

    public NotificationServiceImpl(
        INotificationRepository repo,
        ILogger<NotificationServiceImpl> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<(bool Success, List<NotificationDto>? Data, string Message)> GetAllAsync(int authUserId)
    {
        var notifications = await _repo.GetByAuthUserIdAsync(authUserId);

        var result = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();

        return (true, result, "Notifications fetched.");
    }

    public async Task<(bool Success, string Message)> MarkAsReadAsync(int authUserId, int notificationId)
    {
        var notification = await _repo.GetByIdAsync(notificationId);

        if (notification == null || notification.AuthUserId != authUserId)
            return (false, "Notification not found.");

        notification.IsRead = true;
        await _repo.SaveChangesAsync();

        return (true, "Marked as read.");
    }

    public async Task CreateAsync(int authUserId, string title, string message)
    {
        _logger.LogInformation("Creating notification for AuthUserId: {Id}", authUserId);

        await _repo.AddAsync(new Notification
        {
            AuthUserId = authUserId,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _repo.SaveChangesAsync();
    }
}