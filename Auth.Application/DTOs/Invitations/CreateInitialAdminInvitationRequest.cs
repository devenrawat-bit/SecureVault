using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.Application.DTOs.Invitations
{
    public class CreateInitialAdminInvitationRequest
    {
        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
    }
}
