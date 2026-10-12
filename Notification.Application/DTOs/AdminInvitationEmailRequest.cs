using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.DTOs
{
    public class AdminInvitationEmailRequest
    {
        /// <summary>
        /// For whom the mail is going to 
        /// </summary>
        public string RecipientEmail { get; set; } = string.Empty;

        /// <summary>
        /// Name of the receiver
        /// </summary>
        public string RecipientName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the organization
        /// </summary>
        public string OrganizationName { get; set; } = string.Empty;

        /// <summary>
        /// The link by which the admin will join the organization and sets his password 
        /// </summary>
        public string InvitationLink { get; set; } = string.Empty;
    }
}
