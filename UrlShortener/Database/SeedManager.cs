using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Models;

namespace UrlShortener.Database;

public static class SeedManager
{
    public static async Task Seed(IServiceProvider services)
    {
        await SeedRoles(services);
        await SeedAdminUser(services);
    }

    private static async Task SeedRoles(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await roleManager.CreateAsync(new IdentityRole(Role.Admin));
        await roleManager.CreateAsync(new IdentityRole(Role.User));
    }

    private static async Task SeedAdminUser(IServiceProvider services)
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        var adminUser = await context.Users.FirstOrDefaultAsync(user => user.UserName == "admin");

        if (adminUser == null)
        {
            adminUser = new User { UserName = "AuthenticationAdmin", Email = "admin@mail.com" };
            await userManager.CreateAsync(adminUser, "Admin-123");
            await userManager.AddToRoleAsync(adminUser, Role.Admin);
        }
    }
}