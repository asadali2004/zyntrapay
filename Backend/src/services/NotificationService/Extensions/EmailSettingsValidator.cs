using Microsoft.Extensions.Options;
using NotificationService.Models;

namespace NotificationService.Extensions;

/// <summary>
/// Validates required email SMTP configuration options at startup.
/// </summary>
public class EmailSettingsValidator : IValidateOptions<EmailSettings>
{
    /// <summary>
    /// Validates email settings and returns collected configuration failures.
    /// </summary>
    public ValidateOptionsResult Validate(string? name, EmailSettings options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Host))
            failures.Add("EmailSettings:Host is required.");

        if (options.Port <= 0 || options.Port > 65535)
            failures.Add("EmailSettings:Port must be between 1 and 65535.");

        if (string.IsNullOrWhiteSpace(options.FromName))
            failures.Add("EmailSettings:FromName is required.");

        if (string.IsNullOrWhiteSpace(options.FromEmail))
            failures.Add("EmailSettings:FromEmail is required.");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
