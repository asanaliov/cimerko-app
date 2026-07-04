using System.Security.Claims;
using cimerko_app.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.ViewComponents;

public class NotificationNavViewComponent : ViewComponent {
    private readonly ApplicationDbContext _context;

    public NotificationNavViewComponent(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync() {
        var userId = CurrentUserId();
        if (userId == null) {
            return Content(string.Empty);
        }

        var unreadCount = await _context.Notifications.CountAsync(notification =>
            notification.RecipientId == userId && !notification.IsRead);

        return View(unreadCount);
    }

    private string? CurrentUserId() {
        return UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
