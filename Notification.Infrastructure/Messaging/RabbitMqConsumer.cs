using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Configuration;
using Notification.Infrastructure.Messaging.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notification.Infrastructure.Messaging;

public sealed class RabbitMqConsumer : BackgroundService //sealed is used now this class wont be inherited by any other 
{
    #region Private Fields
    private readonly RabbitMqSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMqConsumer> _logger;
    #endregion

    #region Constructor
    public RabbitMqConsumer(
        IOptions<RabbitMqSettings> settings,
        IServiceScopeFactory scopeFactory,
        ILogger<RabbitMqConsumer> logger)
    {
        _settings = settings.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    #endregion

    /// <summary>
    /// This method is used to consume the messages from the rabbitmq queue that was produced by the Auth service and then process it and send the email to the user.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        //connect to the rabbitMq 
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            Port = _settings.Port,
            UserName = _settings.Username,
            Password = _settings.Password
        };
        //this establish the connection
        await using var connection =
            await factory.CreateConnectionAsync(stoppingToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        #region Initial Admin invitation queue configuration
        const string exchangeName = "securevault.events";
        const string queueName = "notification.invitation-created";
        const string routingKey = "InvitationCreatedEvent";
        #endregion

        #region Reset Password Queue Configuration
        // New password-reset queue configuration.
        const string passwordResetQueueName =
            "notification.password-reset-requested"; //Our queue name in the 
        const string passwordResetRoutingKey =
            "PasswordResetRequestedEvent";
        #endregion

        #region Invitation Event Consumer 
        //check if the exchange is there or not 
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        //check if the queue is there or not 
        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        //checks if that queue, exchange name and the routing key binds with each other 
        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey,
            cancellationToken: stoppingToken);

        // Process one message at a time.
        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        //when the message will come there then this received async will get that message and then process it 
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var body = args.Body.ToArray();

                var json = Encoding.UTF8.GetString(body);

                var invitationEvent =
                    JsonSerializer.Deserialize<InvitationCreatedEvent>(json);

                if (invitationEvent is null ||
                    invitationEvent.InvitationId == Guid.Empty ||
                    string.IsNullOrWhiteSpace(invitationEvent.RecipientEmail) ||
                    string.IsNullOrWhiteSpace(invitationEvent.InvitationLink))
                {
                    _logger.LogWarning(
                        "Received an invalid invitation event.");

                    // Invalid messages are rejected instead of requeued
                    // indefinitely. Configure a dead-letter queue before
                    // using this behavior in production.
                    await channel.BasicNackAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: false);

                    return;
                }

                using var scope = _scopeFactory.CreateScope();

                var invitationEmailService =
                    scope.ServiceProvider
                        .GetRequiredService<IInvitationEmailService>();

                var request = new AdminInvitationEmailRequest
                {
                    RecipientEmail = invitationEvent.RecipientEmail,
                    RecipientName = invitationEvent.RecipientName,
                    OrganizationName = invitationEvent.OrganizationName,
                    InvitationLink = invitationEvent.InvitationLink
                };
                //call the service to send the invitation that we get from the microservice auth where we get the token and other things.
                await invitationEmailService.SendAdminInvitationAsync(
                    request,
                    stoppingToken);

                // Acknowledge only after email submission succeeds.
                await channel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false);

                _logger.LogInformation(
                    "Processed invitation event {InvitationId}.",
                    invitationEvent.InvitationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process an invitation event.");

                // Requeueing can cause repeated processing if the error
                // is permanent. We will add bounded retries and a
                // dead-letter queue before production deployment.
                await channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: false);
            }
        };

        //before the received async method above this will run first and create the connection 
        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Notification Service is consuming queue {QueueName}.",
            queueName);

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
        #endregion

        #region Password Reset Event Consumer
        // Create a separate consumer for password-reset events.
        var passwordResetConsumer = new AsyncEventingBasicConsumer(channel);

        passwordResetConsumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                // Read the message body from RabbitMQ.
                var body = args.Body.ToArray();

                var json = Encoding.UTF8.GetString(body);

                // Deserialize the event published by the Auth service.
                var passwordResetEvent =
                    JsonSerializer.Deserialize<PasswordResetRequestedEvent>(json);

                // Validate the required event properties.
                if (passwordResetEvent is null ||
                    passwordResetEvent.UserId == Guid.Empty ||
                    string.IsNullOrWhiteSpace(passwordResetEvent.RecipientEmail) ||
                    string.IsNullOrWhiteSpace(passwordResetEvent.ResetLink))
                {
                    _logger.LogWarning(
                        "Received an invalid password-reset event.");

                    // Reject invalid messages instead of requeueing them indefinitely.
                    // Configure a dead-letter queue before using this behavior in production.
                    await channel.BasicNackAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: false);

                    return;
                }

                using var scope = _scopeFactory.CreateScope();

                // Resolve the password-reset email service from dependency injection.
                var passwordResetEmailService =
                    scope.ServiceProvider
                        .GetRequiredService<IPasswordResetEmailService>();

                // Send the password-reset email.
                await passwordResetEmailService.SendPasswordResetEmailAsync(
                    passwordResetEvent.UserId,
                    passwordResetEvent.RecipientEmail,
                    passwordResetEvent.ResetLink,
                    stoppingToken);

                // Acknowledge the message only after email submission succeeds.
                await channel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false);

                _logger.LogInformation(
                    "Processed password-reset event for user {UserId}.",
                    passwordResetEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process a password-reset event.");

                // Do not requeue indefinitely; configure bounded retries
                // and a dead-letter queue before production deployment.
                await channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: false);
            }
        };

        // Start consuming password-reset events.
        await channel.BasicConsumeAsync(
            queue: passwordResetQueueName,
            autoAck: false,
            consumer: passwordResetConsumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Notification Service is consuming queue {QueueName}.",
            passwordResetQueueName);

        //before the received async method above this will run first and create the connection 
        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
        #endregion
    }
}
