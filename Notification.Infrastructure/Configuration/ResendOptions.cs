using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Infrastructure.Configuration
{
    /// <summary>
    /// This class is used to configure the Resend email service options.
    /// </summary>
    public class ResendOptions
    {
        public string ApiKey { get; set; } = string.Empty;

        public string FromEmail { get; set; } = string.Empty;

        public string FromName { get; set; } = string.Empty;
    }
}
