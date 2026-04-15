using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Services;

namespace NotificationService.Consumers;

/// <summary>
/// Consumes KYC status change events and creates both in-app and email notifications.
/// </summary>
public class KycStatusChangedConsumer : BackgroundService
{
    private readonly RabbitMqConnectionOptions _rabbitMqOptions;
    private readonly ILogger<KycStatusChangedConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public KycStatusChangedConsumer(
        Microsoft.Extensions.Options.IOptions<RabbitMqConnectionOptions> rabbitMqOptions,
        ILogger<KycStatusChangedConsumer> logger,
        IServiceScopeFactory scopeFactory)
    {
        _rabbitMqOptions = rabbitMqOptions.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Starts RabbitMQ consumption loop with retry behavior on broker unavailability.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = RabbitMqConnectionFactoryBuilder.Create(_rabbitMqOptions);

                var connection = factory.CreateConnection();
                var channel = connection.CreateModel();
                var queueName = RabbitMqQueueConventions.GetQueueName<KycStatusChangedEvent>();

                RabbitMqQueueConventions.DeclareQueueWithDeadLetter(channel, queueName);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var @event = JsonSerializer.Deserialize<KycStatusChangedEvent>(json);

                        if (@event != null)
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var svc = scope.ServiceProvider.GetRequiredService<INotificationService>();
                            var emailSvc = scope.ServiceProvider.GetRequiredService<IEmailService>();

                            // In-app notification
                            var emoji = @event.Status == "Approved" ? "✅" : "❌";
                            await svc.CreateAsync(
                                @event.AuthUserId,
                                $"KYC {@event.Status} {emoji}",
                                @event.Status == "Approved"
                                    ? "Your KYC has been approved. All features are now unlocked."
                                    : $"Your KYC was rejected. Reason: {@event.Reason}"
                            );

                            // Email notification
                            if (!string.IsNullOrEmpty(@event.UserEmail))
                            {
                                await emailSvc.SendAsync(
                                    @event.UserEmail,
                                    $"ZyntraPay — KYC {@event.Status}",
                                    EmailTemplates.KycStatusEmail(@event.Status, @event.Reason)
                                );
                            }
                        }

                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing KycStatusChanged. Sending message to DLQ.");
                        channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                    }
                };

                channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
                _logger.LogInformation("KycStatusChangedConsumer listening...");
                await Task.Delay(Timeout.Infinite, stoppingToken);
                break;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning("RabbitMQ unavailable. Retrying in 5s... {Message}", ex.Message);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    /// <summary>
    /// Processes a single KYC status event by creating in-app and optional email notifications.
    /// </summary>
    public async Task ProcessAsync(KycStatusChangedEvent @event)
    {
        using var scope = _scopeFactory.CreateScope();
        var notificationSvc = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var emailSvc = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var suffix = @event.Status == "Approved" ? "Approved" : "Rejected";
        await notificationSvc.CreateAsync(
            @event.AuthUserId,
            $"KYC {suffix}",
            @event.Status == "Approved"
                ? "Your KYC has been approved. All features are now unlocked."
                : $"Your KYC was rejected. Reason: {@event.Reason}");

        if (!string.IsNullOrEmpty(@event.UserEmail))
        {
            await emailSvc.SendAsync(
                @event.UserEmail,
                $"ZyntraPay - KYC {@event.Status}",
                EmailTemplates.KycStatusEmail(@event.Status, @event.Reason));
        }
    }
}
