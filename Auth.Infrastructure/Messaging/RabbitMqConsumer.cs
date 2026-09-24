using Auth.Infrastructure.Messaging.Events;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client.Events;

namespace Auth.Infrastructure.Messaging
{
    /// <summary>
    /// we are creating the infra of the rabbitmq consumer, which will be used to consume the messages from the rabbitmq queue.
    /// </summary>
    public class RabbitMqConsumer : BackgroundService
    {
        private readonly RabbitMqSettings _settings;

        public RabbitMqConsumer(IOptions<RabbitMqSettings> settings)
        {
            _settings = settings.Value;
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

            consumer.ReceivedAsync += async (sender, args) =>
            {
                var body = args.Body.ToArray();

                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Received event: {message}");

                await Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: true,
                consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
