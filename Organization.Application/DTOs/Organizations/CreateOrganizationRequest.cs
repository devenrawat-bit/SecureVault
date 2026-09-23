using System;
using System.Collections.Generic;
using System.Text;

namespace Organization.Application.DTOs.Organizations
{
    public class CreateOrganizationRequest
    {
        /// <summary>
        /// these are the two properties that the super admin will provide, notice the id of the org is not supplied 
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string AdminFirstName { get; set; } = string.Empty;
        public string AdminLastName { get; set; } = string.Empty;
    }
}
