using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RewardsService.Services;

namespace RewardsService.Consumers;

public class WalletTopUpConsumer : BackgroundService
{
    private readonly RabbitMqConnectionOptions _rabbitMqOptions;
    private readonly ILogger<WalletTopUpConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public WalletTopUpConsumer(
        IOptions<RabbitMqConnectionOptions> rabbitMqOptions,
        ILogger<WalletTopUpConsumer> logger,
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
                var queueName = RabbitMqQueueConventions.GetQueueName<WalletTopUpCompletedEvent>();

                RabbitMqQueueConventions.DeclareQueueWithDeadLetter(channel, queueName);

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);
                        var @event = JsonSerializer.Deserialize<WalletTopUpCompletedEvent>(json);

                        if (@event != null)
                        {
                            _logger.LogInformation("Received WalletTopUpCompleted for AuthUserId: {Id}",
                                @event.AuthUserId);

                            using var scope = _scopeFactory.CreateScope();
                            var rewardsService = scope.ServiceProvider
                                .GetRequiredService<IRewardsService>();

                            await rewardsService.AwardPointsAsync(@event.AuthUserId, @event.Amount);
                        }

                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing WalletTopUpCompleted event. Sending message to DLQ.");
                        channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                    }
                };

                channel.BasicConsume(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer);

                _logger.LogInformation("WalletTopUpConsumer started, listening on queue: {Queue}", queueName);
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
