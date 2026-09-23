using System;
using System.Collections.Generic;
using System.Text;

namespace Organization.Application.DTOs.Organizations
{
    public class OrganizationResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
