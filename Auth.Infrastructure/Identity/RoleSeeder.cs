using Microsoft.AspNetCore.Identity;

namespace Auth.Infrastructure.Identity
{
    /// <summary>
    /// This checks that if the role exists in the db or not, if not then it will put that role in the db 
    /// </summary>
    public class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<IdentityRole<Guid>>
            roleManager)
        {
            {
                string[] roles =
                [
                    "Super Admin",
            "Organization Admin",
            "Project Manager",
            "Developer",
            "QA Engineer"
                ];

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role)) //this will help prevent the duplicate role creation in the db, if the role already exists then it will not create it again
                    {
                        await roleManager.CreateAsync(
                            new IdentityRole<Guid>
                            {
                                Name = role,
                                NormalizedName = role.ToUpperInvariant()
                            });
                    }
                }
            }
        }
    }
}
