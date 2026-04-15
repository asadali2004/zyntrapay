using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Events;
using System.Text;
using System.Text.Json;

namespace AuthService.Services;

/// <summary>
/// Publishes integration events to RabbitMQ queues using shared queue conventions.
/// </summary>
public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly RabbitMqConnectionOptions _rabbitMqOptions;
    private readonly IRabbitMqConnectionFactoryBuilder _factoryBuilder;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(
        IOptions<RabbitMqConnectionOptions> rabbitMqOptions,
        IRabbitMqConnectionFactoryBuilder factoryBuilder,
        ILogger<RabbitMqPublisher> logger)
    {
        _rabbitMqOptions = rabbitMqOptions.Value;
        _factoryBuilder = factoryBuilder;
        _logger = logger;
    }

    public bool Publish<T>(T message)
    {
        try
        {
            var factory = _factoryBuilder.Create(_rabbitMqOptions);

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            var queueName = RabbitMqQueueConventions.GetQueueName<T>();

            RabbitMqQueueConventions.DeclareQueueWithDeadLetter(channel, queueName);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var props = channel.CreateBasicProperties();
            props.Persistent = true;

            channel.BasicPublish(exchange: "", routingKey: queueName,
                basicProperties: props, body: body);

            _logger.LogInformation("Published {EventType} to RabbitMQ", queueName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish {EventType}", typeof(T).Name);
            return false;
        }
    }
}
