using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Infrastructure.Messaging.Events
{
    /// <summary>
    /// The auth will publish the data for the password reset that event is here deserialized 
    /// </summary>
    public class PasswordResetRequestedEvent
    {
        public Guid UserId { get; set; }

        public string RecipientEmail { get; set; } = string.Empty;

        public string ResetLink { get; set; } = string.Empty;
    }
}
