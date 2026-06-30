using System.Security.Claims;
using cimerko_app.Data;
using cimerko_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Controllers;

[Authorize]
public class ListingController : Controller {
    private readonly ApplicationDbContext _context;

    public ListingController(ApplicationDbContext context) {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? title, string? city) {
        var query = _context.Listings
            .Include(listing => listing.Owner)
            .Where(listing => listing.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(title)) {
            query = query.Where(listing => listing.Title.Contains(title));
        }

        if (!string.IsNullOrWhiteSpace(city)) {
            query = query.Where(listing => listing.City.Contains(city));
        }

        ViewBag.TitleFilter = title;
        ViewBag.CityFilter = city;

        return View(await query
            .OrderByDescending(listing => listing.CreatedAt)
            .ToListAsync());
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var listing = await _context.Listings
            .Include(item => item.Owner)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (listing == null) {
            return NotFound();
        }

        return View(listing);
    }

    public IActionResult Create() {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,Description,Type,City,Address,MonthlyRent,RoomCount,AvailableFrom")] Listing listing) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) {
            return Challenge();
        }

        listing.OwnerId = userId;
        listing.CreatedAt = DateTime.UtcNow;
        listing.IsActive = true;
        ModelState.Remove(nameof(Listing.OwnerId));

        if (ModelState.IsValid) {
            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(listing);
    }

    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var listing = await _context.Listings.FindAsync(id);
        if (listing == null || listing.OwnerId != CurrentUserId()) {
            return NotFound();
        }

        return View(listing);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Title,Description,Type,City,Address,MonthlyRent,RoomCount,AvailableFrom,IsActive")]
        Listing formListing) {
        if (id != formListing.Id) {
            return NotFound();
        }

        var listing = await _context.Listings.FindAsync(id);
        if (listing == null || listing.OwnerId != CurrentUserId()) {
            return NotFound();
        }

        ModelState.Remove(nameof(Listing.OwnerId));

        if (!ModelState.IsValid) {
            return View(formListing);
        }

        listing.Title = formListing.Title;
        listing.Description = formListing.Description;
        listing.Type = formListing.Type;
        listing.City = formListing.City;
        listing.Address = formListing.Address;
        listing.MonthlyRent = formListing.MonthlyRent;
        listing.RoomCount = formListing.RoomCount;
        listing.AvailableFrom = formListing.AvailableFrom;
        listing.IsActive = formListing.IsActive;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = listing.Id });
    }

    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var listing = await _context.Listings
            .Include(item => item.Owner)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (listing == null || listing.OwnerId != CurrentUserId()) {
            return NotFound();
        }

        return View(listing);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id) {
        var listing = await _context.Listings.FindAsync(id);
        if (listing == null || listing.OwnerId != CurrentUserId()) {
            return NotFound();
        }

        _context.Listings.Remove(listing);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private string? CurrentUserId() {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
