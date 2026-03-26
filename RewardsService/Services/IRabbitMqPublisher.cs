namespace RewardsService.Services;

public interface IRabbitMqPublisher
{
    void Publish<T>(T message);
}
