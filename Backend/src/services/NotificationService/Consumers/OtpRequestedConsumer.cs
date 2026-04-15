using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NotificationService.Services;

namespace NotificationService.Consumers;

/// <summary>
/// Consumes OTP request events and sends OTP verification emails.
/// </summary>
public class OtpRequestedConsumer : BackgroundService
{
    private readonly RabbitMqConnectionOptions _rabbitMqOptions;
    private readonly ILogger<OtpRequestedConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public OtpRequestedConsumer(
        IOptions<RabbitMqConnectionOptions> rabbitMqOptions,
        ILogger<OtpRequestedConsumer> logger,
        IServiceScopeFactory scopeFactory)
    {
        _rabbitMqOptions = rabbitMqOptions.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Starts RabbitMQ consumption loop with retry behavior on broker unavailability.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = RabbitMqConnectionFactoryBuilder.Create(_rabbitMqOptions);

                var connection = factory.CreateConnection();
                var channel = connection.CreateModel();
                var queueName = RabbitMqQueueConventions.GetQueueName<OtpRequestedEvent>();

                RabbitMqQueueConventions.DeclareQueueWithDeadLetter(channel, queueName);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var @event = JsonSerializer.Deserialize<OtpRequestedEvent>(json);

                        if (@event != null)
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var emailSvc = scope.ServiceProvider.GetRequiredService<IEmailService>();

                            await emailSvc.SendAsync(
                                @event.Email,
                                "ZyntraPay — Email Verification OTP",
                                EmailTemplates.OtpEmail(@event.Otp)
                            );
                        }

                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing OtpRequestedEvent. Sending message to DLQ.");
                        channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                    }
                };

                channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
                _logger.LogInformation("OtpRequestedConsumer listening...");
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

    /// <summary>
    /// Processes a single OTP event by sending the OTP email template.
    /// </summary>
    public async Task ProcessAsync(OtpRequestedEvent @event)
    {
        using var scope = _scopeFactory.CreateScope();
        var emailSvc = scope.ServiceProvider.GetRequiredService<IEmailService>();

        await emailSvc.SendAsync(
            @event.Email,
            "ZyntraPay - Email Verification OTP",
            EmailTemplates.OtpEmail(@event.Otp));
    }
}
