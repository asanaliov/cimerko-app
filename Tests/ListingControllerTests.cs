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

public class ListingControllerTests {
    [Fact]
    public async Task Index_applies_available_now_and_has_images_filters_together() {
        await using var database = await TestDatabase.CreateAsync();
        var context = database.Context;
        var owner = new ApplicationUser {
            Id = "owner",
            UserName = "owner@example.test",
            Email = "owner@example.test",
            FullName = "Owner"
        };
        context.Users.Add(owner);
        context.Listings.AddRange(
            CreateListing(
                owner.Id,
                "Available with photo",
                DateTime.UtcNow.Date,
                new ListingImage { ImageUrl = "/uploads/listings/available.jpg" }),
            CreateListing(
                owner.Id,
                "Future with photo",
                DateTime.UtcNow.Date.AddDays(7),
                new ListingImage { ImageUrl = "/uploads/listings/future.jpg" }),
            CreateListing(owner.Id, "Available without photo", null));
        await context.SaveChangesAsync();

        var controller = new ListingController(
            context,
            new LocalImageStorage(Mock.Of<IWebHostEnvironment>())) {
            ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await controller.Index(new ListingIndexViewModel {
            AvailableNow = true,
            HasImages = true
        });

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ListingIndexViewModel>(view.Model);
        var listing = Assert.Single(model.Listings);
        Assert.Equal("Available with photo", listing.Title);
    }

    private static Listing CreateListing(
        string ownerId,
        string title,
        DateTime? availableFrom,
        ListingImage? image = null) {
        var listing = new Listing {
            OwnerId = ownerId,
            Title = title,
            Description = "Test listing",
            Type = ListingType.PlaceForRent,
            City = "Berlin",
            MonthlyRent = 500,
            RoomCount = 1,
            AvailableFrom = availableFrom
        };

        if (image != null) {
            listing.Images.Add(image);
        }

        return listing;
    }
}
