using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using System.Text;
using System.Text.Json;
using RewardsService.Services;

namespace RewardsService.Consumers;

public class WalletTopUpConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<WalletTopUpConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public WalletTopUpConsumer(
        IConfiguration config,
        ILogger<WalletTopUpConsumer> logger,
        IServiceScopeFactory scopeFactory)
    {
        _config = config;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
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

        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

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

                    // Use scope because IRewardsService is Scoped, BackgroundService is Singleton
                    using var scope = _scopeFactory.CreateScope();
                    var rewardsService = scope.ServiceProvider
                        .GetRequiredService<IRewardsService>();

                    await rewardsService.AwardPointsAsync(@event.AuthUserId, @event.Amount);
                }

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing WalletTopUpCompleted event");
                channel.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
        };

        channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("WalletTopUpConsumer started, listening on queue: {Queue}", queueName);

        return Task.CompletedTask;
    }
}