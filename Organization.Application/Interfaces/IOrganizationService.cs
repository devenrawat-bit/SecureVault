using Organization.Application.DTOs.Organizations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Organization.Application.Interfaces
{
    public interface IOrganizationService
    {
        Task<OrganizationResponse> CreateOrganizationAsync(CreateOrganizationRequest request);
    }
}
