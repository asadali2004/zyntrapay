using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Services;

namespace NotificationService.Consumers;

public class WalletTransferNotificationConsumer : BackgroundService
{
    private readonly RabbitMqConnectionOptions _rabbitMqOptions;
    private readonly ILogger<WalletTransferNotificationConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public WalletTransferNotificationConsumer(
        Microsoft.Extensions.Options.IOptions<RabbitMqConnectionOptions> rabbitMqOptions,
        ILogger<WalletTransferNotificationConsumer> logger,
        IServiceScopeFactory scopeFactory)
    {
        _rabbitMqOptions = rabbitMqOptions.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = RabbitMqConnectionFactoryBuilder.Create(_rabbitMqOptions);

                var connection = factory.CreateConnection();
                var channel = connection.CreateModel();
                var queueName = RabbitMqQueueConventions.GetQueueName<WalletTransferCompletedEvent>();

                RabbitMqQueueConventions.DeclareQueueWithDeadLetter(channel, queueName);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var @event = JsonSerializer.Deserialize<WalletTransferCompletedEvent>(json);

                        if (@event != null)
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var notificationSvc = scope.ServiceProvider
                                .GetRequiredService<INotificationService>();
                            var emailSvc = scope.ServiceProvider
                                .GetRequiredService<IEmailService>();

                            // Notify sender
                            await notificationSvc.CreateAsync(
                                @event.SenderAuthUserId,
                                "Transfer Successful",
                                $"You have successfully transferred Rs.{@event.Amount:F2}."
                            );

                            // Send sender email
                            if (!string.IsNullOrEmpty(@event.SenderEmail))
                            {
                                await emailSvc.SendAsync(
                                    @event.SenderEmail,
                                    "ZyntraPay — Transfer Successful",
                                    EmailTemplates.TransactionEmail("Transfer Sent", @event.Amount, 0)
                                );
                            }

                            // Notify receiver
                            await notificationSvc.CreateAsync(
                                @event.ReceiverAuthUserId,
                                "Money Received",
                                $"You have received Rs.{@event.Amount:F2} in your wallet."
                            );

                            // Send receiver email
                            if (!string.IsNullOrEmpty(@event.ReceiverEmail))
                            {
                                await emailSvc.SendAsync(
                                    @event.ReceiverEmail,
                                    "ZyntraPay — Money Received",
                                    EmailTemplates.TransactionEmail("Transfer Received", @event.Amount, 0)
                                );
                            }
                        }

                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing WalletTransferCompleted. Sending message to DLQ.");
                        channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                    }
                };

                channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
                _logger.LogInformation("WalletTransferNotificationConsumer listening...");
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
}
