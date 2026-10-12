using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Infrastructure.Messaging.Events
{
    public class InvitationCreatedEvent
    {
        public Guid InvitationId { get; set; }

        public string RecipientEmail { get; set; } = string.Empty;

        public string RecipientName { get; set; } = string.Empty;

        public string OrganizationName { get; set; } = string.Empty;

        public string InvitationLink { get; set; } = string.Empty;
    }
}
