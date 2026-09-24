using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.Infrastructure.Messaging.Events
{
    /// <summary>
    /// the org project has its own class the same as below one and now this also have the same one in this auth project 
    /// </summary>
    public class OrganizationCreatedEvent
    {
        public Guid OrganizationId { get; set; }

        public string AdminEmail { get; set; } = string.Empty;

        public string AdminFirstName { get; set; } = string.Empty;

        public string AdminLastName { get; set; } = string.Empty;
    }
}
