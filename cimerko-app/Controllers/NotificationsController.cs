using System.Security.Claims;
using cimerko_app.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Controllers;

[Authorize]
public class NotificationsController : Controller {
    private readonly ApplicationDbContext _context;

    public NotificationsController(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<IActionResult> Index() {
        var userId = CurrentUserId();
        if (userId == null) {
            return Challenge();
        }

        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(notification => notification.RecipientId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .ToListAsync();

        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id) {
        var userId = CurrentUserId();
        if (userId == null) {
            return Challenge();
        }

        var notification = await _context.Notifications.FirstOrDefaultAsync(item =>
            item.Id == id && item.RecipientId == userId);

        if (notification == null) {
            return NotFound();
        }

        if (!notification.IsRead) {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead() {
        var userId = CurrentUserId();
        if (userId == null) {
            return Challenge();
        }

        var unreadNotifications = await _context.Notifications
            .Where(notification => notification.RecipientId == userId && !notification.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications) {
            notification.IsRead = true;
        }

        if (unreadNotifications.Count > 0) {
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private string? CurrentUserId() {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
