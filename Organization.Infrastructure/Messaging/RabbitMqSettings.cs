using System;
using System.Collections.Generic;
using System.Text;

namespace Organization.Infrastructure.Messaging
{
    public class RabbitMqSettings
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
