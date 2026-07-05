using cimerko_app.Controllers;
using cimerko_app.Data;
using cimerko_app.Models;
using cimerko_app.Models.Enums;
using cimerko_app.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Tests;

public class AdminControllerTests {
    [Fact]
    public void Admin_controller_requires_the_admin_role() {
        var attribute = Assert.Single(
            typeof(AdminController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>());

        Assert.Equal(AppRoles.Admin, attribute.Roles);
    }

    [Fact]
    public async Task Index_returns_platform_counts_and_recent_records() {
        await using var database = await TestDatabase.CreateAsync();
        var context = database.Context;
        var olderUser = CreateUser("older", new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc));
        var newerUser = CreateUser("newer", new DateTime(2026, 7, 2, 10, 0, 0, DateTimeKind.Utc));
        context.Users.AddRange(olderUser, newerUser);
        context.Listings.AddRange(
            CreateListing("Older active", olderUser.Id, true, new DateTime(2026, 7, 3, 10, 0, 0, DateTimeKind.Utc)),
            CreateListing("Newer inactive", newerUser.Id, false, new DateTime(2026, 7, 4, 10, 0, 0, DateTimeKind.Utc)));
        await context.SaveChangesAsync();

        var controller = new AdminController(context);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AdminDashboardViewModel>(view.Model);
        Assert.Equal(2, model.UserCount);
        Assert.Equal(2, model.ListingCount);
        Assert.Equal(1, model.ActiveListingCount);
        Assert.Equal(0, model.PendingRequestCount);
        Assert.Equal(0, model.ReviewCount);
        Assert.Equal(["newer", "older"], model.RecentUsers.Select(user => user.Id));
        Assert.Equal(["Newer inactive", "Older active"], model.RecentListings.Select(listing => listing.Title));
    }

    private static ApplicationUser CreateUser(string id, DateTime createdAt) {
        return new ApplicationUser {
            Id = id,
            UserName = $"{id}@example.test",
            Email = $"{id}@example.test",
            FullName = $"{id} user",
            CreatedAt = createdAt
        };
    }

    private static Listing CreateListing(
        string title,
        string ownerId,
        bool isActive,
        DateTime createdAt) {
        return new Listing {
            OwnerId = ownerId,
            Title = title,
            Description = "Test listing",
            Type = ListingType.PlaceForRent,
            City = "Berlin",
            MonthlyRent = 500,
            RoomCount = 1,
            IsActive = isActive,
            CreatedAt = createdAt
        };
    }
}
