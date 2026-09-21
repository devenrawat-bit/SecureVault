using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.Application.DTOs.Invitations
{
    public class InvitationCreatedResponse
    {
        public Guid InvitationId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string InvitationToken { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
    }
}
