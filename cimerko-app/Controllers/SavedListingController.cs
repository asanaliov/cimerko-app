using System.Security.Claims;
using cimerko_app.Data;
using cimerko_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Controllers;

[Authorize]
public class SavedListingController : Controller {
    private readonly ApplicationDbContext _context;

    public SavedListingController(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<IActionResult> Index() {
        var userId = CurrentUserId();
        if (userId == null) {
            return Challenge();
        }

        var savedListings = await _context.SavedListings
            .Where(savedListing => savedListing.UserId == userId)
            .Include(savedListing => savedListing.Listing)
            .ThenInclude(listing => listing!.Owner)
            .Include(savedListing => savedListing.Listing)
            .ThenInclude(listing => listing!.Images)
            .OrderByDescending(savedListing => savedListing.SavedAt)
            .ToListAsync();

        return View(savedListings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int listingId, string? returnUrl) {
        var userId = CurrentUserId();
        if (userId == null) {
            return Challenge();
        }

        var listing = await _context.Listings.FindAsync(listingId);
        if (listing == null || !listing.IsActive) {
            return NotFound();
        }

        if (listing.OwnerId == userId) {
            return RedirectToAction("Details", "Listing", new { id = listingId });
        }

        var alreadySaved = await _context.SavedListings.AnyAsync(savedListing =>
            savedListing.UserId == userId && savedListing.ListingId == listingId);

        if (!alreadySaved) {
            _context.SavedListings.Add(new SavedListing {
                UserId = userId,
                ListingId = listingId,
                SavedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        if (IsAjaxRequest()) {
            return Json(new { isSaved = true });
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Details", "Listing", new { id = listingId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int listingId, string? returnTo, string? returnUrl) {
        var userId = CurrentUserId();
        if (userId == null) {
            return Challenge();
        }

        var savedListing = await _context.SavedListings.FirstOrDefaultAsync(item =>
            item.UserId == userId && item.ListingId == listingId);

        if (savedListing != null) {
            _context.SavedListings.Remove(savedListing);
            await _context.SaveChangesAsync();
        }

        if (IsAjaxRequest()) {
            return Json(new { isSaved = false });
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) {
            return LocalRedirect(returnUrl);
        }

        if (returnTo == "details") {
            return RedirectToAction("Details", "Listing", new { id = listingId });
        }

        return RedirectToAction(nameof(Index));
    }

    private bool IsAjaxRequest() {
        return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }

    private string? CurrentUserId() {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
