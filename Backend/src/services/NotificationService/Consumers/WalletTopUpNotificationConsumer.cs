using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Services;

namespace NotificationService.Consumers;

/// <summary>
/// Consumes wallet top-up events and generates in-app and email transaction notifications.
/// </summary>
public class WalletTopUpNotificationConsumer : BackgroundService
{
    private readonly RabbitMqConnectionOptions _rabbitMqOptions;
    private readonly ILogger<WalletTopUpNotificationConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public WalletTopUpNotificationConsumer(
        Microsoft.Extensions.Options.IOptions<RabbitMqConnectionOptions> rabbitMqOptions,
        ILogger<WalletTopUpNotificationConsumer> logger,
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
                var queueName = RabbitMqQueueConventions.GetQueueName<WalletTopUpCompletedEvent>();

                RabbitMqQueueConventions.DeclareQueueWithDeadLetter(channel, queueName);
                
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var @event = JsonSerializer.Deserialize<WalletTopUpCompletedEvent>(json);

                        if (@event != null)
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var notificationSvc = scope.ServiceProvider
                                .GetRequiredService<INotificationService>();
                            var emailSvc = scope.ServiceProvider
                                .GetRequiredService<IEmailService>();

                            // Save in-app notification
                            await notificationSvc.CreateAsync(
                                @event.AuthUserId,
                                "Wallet Top-Up Successful",
                                $"Your wallet has been credited with Rs.{@event.Amount:F2}. New balance: Rs.{@event.NewBalance:F2}."
                            );

                            // Send email
                            if (!string.IsNullOrEmpty(@event.UserEmail))
                            {
                                await emailSvc.SendAsync(
                                    @event.UserEmail,
                                    "ZyntraPay — Wallet Top-Up Successful",
                                    EmailTemplates.TransactionEmail("Top-Up", @event.Amount, @event.NewBalance)
                                );
                            }
                        }

                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing WalletTopUpCompleted. Sending message to DLQ.");
                        channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                    }
                };

                channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
                _logger.LogInformation("WalletTopUpNotificationConsumer listening...");
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
    /// Processes a single wallet top-up event by creating in-app and optional email notifications.
    /// </summary>
    public async Task ProcessAsync(WalletTopUpCompletedEvent @event)
    {
        using var scope = _scopeFactory.CreateScope();
        var notificationSvc = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var emailSvc = scope.ServiceProvider.GetRequiredService<IEmailService>();

        await notificationSvc.CreateAsync(
            @event.AuthUserId,
            "Wallet Top-Up Successful",
            $"Your wallet has been credited with Rs.{@event.Amount:F2}. New balance: Rs.{@event.NewBalance:F2}.");

        if (!string.IsNullOrEmpty(@event.UserEmail))
        {
            await emailSvc.SendAsync(
                @event.UserEmail,
                "ZyntraPay - Wallet Top-Up Successful",
                EmailTemplates.TransactionEmail("Top-Up", @event.Amount, @event.NewBalance));
        }
    }
}