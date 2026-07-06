using System.Security.Claims;
using cimerko_app.Controllers;
using cimerko_app.Models;
using cimerko_app.Models.Enums;
using cimerko_app.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace Tests;

public class ListingRequestControllerTests {
    [Fact]
    public async Task Create_does_not_create_duplicate_requests_or_notifications() {
        await using var database = await TestDatabase.CreateAsync();
        var context = database.Context;
        context.Users.AddRange(
            CreateUser("owner"),
            CreateUser("student"));
        var listing = CreateListing("owner");
        context.Listings.Add(listing);
        await context.SaveChangesAsync();
        var controller = CreateController(context, "student");

        await controller.Create(listing.Id, "I would like to view this place.");
        await controller.Create(listing.Id, "Sending the same request again.");

        Assert.Single(context.ListingRequests);
        Assert.Single(context.Notifications);
    }

    [Fact]
    public async Task UpdateStatus_does_not_change_an_already_decided_request() {
        await using var database = await TestDatabase.CreateAsync();
        var context = database.Context;
        context.Users.AddRange(
            CreateUser("owner"),
            CreateUser("student"));
        var listing = CreateListing("owner");
        var request = new ListingRequest {
            Listing = listing,
            SenderId = "student",
            Message = "Request",
            Status = RequestStatus.Rejected
        };
        context.ListingRequests.Add(request);
        await context.SaveChangesAsync();
        var controller = CreateController(context, "owner");

        await controller.UpdateStatus(request.Id, RequestStatus.Accepted);

        Assert.Equal(RequestStatus.Rejected, request.Status);
        Assert.Empty(context.Notifications);
    }

    private static ListingRequestController CreateController(
        cimerko_app.Data.ApplicationDbContext context,
        string userId) {
        return new ListingRequestController(context, new NotificationService(context)) {
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

    private static Listing CreateListing(string ownerId) {
        return new Listing {
            OwnerId = ownerId,
            Title = "Test listing",
            Description = "A test listing.",
            Type = ListingType.PlaceForRent,
            City = "Berlin",
            ContactPhone = "+49 30 123456",
            MonthlyRent = 500,
            RoomCount = 1,
            BedroomCount = 1,
            IsActive = true,
            ModerationStatus = ListingModerationStatus.Approved
        };
    }
}
