using RabbitMQ.Client;

namespace Shared.Events;

/// <summary>
/// Provides shared queue naming and dead-letter declaration conventions for RabbitMQ.
/// </summary>
public static class RabbitMqQueueConventions
{
    /// <summary>
    /// Resolves queue name based on event type name.
    /// </summary>
    public static string GetQueueName<T>() => typeof(T).Name;

    /// <summary>
    /// Resolves dead-letter queue name for the specified primary queue.
    /// </summary>
    public static string GetDeadLetterQueueName(string queueName) => $"{queueName}.dlq";

    /// <summary>
    /// Creates RabbitMQ dead-letter arguments for queue declaration.
    /// </summary>
    public static IDictionary<string, object> CreateDeadLetterArguments(string deadLetterQueueName)
        => new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = "",
            ["x-dead-letter-routing-key"] = deadLetterQueueName
        };

    /// <summary>
    /// Declares both primary and dead-letter queues with durable settings.
    /// </summary>
    public static void DeclareQueueWithDeadLetter(IModel channel, string queueName)
    {
        var deadLetterQueueName = GetDeadLetterQueueName(queueName);

        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: CreateDeadLetterArguments(deadLetterQueueName));

        channel.QueueDeclare(
            queue: deadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }
}
