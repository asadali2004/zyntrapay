using RabbitMQ.Client;

namespace Shared.Events;

public static class RabbitMqQueueConventions
{
    public static string GetQueueName<T>() => typeof(T).Name;

    public static string GetDeadLetterQueueName(string queueName) => $"{queueName}.dlq";

    public static IDictionary<string, object> CreateDeadLetterArguments(string deadLetterQueueName)
        => new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = "",
            ["x-dead-letter-routing-key"] = deadLetterQueueName
        };

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
