using RabbitMQ.Client;
using Shared.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace WalletService.Services;

public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly RabbitMqConnectionOptions _rabbitMqOptions;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(
        IOptions<RabbitMqConnectionOptions> rabbitMqOptions,
        ILogger<RabbitMqPublisher> logger)
    {
        _rabbitMqOptions = rabbitMqOptions.Value;
        _logger = logger;
    }

    public bool Publish<T>(T message)
    {
        try
        {
            var factory = RabbitMqConnectionFactoryBuilder.Create(_rabbitMqOptions);

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var queueName = RabbitMqQueueConventions.GetQueueName<T>();

            RabbitMqQueueConventions.DeclareQueueWithDeadLetter(channel, queueName);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var props = channel.CreateBasicProperties();
            props.Persistent = true;

            channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: props,
                body: body);

            _logger.LogInformation("Published event {EventType} to RabbitMQ", queueName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} to RabbitMQ", typeof(T).Name);
            // We don't throw — wallet operation already succeeded, event failure is non-critical
            return false;
        }
    }
}
