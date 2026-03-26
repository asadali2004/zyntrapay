using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Services;

namespace NotificationService.Consumers;

public class WelcomeEmailConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<WelcomeEmailConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public WelcomeEmailConsumer(
        IConfiguration config,
        ILogger<WelcomeEmailConsumer> logger,
        IServiceScopeFactory scopeFactory)
    {
        _config = config;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _config["RabbitMQ:Host"] ?? "localhost",
                    UserName = _config["RabbitMQ:Username"] ?? "guest",
                    Password = _config["RabbitMQ:Password"] ?? "guest"
                };

                var connection = factory.CreateConnection();
                var channel = connection.CreateModel();
                var queueName = nameof(WelcomeEmailRequestedEvent);

                channel.QueueDeclare(queue: queueName, durable: true,
                    exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var @event = JsonSerializer.Deserialize<WelcomeEmailRequestedEvent>(json);

                        if (@event != null)
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var emailSvc = scope.ServiceProvider.GetRequiredService<IEmailService>();

                            await emailSvc.SendAsync(
                                @event.Email,
                                "Welcome to ZyntraPay!",
                                EmailTemplates.WelcomeEmail("ZyntraPay User")
                            );
                        }

                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing WelcomeEmailRequestedEvent");
                        channel.BasicNack(ea.DeliveryTag, false, requeue: true);
                    }
                };

                channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
                _logger.LogInformation("WelcomeEmailConsumer listening...");
                await Task.Delay(Timeout.Infinite, stoppingToken);
                break;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning("RabbitMQ unavailable. Retrying in 5s... {Message}", ex.Message);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
