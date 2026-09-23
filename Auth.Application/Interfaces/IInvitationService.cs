using Auth.Application.DTOs.Invitations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.Application.Interfaces
{
    public interface IInvitationService
    {
        /// <summary>
        /// This method is used to create an invitation for a user to join an organization.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<InvitationCreatedResponse> CreateInvitationAsync(CreateInvitationRequest request, Guid organizationId);

        /// <summary>
        /// This method is used to create an initial admin invitation for a new organization.
        /// </summary>
        /// <param name="requestorganizationId"></param>
        /// <returns></returns>
        Task<InvitationCreatedResponse> CreateInitialAdminInvitationAsync(CreateInitialAdminInvitationRequest requestorganizationId, Guid OrganizationId);
    }
}
