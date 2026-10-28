using Microsoft.AspNetCore.Identity;

namespace ProductsApi.Data;

// Demo only: creates the roles and one admin account on startup.
// The admin password comes from configuration (user secrets), never from code.
public static class DemoSeed
{
    public static async Task RunAsync(IServiceProvider services, IConfiguration config)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole(role));
        }

        var users = services.GetRequiredService<UserManager<IdentityUser>>();
        const string email = "admin@demo.local";
        if (await users.FindByEmailAsync(email) is null)
        {
            var admin = new IdentityUser { UserName = email, Email = email };
            await users.CreateAsync(admin, config["Seed:AdminPassword"]!);
            await users.AddToRoleAsync(admin, "Admin");
        }
    }
}
