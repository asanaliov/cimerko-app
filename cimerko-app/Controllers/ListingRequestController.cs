using System.Security.Claims;
using cimerko_app.Data;
using cimerko_app.Models;
using cimerko_app.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Controllers;

[Authorize]
public class ListingRequestController : Controller {
    private readonly ApplicationDbContext _context;

    public ListingRequestController(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<IActionResult> Index(string view = "received") {
        var userId = CurrentUserId();
        if (userId == null) {
            return Challenge();
        }

        IQueryable<ListingRequest> query;

        if (view == "sent") {
            query = _context.ListingRequests
                .Where(request => request.SenderId == userId)
                .Include(request => request.Listing)
                .ThenInclude(listing => listing!.Owner);
        }
        else {
            view = "received";
            query = _context.ListingRequests
                .Where(request => request.Listing!.OwnerId == userId)
                .Include(request => request.Listing)
                .Include(request => request.Sender);
        }

        ViewBag.CurrentView = view;

        return View(await query
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int listingId, string message) {
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

        var requestExists = await _context.ListingRequests.AnyAsync(request =>
            request.ListingId == listingId && request.SenderId == userId);

        if (requestExists) {
            TempData["RequestMessage"] = "You have already sent a request for this listing.";
            return RedirectToAction("Details", "Listing", new { id = listingId });
        }

        var listingRequest = new ListingRequest {
            ListingId = listingId,
            SenderId = userId,
            Message = message,
            Status = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        if (!TryValidateModel(listingRequest)) {
            TempData["RequestMessage"] = "The message is required and must be at most 1000 characters.";
            return RedirectToAction("Details", "Listing", new { id = listingId });
        }

        _context.ListingRequests.Add(listingRequest);
        await _context.SaveChangesAsync();

        TempData["RequestMessage"] = "Your request was sent.";
        return RedirectToAction("Details", "Listing", new { id = listingId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, RequestStatus status) {
        var userId = CurrentUserId();
        if (userId == null) {
            return Challenge();
        }

        if (status != RequestStatus.Accepted && status != RequestStatus.Rejected) {
            return BadRequest();
        }

        var listingRequest = await _context.ListingRequests
            .Include(request => request.Listing)
            .FirstOrDefaultAsync(request => request.Id == id);

        if (listingRequest == null || listingRequest.Listing?.OwnerId != userId) {
            return NotFound();
        }

        listingRequest.Status = status;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private string? CurrentUserId() {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
