using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.Infrastructure.Messaging
{
    /// <summary>
    /// we are here creating the consumer infrsatructure for the rabbitmq, so we need to create the settings for the rabbitmq
    /// </summary>
    public class RabbitMqSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
