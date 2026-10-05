using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisDal.Data.DataSeed;

public static class IdentityDbContextSeeding
{
    public static bool SeedData(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var hasUsers = userManager.Users.Any();
        var hasRoles = roleManager.Roles.Any();

        if (!hasRoles)
        {
            var roles = new[]
            {
                new IdentityRole("SuperAdmin"),
                new IdentityRole("Admin"),
                new IdentityRole("Doctor"),
                new IdentityRole("Staff")
            };

            foreach (var role in roles)
            {
                if (!roleManager.RoleExistsAsync(role.Name!).GetAwaiter().GetResult())
                {
                    var createRoleResult = roleManager.CreateAsync(role).GetAwaiter().GetResult();
                    if (!createRoleResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Failed to create role '{role.Name}': {string.Join(", ", createRoleResult.Errors.Select(error => error.Code))}");
                    }
                }
            }
        }

        if (hasUsers)
            return false;

        var email = configuration["InitialAdmin:Email"];
        var password = configuration["InitialAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine(
                "Initial administrator was not created. Configure InitialAdmin:Email and InitialAdmin:Password to create one.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "InitialAdmin:Email and InitialAdmin:Password must both be configured.");
        }

        var initialAdmin = new ApplicationUser
        {
            FirstName = configuration["InitialAdmin:FirstName"] ?? "Site",
            LastName = configuration["InitialAdmin:LastName"] ?? "Administrator",
            UserName = email,
            Email = email,
            PhoneNumber = configuration["InitialAdmin:PhoneNumber"]
        };

        var createResult = userManager.CreateAsync(initialAdmin, password).GetAwaiter().GetResult();
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create initial administrator: {string.Join(", ", createResult.Errors.Select(error => error.Code))}");
        }

        var assignAdminRoleResult = userManager.AddToRoleAsync(initialAdmin, "SuperAdmin").GetAwaiter().GetResult();
        if (!assignAdminRoleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to assign the SuperAdmin role: {string.Join(", ", assignAdminRoleResult.Errors.Select(error => error.Code))}");
        }

        return true;
    }
}
