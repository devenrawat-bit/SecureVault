using System;
using System.Collections.Generic;
using System.Text;

namespace Organization.Application.Events
{
    public class OrganizationCreatedEvent
    {
        public Guid OrganizationId { get; set; }

        public string AdminEmail { get; set; } = string.Empty;

        public string AdminFirstName { get; set; } = string.Empty;

        public string AdminLastName { get; set; } = string.Empty;
    }
}
