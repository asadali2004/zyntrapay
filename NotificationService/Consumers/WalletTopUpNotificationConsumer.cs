using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Services;

namespace NotificationService.Consumers;

public class WalletTopUpNotificationConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<WalletTopUpNotificationConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public WalletTopUpNotificationConsumer(
        IConfiguration config,
        ILogger<WalletTopUpNotificationConsumer> logger,
        IServiceScopeFactory scopeFactory)
    {
        _config = config;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _config["RabbitMQ:Host"] ?? "localhost",
                    UserName = _config["RabbitMQ:Username"] ?? "guest",
                    Password = _config["RabbitMQ:Password"] ?? "guest"
                };

                var connection = factory.CreateConnection();
                var channel = connection.CreateModel();
                var queueName = nameof(WalletTopUpCompletedEvent);
                var dlqName = $"{queueName}.dlq";

                var queueArgs = new Dictionary<string, object>
                {
                    ["x-dead-letter-exchange"] = "",
                    ["x-dead-letter-routing-key"] = dlqName
                };

                channel.QueueDeclare(queue: queueName, durable: true,
                    exclusive: false, autoDelete: false, arguments: queueArgs);

                channel.QueueDeclare(queue: dlqName, durable: true,
                    exclusive: false, autoDelete: false, arguments: null);

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
}