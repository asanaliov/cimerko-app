using cimerko_app.Controllers;
using cimerko_app.Models;
using cimerko_app.Models.Enums;
using cimerko_app.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Tests;

public class HomeControllerTests {
    [Fact]
    public async Task Index_returns_the_three_latest_active_listings() {
        await using var database = await TestDatabase.CreateAsync();
        var context = database.Context;
        context.Users.Add(new ApplicationUser {
            Id = "owner",
            UserName = "owner@example.test",
            Email = "owner@example.test",
            FullName = "Owner"
        });

        for (var index = 1; index <= 5; index++) {
            context.Listings.Add(CreateListing(
                $"Active {index}",
                isActive: true,
                new DateTime(2026, 7, index, 10, 0, 0, DateTimeKind.Utc)));
        }

        context.Listings.Add(CreateListing(
            "Inactive newest",
            isActive: false,
            new DateTime(2026, 7, 6, 10, 0, 0, DateTimeKind.Utc)));
        await context.SaveChangesAsync();

        var controller = new HomeController(context);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<HomeIndexViewModel>(view.Model);
        Assert.Equal(
            ["Active 5", "Active 4", "Active 3"],
            model.LatestListings.Select(listing => listing.Title));
    }

    private static Listing CreateListing(
        string title,
        bool isActive,
        DateTime createdAt) {
        return new Listing {
            OwnerId = "owner",
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
