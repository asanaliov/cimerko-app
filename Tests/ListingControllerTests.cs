using System.Security.Claims;
using cimerko_app.Controllers;
using cimerko_app.Models;
using cimerko_app.Models.Enums;
using cimerko_app.Models.ViewModels;
using cimerko_app.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace Tests;

public class ListingControllerTests {
    [Fact]
    public async Task Index_rejects_a_maximum_budget_below_the_minimum_budget() {
        await using var database = await TestDatabase.CreateAsync();
        var controller = new ListingController(
            database.Context,
            new LocalImageStorage(Mock.Of<IWebHostEnvironment>())) {
            ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await controller.Index(new ListingIndexViewModel {
            MinimumBudget = 600,
            MaximumBudget = 500
        });

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        var error = Assert.Single(controller.ModelState[nameof(ListingIndexViewModel.MaximumBudget)]!.Errors);
        Assert.Equal(
            "Maximum budget must be greater than or equal to minimum budget.",
            error.ErrorMessage);
    }

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

    [Fact]
    public async Task Index_filters_rentals_by_real_property_details_and_policies() {
        await using var database = await TestDatabase.CreateAsync();
        var context = database.Context;
        var owner = new ApplicationUser {
            Id = "owner",
            UserName = "owner@example.test",
            Email = "owner@example.test",
            FullName = "Owner"
        };
        context.Users.Add(owner);
        var studio = CreateListing(owner.Id, "Pet-friendly studio", null);
        studio.BedroomCount = 0;
        studio.RentalPetPolicy = RentalPetPolicy.Allowed;
        studio.TenantTypePreference = TenantTypePreference.Student;

        var apartment = CreateListing(owner.Id, "One-bedroom apartment", null);
        apartment.BedroomCount = 1;
        apartment.RentalPetPolicy = RentalPetPolicy.NoPets;
        context.Listings.AddRange(studio, apartment);
        await context.SaveChangesAsync();

        var controller = new ListingController(
            context,
            new LocalImageStorage(Mock.Of<IWebHostEnvironment>())) {
            ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await controller.Index(new ListingIndexViewModel {
            Type = ListingType.PlaceForRent,
            BedroomCount = 0,
            RentalPetPolicy = RentalPetPolicy.Allowed,
            TenantTypePreference = TenantTypePreference.Student
        });

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ListingIndexViewModel>(view.Model);
        var listing = Assert.Single(model.Listings);
        Assert.Equal("Pet-friendly studio", listing.Title);
    }

    [Fact]
    public async Task Create_requires_a_bedroom_count_for_a_rental() {
        await using var database = await TestDatabase.CreateAsync();
        var context = database.Context;
        context.Users.Add(CreateUser("owner"));
        await context.SaveChangesAsync();
        var controller = CreateAuthenticatedController(context, "owner");
        var listing = ValidListing("owner", ListingType.PlaceForRent);
        listing.BedroomCount = null;

        var result = await controller.Create(listing, null);

        Assert.IsType<ViewResult>(result);
        var error = Assert.Single(
            controller.ModelState[nameof(Listing.BedroomCount)]!.Errors);
        Assert.Equal(
            "Enter the number of bedrooms. Use 0 for a studio.",
            error.ErrorMessage);
        Assert.Empty(context.Listings);
    }

    [Fact]
    public async Task Create_keeps_roommate_bedrooms_and_preferences_separate_from_rental_policies() {
        await using var database = await TestDatabase.CreateAsync();
        var context = database.Context;
        context.Users.Add(CreateUser("owner"));
        await context.SaveChangesAsync();
        var controller = CreateAuthenticatedController(context, "owner");
        var listing = ValidListing("owner", ListingType.LookingForRoommate);
        listing.RoommatesNeeded = 1;
        listing.RoommateHousingPlan = RoommateHousingPlan.HavePlace;
        listing.RoommateGenderPreference = RoommateGenderPreference.Women;
        listing.BedroomCount = 3;
        listing.TenantTypePreference = TenantTypePreference.Student;
        listing.RentalSmokingPolicy = RentalSmokingPolicy.Allowed;
        listing.RentalPetPolicy = RentalPetPolicy.Allowed;

        var result = await controller.Create(listing, null);

        Assert.IsType<RedirectToActionResult>(result);
        var savedListing = Assert.Single(context.Listings);
        Assert.Equal(3, savedListing.BedroomCount);
        Assert.Equal(TenantTypePreference.NoPreference, savedListing.TenantTypePreference);
        Assert.Equal(RentalSmokingPolicy.NotSpecified, savedListing.RentalSmokingPolicy);
        Assert.Equal(RentalPetPolicy.NotSpecified, savedListing.RentalPetPolicy);
        Assert.Equal(RoommateGenderPreference.Women, savedListing.RoommateGenderPreference);
    }

    private static ListingController CreateAuthenticatedController(
        cimerko_app.Data.ApplicationDbContext context,
        string userId) {
        return new ListingController(
            context,
            new LocalImageStorage(Mock.Of<IWebHostEnvironment>())) {
            TempData = Mock.Of<ITempDataDictionary>(),
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

    private static Listing ValidListing(string ownerId, ListingType type) {
        return new Listing {
            OwnerId = ownerId,
            Title = "Valid listing",
            Description = "A valid listing.",
            Type = type,
            City = "Skopje",
            ContactPhone = "+389 70 123 456",
            MonthlyRent = 400,
            RoomCount = 2,
            BedroomCount = 1
        };
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
            ContactPhone = "+49 30 123456",
            MonthlyRent = 500,
            RoomCount = 1,
            BedroomCount = 1,
            IsActive = true,
            ModerationStatus = ListingModerationStatus.Approved,
            AvailableFrom = availableFrom
        };

        if (image != null) {
            listing.Images.Add(image);
        }

        return listing;
    }
}
