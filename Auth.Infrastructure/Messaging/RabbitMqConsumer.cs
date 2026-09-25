using Auth.Application.DTOs.Invitations;
using Auth.Application.Interfaces;
using Auth.Infrastructure.Messaging.Events;
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
        private readonly IServiceScopeFactory _scopeFactory;

        public RabbitMqConsumer(IOptions<RabbitMqSettings> settings, IServiceScopeFactory scopeFactory)
        {
            _settings = settings.Value;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) //this automatically gets called by the asp.net core when the application starts 
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
                await factory.CreateConnectionAsync();

            await using var channel =
                await connection.CreateChannelAsync();

            const string exchangeName = "securevault.events";
            const string queueName = "auth.organization-created";
            const string routingKey = nameof(OrganizationCreatedEvent); //simply gives the class name as a string

            //Create and ensure an exchange called securevault.events
            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            //create and ensure the queue called auth.organization-created 
            //The queue is where the event waits until the Auth Service's consumer processes it.
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            //Bind the queue to the exchange and route messages by the routing key into this queue.
            await channel.QueueBindAsync(
                queue: queueName,
                exchange: exchangeName,
                routingKey: routingKey);

            var consumer = new AsyncEventingBasicConsumer(channel);

            //what to do when the message arrives from the rabbit mq 
            consumer.ReceivedAsync += async (sender, args) => //this code tells that when the rabbit mq consumer gets any message received then run this code 
            {
                var body = args.Body.ToArray();

                var message = Encoding.UTF8.GetString(body); //in this message var the data that is transmitted from the organization.api is stored in the form of string, so we need to deserialize it into the OrganizationCreatedEvent class object

                //print the data that the organization.api transmit 
                Console.WriteLine($"Received event: {message}");

                var organizationCreatedEvent = JsonSerializer.Deserialize<OrganizationCreatedEvent>(message);
                if (organizationCreatedEvent is null)
                {
                    Console.WriteLine("Invalid OrganizationCreatedEvent received.");
                    return;
                }
                Console.WriteLine($"Organization created: {organizationCreatedEvent.OrganizationId}");

                Console.WriteLine(
                    $"Admin email: {organizationCreatedEvent.AdminEmail}");
                var request = new CreateInitialAdminInvitationRequest
                {
                    Email=organizationCreatedEvent.AdminEmail,
                    FirstName=organizationCreatedEvent.AdminFirstName,
                    LastName=organizationCreatedEvent.AdminLastName,
                };
                //now pass this info into the invitation service 
                using var scope = _scopeFactory.CreateScope();
                Console.WriteLine("data is transfering further");
                var invitationService =
                    scope.ServiceProvider.GetRequiredService<IInvitationService>();

                await invitationService.CreateInitialAdminInvitationAsync(
                    request,
                    organizationCreatedEvent.OrganizationId);
                //the role was not supplied here as the role is already exist in the invitation service as hardcoded.
                Console.WriteLine("data is transfered to the invitation service");

                //this line tells the rabbit that i succesfully consume the message now you can mark it as acknowledge or sending an ack back to the rabbit 
                await channel.BasicAckAsync(deliveryTag: args.DeliveryTag, multiple: false);
            };

            //Manual acknowledgement is enabled, so we ACK after successful processing.
            //this code block always run first before the received async method 
            await channel.BasicConsumeAsync(
                queue: queueName,   
                autoAck: false, //now the rabbit mq will wait for us to explicitly say that the message is been successfully processed  
                consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
