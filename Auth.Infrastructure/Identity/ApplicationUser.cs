using Microsoft.AspNetCore.Identity;

namespace Auth.Infrastructure.Identity
{
    public class ApplicationUser: IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// This is here intentionally and the auth service is having its own presence of this property and the organisation service will have its own 
        /// </summary>
        public Guid? OrganizationId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }
    }
}
