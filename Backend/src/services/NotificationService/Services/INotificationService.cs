using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Services;

/// <summary>
/// Defines business operations for notification retrieval, read-status updates, and creation.
/// </summary>
public interface INotificationService
{
    Task<(bool Success, List<NotificationDto>? Data, string Message)> GetAllAsync(int authUserId);
    Task<(bool Success, string Message)> MarkAsReadAsync(int authUserId, int notificationId);
    Task CreateAsync(int authUserId, string title, string message);
}