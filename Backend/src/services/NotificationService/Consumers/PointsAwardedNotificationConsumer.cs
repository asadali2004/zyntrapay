using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Services;

namespace NotificationService.Consumers;

/// <summary>
/// Consumes points-awarded events and creates in-app reward notifications.
/// </summary>
public class PointsAwardedNotificationConsumer : BackgroundService
{
    private readonly RabbitMqConnectionOptions _rabbitMqOptions;
    private readonly ILogger<PointsAwardedNotificationConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public PointsAwardedNotificationConsumer(
        Microsoft.Extensions.Options.IOptions<RabbitMqConnectionOptions> rabbitMqOptions,
        ILogger<PointsAwardedNotificationConsumer> logger,
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
                var queueName = RabbitMqQueueConventions.GetQueueName<PointsAwardedEvent>();

                RabbitMqQueueConventions.DeclareQueueWithDeadLetter(channel, queueName);
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var @event = JsonSerializer.Deserialize<PointsAwardedEvent>(json);

                        if (@event != null)
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var notificationSvc = scope.ServiceProvider
                                .GetRequiredService<INotificationService>();

                            await notificationSvc.CreateAsync(
                                @event.AuthUserId,
                                "Reward Points Earned",
                                $"You earned {@event.PointsEarned} points. Total points: {@event.TotalPoints}. Current tier: {@event.Tier}."
                            );
                        }

                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing PointsAwardedEvent. Sending message to DLQ.");
                        channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                    }
                };

                channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
                _logger.LogInformation("PointsAwardedNotificationConsumer listening...");
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
    /// Processes a single points-awarded event by creating an in-app notification.
    /// </summary>
    public async Task ProcessAsync(PointsAwardedEvent @event)
    {
        using var scope = _scopeFactory.CreateScope();
        var notificationSvc = scope.ServiceProvider.GetRequiredService<INotificationService>();

        await notificationSvc.CreateAsync(
            @event.AuthUserId,
            "Reward Points Earned",
            $"You earned {@event.PointsEarned} points. Total points: {@event.TotalPoints}. Current tier: {@event.Tier}.");
    }
}
