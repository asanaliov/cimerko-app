using cimerko_app.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Data;

public static class IdentitySeed {
    public static async Task SeedAsync(IServiceProvider services) {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in AppRoles.All) {
            if (!await roleManager.RoleExistsAsync(roleName)) {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                EnsureSucceeded(result, $"Could not create the {roleName} role.");
            }
        }

        var usersWithoutRoles = await context.Users
            .Where(user => !context.UserRoles.Any(userRole => userRole.UserId == user.Id))
            .ToListAsync();

        foreach (var user in usersWithoutRoles) {
            var ownsListing = await context.Listings.AnyAsync(listing => listing.OwnerId == user.Id);
            var roleName = ownsListing ? AppRoles.Landlord : AppRoles.Student;
            var result = await userManager.AddToRoleAsync(user, roleName);
            EnsureSucceeded(result, $"Could not assign a role to user {user.Id}.");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string message) {
        if (result.Succeeded) {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"{message} {errors}");
    }
}
