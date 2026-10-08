using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Infrastructure.Configuration
{
    /// <summary>
    /// This class is used to configure the SMTP email service options.
    /// </summary>
    public class SmtpOptions
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string FromEmail { get; set; } = string.Empty;

        public string FromName { get; set; } = string.Empty;
    }
}
