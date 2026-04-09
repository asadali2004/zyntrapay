using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Services;

public interface INotificationService
{
    Task<(bool Success, List<NotificationDto>? Data, string Message)> GetAllAsync(int authUserId);
    Task<(bool Success, string Message)> MarkAsReadAsync(int authUserId, int notificationId);
    Task CreateAsync(int authUserId, string title, string message);
}