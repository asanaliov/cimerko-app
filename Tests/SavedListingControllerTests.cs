using System.Security.Claims;
using cimerko_app.Controllers;
using cimerko_app.Models;
using cimerko_app.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tests;

public class SavedListingControllerTests {
    [Fact]
    public async Task Save_does_not_create_duplicate_saved_listings() {
        await using var database = await TestDatabase.CreateAsync();
        var context = database.Context;
        context.Users.AddRange(
            CreateUser("owner"),
            CreateUser("student"));
        var listing = CreateListing("owner");
        context.Listings.Add(listing);
        await context.SaveChangesAsync();
        var controller = CreateController(context, "student");

        await controller.Save(listing.Id, null);
        await controller.Save(listing.Id, null);

        var savedListing = Assert.Single(context.SavedListings);
        Assert.Equal("student", savedListing.UserId);
        Assert.Equal(listing.Id, savedListing.ListingId);
    }

    [Fact]
    public async Task Save_does_not_allow_an_owner_to_save_their_own_listing() {
        await using var database = await TestDatabase.CreateAsync();
        var context = database.Context;
        context.Users.Add(CreateUser("owner"));
        var listing = CreateListing("owner");
        context.Listings.Add(listing);
        await context.SaveChangesAsync();
        var controller = CreateController(context, "owner");

        var result = await controller.Save(listing.Id, null);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Empty(context.SavedListings);
    }

    private static SavedListingController CreateController(
        cimerko_app.Data.ApplicationDbContext context,
        string userId) {
        return new SavedListingController(context) {
            ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, userId)],
                        "TestAuthentication"))
                }
            }
        };
    }

    private static ApplicationUser CreateUser(string id) {
        return new ApplicationUser {
            Id = id,
            UserName = $"{id}@example.test",
            Email = $"{id}@example.test",
            FullName = id
        };
    }

    private static Listing CreateListing(string ownerId) {
        return new Listing {
            OwnerId = ownerId,
            Title = "Test listing",
            Description = "A test listing.",
            Type = ListingType.PlaceForRent,
            City = "Berlin",
            MonthlyRent = 500,
            RoomCount = 1,
            IsActive = true
        };
    }
}
