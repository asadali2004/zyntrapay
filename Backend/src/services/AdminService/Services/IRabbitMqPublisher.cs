namespace AdminService.Services;

public interface IRabbitMqPublisher
{
    bool Publish<T>(T message);
}
