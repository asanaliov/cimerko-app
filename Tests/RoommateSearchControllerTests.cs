using cimerko_app.Controllers;
using cimerko_app.Models;
using cimerko_app.Models.Enums;
using cimerko_app.Models.ViewModels;
using cimerko_app.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests;

public class RoommateSearchControllerTests {
    [Fact]
    public async Task Roommates_applies_profile_and_listing_filters() {
        await using var database = await TestDatabase.CreateAsync();
        var context = database.Context;
        AddRoommate(
            context,
            "matching",
            "Female",
            "No smoking",
            "Skopje",
            350,
            ListingModerationStatus.Approved);
        AddRoommate(
            context,
            "wrong-gender",
            "Male",
            "No smoking",
            "Skopje",
            350,
            ListingModerationStatus.Approved);
        AddRoommate(
            context,
            "pending",
            "Female",
            "No smoking",
            "Skopje",
            300,
            ListingModerationStatus.Pending);
        await context.SaveChangesAsync();

        var controller = new ProfileController(
            context,
            new LocalImageStorage(Mock.Of<IWebHostEnvironment>())) {
            ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await controller.Roommates(new RoommateSearchViewModel {
            City = "Skopje",
            MaximumBudget = 400,
            Gender = "Female",
            SmokingPreference = "No smoking",
            HousingPlan = RoommateHousingPlan.HavePlace
        });

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RoommateSearchViewModel>(view.Model);
        var match = Assert.Single(model.Results);
        Assert.Equal("matching", match.User.Id);
        Assert.Equal("matching listing", match.Listing.Title);
    }

    private static void AddRoommate(
        cimerko_app.Data.ApplicationDbContext context,
        string id,
        string gender,
        string smokingPreference,
        string city,
        decimal rent,
        ListingModerationStatus moderationStatus) {
        var user = new ApplicationUser {
            Id = id,
            UserName = $"{id}@example.test",
            Email = $"{id}@example.test",
            FullName = id,
            RoommateProfile = new RoommateProfile {
                UserId = id,
                City = city,
                Gender = gender,
                SmokingPreference = smokingPreference
            }
        };
        context.Users.Add(user);
        context.Listings.Add(new Listing {
            OwnerId = id,
            Title = $"{id} listing",
            Description = "Roommate listing",
            Type = ListingType.LookingForRoommate,
            City = city,
            ContactPhone = "+389 70 123 456",
            MonthlyRent = rent,
            RoomCount = 2,
            RoommatesNeeded = 1,
            RoommateHousingPlan = RoommateHousingPlan.HavePlace,
            IsActive = moderationStatus == ListingModerationStatus.Approved,
            ModerationStatus = moderationStatus
        });
    }
}
