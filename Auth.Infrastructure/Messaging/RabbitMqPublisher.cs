using System.Text;
using System.Text.Json;
using Auth.Application.DTOs.Invitations;
using Auth.Application.DTOs.PasswordReset;
using Auth.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Auth.Infrastructure.Messaging;

public sealed class RabbitMqPublisher : IEventPublisher
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqPublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task PublishInvitationCreatedAsync(
        InvitationCreatedMessage message,
        CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            Port = _settings.Port,
            UserName = _settings.Username,
            Password = _settings.Password
        };

        await using var connection =
            await factory.CreateConnectionAsync(cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        const string exchangeName = "securevault.events";
        const string routingKey = "InvitationCreatedEvent";

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Published InvitationCreatedEvent for invitation {InvitationId}.",
            message.InvitationId);
    }

    #region Publish Password Reset Request
  
    /// <summary>
    /// This method is used to publish the message in the Rabbit MQ queue name  
    /// </summary>
    /// <param name="message">The data that will be published on the rabbit mq</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task PublishPasswordResetRequestedAsync(PasswordResetRequestedMessage message, CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            Port = _settings.Port,
            UserName = _settings.Username,
            Password = _settings.Password
        };

        await using var connection =
            await factory.CreateConnectionAsync(cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        const string exchangeName = "securevault.events";
        const string routingKey = "PasswordResetRequestedEvent";
        const string queueName = "notification.password-reset-requested";

        // Ensure the exchange exists before publishing the event.
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        // Create the password-reset queue.
        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Bind the queue to the exchange.
        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey);

        // Serialize the message so RabbitMQ can transport it as JSON.
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        // Publish the password-reset event to the Notification Service.
        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Published password-reset event for user {UserId}.",
            message.UserId);
    }
    #endregion
}