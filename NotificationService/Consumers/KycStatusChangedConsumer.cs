using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Services;

namespace NotificationService.Consumers;

public class KycStatusChangedConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<KycStatusChangedConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public KycStatusChangedConsumer(
        IConfiguration config,
        ILogger<KycStatusChangedConsumer> logger,
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
                var queueName = nameof(KycStatusChangedEvent);
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
}
