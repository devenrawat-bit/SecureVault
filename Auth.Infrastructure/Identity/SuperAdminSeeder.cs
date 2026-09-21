using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Auth.Infrastructure.Identity;

public static class SuperAdminSeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IConfiguration configuration)
    {
        const string superAdminRole = "Super Admin";

        if (!await roleManager.RoleExistsAsync(superAdminRole)) //checks whether the role or not, so the role seeder must run first 
        {
            throw new InvalidOperationException(
                $"Required role '{superAdminRole}' does not exist.");
        }

        var email = configuration["BootstrapAdmin:Email"];
        var password = configuration["BootstrapAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Bootstrap Super Admin credentials are not configured.");
        }

        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            if (!await userManager.IsInRoleAsync(existingUser, superAdminRole)) //here the isinroleasync method is used to check if the existing user is already in the super admin role or the exisiting user is already in this role that we are passing 
            {
                await userManager.AddToRoleAsync(
                    existingUser,
                    superAdminRole);
            }

            return;
        }

        //if the existing user is null then create one 
        var superAdmin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Administrator",
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(
            superAdmin,
            password);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Failed to create Super Admin: {errors}");
        }

        await userManager.AddToRoleAsync(
            superAdmin,
            superAdminRole);
    }
}