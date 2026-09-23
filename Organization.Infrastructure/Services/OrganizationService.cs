using Organization.Application.DTOs.Organizations;
using Organization.Application.Interfaces;
using Organization.Domain.Entities;
using Organization.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace Organization.Infrastructure.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly AppDbContext _context;
        /// <summary>
        /// this helps in the crud operation on the db
        /// </summary>
        /// <param name="context"></param>
        public OrganizationService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new organization in the database.
        /// </summary>
        /// <param name="request">The org data that is passed from the super admin only </param>
        /// <returns>The details of the organization that contains the Org id also </returns>
        public async Task<OrganizationResponse> CreateOrganizationAsync(
            CreateOrganizationRequest request)
        {
            var organization = new Organizations
            {
                Id = Guid.NewGuid(), //This one is the organization id, the frontend doesnt send that our backend gnerates it
                Name = request.Name,
                Code = request.Code,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Organizations.AddAsync(organization);
            await _context.SaveChangesAsync();

            return new OrganizationResponse
            {
                Id = organization.Id,
                Name = organization.Name,
                Code = organization.Code,
                IsActive = organization.IsActive,
                CreatedAt = organization.CreatedAt
            };
        }
    }
}
