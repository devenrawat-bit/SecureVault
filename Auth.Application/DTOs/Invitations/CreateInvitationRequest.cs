using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.Application.DTOs.Invitations
{
    public class CreateInvitationRequest
    {
        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
