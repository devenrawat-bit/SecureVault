using Microsoft.Extensions.Options;
using Organization.Application.Interfaces;
using Organization.Infrastructure.Messaging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Organization.Infrastructure.Services
{
    /// <summary>
    /// EventService is responsible for publishing events to the RabbitMQ. This event service is our publisher which sends the message
    /// </summary>
    public class EventService : IEventPublisher
    {
        private readonly RabbitMqSettings _settings;

        public EventService(
            IOptions<RabbitMqSettings> settings)
        {
            _settings = settings.Value;
        }
        public async Task PublishAsync<T>(T @event) //treat it as a simple variable ->@event, in reality the event is a keyword in c# so we need to use @ to escape it
        {
            //creating the rabbitmq connection factory with the settings from the configuration, think of it as a connection template, we are telling rabbitmq where, which port, username and password to connect to the rabbitmq server, we are not connected yet, just preparing the connection factory to connect through it
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port, 
                UserName = _settings.Username,
                Password = _settings.Password
            };

            //now connecting to the rabbit mq
            await using var connection =
                await factory.CreateConnectionAsync();

            //creating the path from where the event travels
            await using var channel =
                await connection.CreateChannelAsync();

            //The publisher doesn't normally send directly to a specific queue. It sends the message to an exchange. The exchange looks at the routing key and routes the message accordingly.
            const string exchangeName = "securevault.events";

            //The exchange receives the message and decides where it should go.
            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            //convert c# object to the json string
            var message = JsonSerializer.Serialize(@event);
            //convert json into bytes
            var body = Encoding.UTF8.GetBytes(message);

            //adding the metadata to the message
            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            //this line send the message to the exchange
            await channel.BasicPublishAsync(
                exchange: exchangeName, //send this message to the securevault.events exchange
                routingKey: typeof(T).Name,  //if the T = OrganizationCreatedEvent then the routing key will be OrganizationCreatedEvent, this is how the exchange knows where to send the message, it will send it to the queue that is bound to this routing key
                  //The routing key is basically a label telling RabbitMQ what type of message this is.
                mandatory: false,
                basicProperties: properties,
                body: body);
        }
    }
}
