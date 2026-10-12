using Auth.Application.DTOs.Invitations;
using Auth.Application.Interfaces;
using Auth.Infrastructure.Messaging.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Auth.Infrastructure.Messaging
{
    /// <summary>
    /// we are creating the infra of the rabbitmq consumer, which will be used to consume the messages from the rabbitmq queue.
    /// </summary>
    public class RabbitMqConsumer : BackgroundService
    {
        private readonly RabbitMqSettings _settings;
        private readonly IServiceScopeFactory _scopeFactory; //part of DI
        private readonly string _frontendBaseUrl;

        public RabbitMqConsumer(
            IOptions<RabbitMqSettings> settings,
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration)
        {
            _settings = settings.Value;
            _scopeFactory = scopeFactory;

            //Read the frontend URL from the configuration.
            _frontendBaseUrl =
                configuration["Frontend:BaseUrl"]
                ?? throw new InvalidOperationException(
                    "Frontend:BaseUrl configuration is missing.");
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken) //this automatically gets called by the asp.net core when the application starts
        {
            //telling the rabbit mq how to create the connection
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password
            };

            await using var connection =
                await factory.CreateConnectionAsync(stoppingToken);

            await using var channel =
                await connection.CreateChannelAsync(
                    cancellationToken: stoppingToken);

            const string exchangeName = "securevault.events";
            const string queueName = "auth.organization-created";
            const string routingKey = nameof(OrganizationCreatedEvent); //simply gives the class name as a string

            //Create and ensure an exchange called securevault.events
            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: stoppingToken);

            //create and ensure the queue called auth.organization-created
            //The queue is where the event waits until the Auth Service's consumer processes it.
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken);

            //Bind the queue to the exchange and route messages by the routing key into this queue.
            await channel.QueueBindAsync(
                queue: queueName,
                exchange: exchangeName,
                routingKey: routingKey,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);

            //what to do when the message arrives from the rabbit mq
            consumer.ReceivedAsync += async (sender, args) => //this code tells that when the rabbit mq consumer gets any message received then run this code
            {
                try
                {
                    var body = args.Body.ToArray();

                    var message = Encoding.UTF8.GetString(body); //in this message var the data that is transmitted from the organization.api is stored in the form of string, so we need to deserialize it into the OrganizationCreatedEvent class object

                    //print the data that the organization.api transmit
                    Console.WriteLine($"Received event: {message}");

                    var organizationCreatedEvent =
                        JsonSerializer.Deserialize<OrganizationCreatedEvent>(message);

                    if (organizationCreatedEvent is null ||
                        organizationCreatedEvent.OrganizationId == Guid.Empty ||
                        string.IsNullOrWhiteSpace(
                            organizationCreatedEvent.AdminEmail) ||
                        string.IsNullOrWhiteSpace(
                            organizationCreatedEvent.OrganizationName))
                    {
                        Console.WriteLine("Invalid OrganizationCreatedEvent received.");

                        //Reject invalid messages instead of repeatedly processing them.
                        //Configure a dead-letter queue before production deployment.
                        await channel.BasicNackAsync(
                            deliveryTag: args.DeliveryTag,
                            multiple: false,
                            requeue: false);

                        return;
                    }

                    Console.WriteLine(
                        $"Organization created: {organizationCreatedEvent.OrganizationId}");

                    Console.WriteLine(
                        $"Admin email: {organizationCreatedEvent.AdminEmail}");

                    var request = new CreateInitialAdminInvitationRequest
                    {
                        Email = organizationCreatedEvent.AdminEmail,
                        FirstName = organizationCreatedEvent.AdminFirstName,
                        LastName = organizationCreatedEvent.AdminLastName,
                    };

                    //now pass this info into the invitation service
                    using var scope = _scopeFactory.CreateScope();

                    Console.WriteLine("data is transfering further");

                    var invitationService =
                        scope.ServiceProvider.GetRequiredService<IInvitationService>();

                    //Resolve the publisher that will publish the invitation event to RabbitMQ.
                    var eventPublisher =
                        scope.ServiceProvider.GetRequiredService<IEventPublisher>();

                    //Create the invitation and save it to the database.
                    var invitationResponse =
                        await invitationService.CreateInitialAdminInvitationAsync(
                            request,
                            organizationCreatedEvent.OrganizationId);

                    //the role was not supplied here as the role is already exist in the invitation service as hardcoded.
                    Console.WriteLine(
                        "data is transfered to the invitation service.");

                    //Build the invitation link using the configured frontend URL and the raw token.
                    var invitationLink =
                        $"{_frontendBaseUrl.TrimEnd('/')}/accept-invitation" +
                        $"?token={Uri.EscapeDataString(invitationResponse.InvitationToken)}";

                    //Create the message that will be sent to the Notification Service.
                    var invitationMessage = new InvitationCreatedMessage
                    {
                        InvitationId = invitationResponse.InvitationId,
                        RecipientEmail = invitationResponse.Email,
                        RecipientName =
                            $"{organizationCreatedEvent.AdminFirstName} " +
                            $"{organizationCreatedEvent.AdminLastName}".Trim(),
                        OrganizationName =
                            organizationCreatedEvent.OrganizationName,
                        InvitationLink = invitationLink
                    };

                    //Publish InvitationCreatedEvent so the Notification Service can send the invitation email.
                    await eventPublisher.PublishInvitationCreatedAsync(
                        invitationMessage,
                        stoppingToken);

                    Console.WriteLine(
                        "InvitationCreatedEvent published to RabbitMQ.");

                    //this line tells the rabbit that i succesfully consume the message now you can mark it as acknowledge or sending an ack back to the rabbit
                    //Acknowledge only after invitation creation and event publishing succeed.
                    await channel.BasicAckAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Failed to process OrganizationCreatedEvent: {ex.Message}");

                    //Reject the message instead of requeueing indefinitely.
                    //A dead-letter queue and idempotent processing should be implemented before production.
                    await channel.BasicNackAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: false);
                }
            };

            //Manual acknowledgement is enabled, so we ACK after successful processing.
            //this code block always run first before the received async method
            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false, //now the rabbit mq will wait for us to explicitly say that the message is been successfully processed
                consumer: consumer,
                cancellationToken: stoppingToken);

            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }
    }
}