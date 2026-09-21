using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.Domain.Entities
{
    public class Invitation
    {
        /// <summary>
        /// Unique ID for the invitation
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The id of the org the invited user belongs to 
        /// </summary>
        public Guid OrganizationId { get; set; }

        /// <summary>
        /// Email address receiving the invitation
        /// </summary>
        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Role assigned by the Organization Admin
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Hash of the invitation token, not the raw token
        /// </summary>
        public string TokenHash { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// When the invitation was accepted; null means unused
        /// </summary>
        public DateTime? UsedAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
