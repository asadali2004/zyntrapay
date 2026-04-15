using Microsoft.Extensions.Options;
using NotificationService.Models;

namespace NotificationService.Extensions;

/// <summary>
/// Validates required RabbitMQ connection configuration at startup.
/// </summary>
public class RabbitMqSettingsValidator : IValidateOptions<RabbitMqSettings>
{
    /// <summary>
    /// Validates RabbitMQ settings and returns failure when required values are missing.
    /// </summary>
    public ValidateOptionsResult Validate(string? name, RabbitMqSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.Host))
            return ValidateOptionsResult.Fail("RabbitMQ:Host is required.");

        return ValidateOptionsResult.Success;
    }
}
