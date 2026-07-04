using System.Security.Claims;
using cimerko_app.Controllers;
using cimerko_app.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Tests;

public class NotificationsControllerTests {
    [Fact]
    public async Task Index_returns_only_current_users_notifications_newest_first() {
        await using var database = await TestDatabase.CreateAsync();
        var context = database.Context;
        context.Users.AddRange(
            CreateUser("recipient"),
            CreateUser("another-user"));
        context.Notifications.AddRange(
            CreateNotification("recipient", "Older", new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc)),
            CreateNotification("recipient", "Newer", new DateTime(2026, 7, 2, 10, 0, 0, DateTimeKind.Utc)),
            CreateNotification("another-user", "Not visible", DateTime.UtcNow));
        await context.SaveChangesAsync();

        var controller = CreateController(context, "recipient");

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var notifications = Assert.IsAssignableFrom<IReadOnlyList<Notification>>(view.Model);
        Assert.Collection(
            notifications,
            notification => Assert.Equal("Newer", notification.Title),
            notification => Assert.Equal("Older", notification.Title));
    }

    [Fact]
    public async Task MarkAsRead_updates_a_notification_owned_by_current_user() {
        await using var database = await TestDatabase.CreateAsync();
        var context = database.Context;
        context.Users.Add(CreateUser("recipient"));
        var notification = CreateNotification("recipient", "Unread", DateTime.UtcNow);
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        var controller = CreateController(context, "recipient");

        var result = await controller.MarkAsRead(notification.Id);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.True(await context.Notifications
            .Where(item => item.Id == notification.Id)
            .Select(item => item.IsRead)
            .SingleAsync());
    }

    [Fact]
    public async Task MarkAsRead_does_not_update_another_users_notification() {
        await using var database = await TestDatabase.CreateAsync();
        var context = database.Context;
        context.Users.AddRange(
            CreateUser("recipient"),
            CreateUser("another-user"));
        var notification = CreateNotification("another-user", "Private", DateTime.UtcNow);
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        var controller = CreateController(context, "recipient");

        var result = await controller.MarkAsRead(notification.Id);

        Assert.IsType<NotFoundResult>(result);
        Assert.False(await context.Notifications
            .Where(item => item.Id == notification.Id)
            .Select(item => item.IsRead)
            .SingleAsync());
    }

    private static NotificationsController CreateController(
        cimerko_app.Data.ApplicationDbContext context,
        string userId) {
        return new NotificationsController(context) {
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

    private static Notification CreateNotification(
        string recipientId,
        string title,
        DateTime createdAt) {
        return new Notification {
            RecipientId = recipientId,
            Title = title,
            Message = $"{title} message",
            CreatedAt = createdAt
        };
    }
}
