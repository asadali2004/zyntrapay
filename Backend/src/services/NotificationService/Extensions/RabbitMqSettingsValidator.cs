using Microsoft.Extensions.Options;
using NotificationService.Models;

namespace NotificationService.Extensions;

public class RabbitMqSettingsValidator : IValidateOptions<RabbitMqSettings>
{
    public ValidateOptionsResult Validate(string? name, RabbitMqSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.Host))
            return ValidateOptionsResult.Fail("RabbitMQ:Host is required.");

        return ValidateOptionsResult.Success;
    }
}
