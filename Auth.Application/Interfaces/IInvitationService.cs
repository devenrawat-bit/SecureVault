using Auth.Application.DTOs.Invitations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.Application.Interfaces
{
    public interface IInvitationService
    {
        Task<InvitationCreatedResponse> CreateInvitationAsync(CreateInvitationRequest request, Guid organizationId);
    }
}
