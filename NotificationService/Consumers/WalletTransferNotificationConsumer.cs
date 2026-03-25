using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Services;

namespace NotificationService.Consumers;

public class WalletTransferNotificationConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<WalletTransferNotificationConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public WalletTransferNotificationConsumer(
        IConfiguration config,
        ILogger<WalletTransferNotificationConsumer> logger,
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
                var queueName = nameof(WalletTransferCompletedEvent);

                channel.QueueDeclare(queue: queueName, durable: true,
                    exclusive: false, autoDelete: false, arguments: null);

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
                            var svc = scope.ServiceProvider
                                .GetRequiredService<INotificationService>();

                            // Notify sender
                            await svc.CreateAsync(
                                @event.SenderAuthUserId,
                                "Transfer Successful",
                                $"You have successfully transferred Rs.{@event.Amount:F2}."
                            );

                            // Notify receiver
                            await svc.CreateAsync(
                                @event.ReceiverAuthUserId,
                                "Money Received",
                                $"You have received Rs.{@event.Amount:F2} in your wallet."
                            );
                        }

                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing WalletTransferCompleted");
                        channel.BasicNack(ea.DeliveryTag, false, requeue: true);
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