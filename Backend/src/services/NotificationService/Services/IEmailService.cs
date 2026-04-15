namespace NotificationService.Services;

/// <summary>
/// Defines outbound email delivery operations for notification workflows.
/// </summary>
public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string htmlBody);
}