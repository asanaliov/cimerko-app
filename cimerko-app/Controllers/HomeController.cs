using System.Diagnostics;
using cimerko_app.Data;
using Microsoft.AspNetCore.Mvc;
using cimerko_app.Models;
using cimerko_app.Models.ViewModels;
using cimerko_app.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Controllers;

public class HomeController : Controller {
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<IActionResult> Index() {
        var latestListings = await _context.Listings
            .AsNoTracking()
            .Where(listing =>
                listing.IsActive &&
                listing.ModerationStatus == ListingModerationStatus.Approved)
            .Include(listing => listing.Owner)
            .Include(listing => listing.Images)
            .OrderByDescending(listing => listing.CreatedAt)
            .Take(3)
            .ToListAsync();

        return View(new HomeIndexViewModel {
            LatestListings = latestListings
        });
    }

    public IActionResult Privacy() {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
