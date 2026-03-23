namespace WalletService.Services;

public interface IRabbitMqPublisher
{
    void Publish<T>(T message);
}