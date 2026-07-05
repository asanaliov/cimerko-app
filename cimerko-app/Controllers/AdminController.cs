using cimerko_app.Data;
using cimerko_app.Models.Enums;
using cimerko_app.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminController : Controller {
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<IActionResult> Index() {
        var model = new AdminDashboardViewModel {
            UserCount = await _context.Users.CountAsync(),
            ListingCount = await _context.Listings.CountAsync(),
            ActiveListingCount = await _context.Listings.CountAsync(listing => listing.IsActive),
            PendingRequestCount = await _context.ListingRequests
                .CountAsync(request => request.Status == RequestStatus.Pending),
            ReviewCount = await _context.Reviews.CountAsync(),
            RecentUsers = await _context.Users
                .AsNoTracking()
                .OrderByDescending(user => user.CreatedAt)
                .Take(5)
                .ToListAsync(),
            RecentListings = await _context.Listings
                .AsNoTracking()
                .Include(listing => listing.Owner)
                .OrderByDescending(listing => listing.CreatedAt)
                .Take(5)
                .ToListAsync()
        };

        return View(model);
    }
}
