using System.Security.Claims;
using cimerko_app.Data;
using cimerko_app.Models;
using cimerko_app.Models.ViewModels;
using cimerko_app.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Controllers;

[Authorize]
public class SavedSearchController : Controller {
    private const int MaxSavedSearches = 10;

    private readonly ApplicationDbContext _context;

    public SavedSearchController(ApplicationDbContext context) {
        _context = context;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ListingSearchFilters filters) {
        var userId = CurrentUserId();
        if (userId == null) {
            return Challenge();
        }

        var backToSearch = RedirectToAction("Index", "Listing", filters.ToQueryValues());
        if (!filters.HasActiveFilters) {
            return backToSearch;
        }

        var filtersJson = SavedSearchAlerts.Serialize(filters);
        var userSearches = await _context.SavedSearches
            .Where(search => search.UserId == userId)
            .ToListAsync();

        if (userSearches.Any(search => search.FiltersJson == filtersJson)) {
            TempData["SearchMessage"] = "This search is already saved.";
            return backToSearch;
        }

        if (userSearches.Count >= MaxSavedSearches) {
            TempData["SearchMessage"] = $"You can save up to {MaxSavedSearches} searches. Remove one from your saved page first.";
            return backToSearch;
        }

        _context.SavedSearches.Add(new SavedSearch {
            UserId = userId,
            Name = filters.Describe(),
            FiltersJson = filtersJson,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        TempData["SearchMessage"] = "Search saved. We will notify you when a new listing matches it.";
        return backToSearch;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id) {
        var userId = CurrentUserId();
        var search = await _context.SavedSearches.FirstOrDefaultAsync(item =>
            item.Id == id && item.UserId == userId);

        if (search != null) {
            _context.SavedSearches.Remove(search);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index", "SavedListing");
    }

    private string? CurrentUserId() {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
