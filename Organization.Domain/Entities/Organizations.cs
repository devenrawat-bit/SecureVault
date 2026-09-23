using System;
using System.Collections.Generic;
using System.Text;

namespace Organization.Domain.Entities
{
    public class Organizations
    {
        /// <summary>
        /// unique identifier for the organization
        /// </summary>
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }
    }
}
