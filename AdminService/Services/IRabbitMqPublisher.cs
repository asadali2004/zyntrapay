namespace AdminService.Services;

public interface IRabbitMqPublisher
{
    void Publish<T>(T message);
}
