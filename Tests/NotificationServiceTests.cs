using cimerko_app.Models;
using cimerko_app.Services;

namespace Tests;

public class NotificationServiceTests {
    [Fact]
    public async Task Add_queues_a_notification_with_a_safe_local_link() {
        await using var database = await TestDatabase.CreateAsync();
        var context = database.Context;
        context.Users.AddRange(
            CreateUser("recipient"),
            CreateUser("actor"));
        var service = new NotificationService(context);

        service.Add(
            "recipient",
            "actor",
            "New review",
            "You received a review.",
            "/Profile/Details/recipient");
        await context.SaveChangesAsync();

        var notification = Assert.Single(context.Notifications);
        Assert.Equal("recipient", notification.RecipientId);
        Assert.Equal("actor", notification.ActorId);
        Assert.Equal("/Profile/Details/recipient", notification.LinkUrl);
        Assert.False(notification.IsRead);
    }

    [Theory]
    [InlineData("https://example.test")]
    [InlineData("//example.test/path")]
    [InlineData("/\\example.test/path")]
    public async Task Add_rejects_non_local_links(string linkUrl) {
        await using var database = await TestDatabase.CreateAsync();
        var service = new NotificationService(database.Context);

        Assert.Throws<ArgumentException>(() =>
            service.Add("recipient", null, "Title", "Message", linkUrl));
    }

    private static ApplicationUser CreateUser(string id) {
        return new ApplicationUser {
            Id = id,
            UserName = $"{id}@example.test",
            Email = $"{id}@example.test",
            FullName = id
        };
    }
}
