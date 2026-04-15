using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Models;

namespace NotificationService.Services;

/// <summary>
/// Sends transactional emails using configured SMTP settings.
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> settings,
        ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.Host) ||
                string.IsNullOrWhiteSpace(_settings.FromEmail) ||
                string.IsNullOrWhiteSpace(_settings.FromName) ||
                _settings.Port <= 0)
            {
                throw new InvalidOperationException("Email settings are incomplete.");
            }

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                SecureSocketOptions.StartTlsWhenAvailable);

            if (!string.IsNullOrWhiteSpace(_settings.Username) &&
                !string.IsNullOrWhiteSpace(_settings.Password))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
            }
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation(
                "Email sent to {Email} with subject: {Subject} using SMTP host {Host}:{Port}",
                toEmail,
                subject,
                _settings.Host,
                _settings.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email to {Email}. SMTP host {Host}:{Port}, from {FromEmail}, subject {Subject}",
                toEmail,
                _settings.Host,
                _settings.Port,
                _settings.FromEmail,
                subject);
            // Don't throw — email failure should not break main flow
        }
    }
}