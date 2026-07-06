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
    public async Task Index_returns_platform_counts_and_moderation_queues() {
        await using var database = await TestDatabase.CreateAsync();
        var context = database.Context;
        var olderUser = CreateUser("older", new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc));
        var newerUser = CreateUser("newer", new DateTime(2026, 7, 2, 10, 0, 0, DateTimeKind.Utc));
        context.Users.AddRange(olderUser, newerUser);
        context.Listings.AddRange(
            CreateListing(
                "Approved listing",
                olderUser.Id,
                ListingModerationStatus.Approved,
                new DateTime(2026, 7, 3, 10, 0, 0, DateTimeKind.Utc)),
            CreateListing(
                "Pending listing",
                newerUser.Id,
                ListingModerationStatus.Pending,
                new DateTime(2026, 7, 4, 10, 0, 0, DateTimeKind.Utc)));
        await context.SaveChangesAsync();

        var controller = new AdminController(context);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AdminDashboardViewModel>(view.Model);
        Assert.Equal(2, model.UserCount);
        Assert.Equal(2, model.ListingCount);
        Assert.Equal(1, model.PendingListingCount);
        Assert.Equal(0, model.OpenReportCount);
        Assert.Equal(["Pending listing"], model.RecentPendingListings.Select(listing => listing.Title));
        Assert.Empty(model.RecentOpenReports);
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
        ListingModerationStatus status,
        DateTime createdAt) {
        return new Listing {
            OwnerId = ownerId,
            Title = title,
            Description = "Test listing",
            Type = ListingType.PlaceForRent,
            City = "Berlin",
            MonthlyRent = 500,
            RoomCount = 1,
            IsActive = status == ListingModerationStatus.Approved,
            ModerationStatus = status,
            CreatedAt = createdAt
        };
    }
}
