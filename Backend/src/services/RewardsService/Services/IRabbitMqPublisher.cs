namespace RewardsService.Services;

public interface IRabbitMqPublisher
{
    bool Publish<T>(T message);
}
