using System.Security.Claims;
using cimerko_app.Data;
using cimerko_app.Models;
using cimerko_app.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Controllers;

[Authorize]
public class ReviewController : Controller {
    private readonly ApplicationDbContext _context;

    public ReviewController(ApplicationDbContext context) {
        _context = context;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string reviewedUserId,
        int rating,
        int? smokingRating,
        int? petsRating,
        int? cleanlinessRating,
        int? sleepScheduleRating,
        int? guestPreferenceRating,
        string? comment) {
        var reviewerId = CurrentUserId();
        if (reviewerId == null) {
            return Challenge();
        }

        if (reviewerId == reviewedUserId) {
            return BadRequest();
        }

        var reviewedUserExists = await _context.Users.AnyAsync(user => user.Id == reviewedUserId);
        if (!reviewedUserExists) {
            return NotFound();
        }

        var hasHousingRelationship = await _context.ListingRequests.AnyAsync(request =>
            request.Status == RequestStatus.Accepted &&
            ((request.SenderId == reviewerId && request.Listing!.OwnerId == reviewedUserId) ||
             (request.SenderId == reviewedUserId && request.Listing!.OwnerId == reviewerId)));

        if (!hasHousingRelationship) {
            return Forbid();
        }

        var detailedRatings = new[] {
            smokingRating,
            petsRating,
            cleanlinessRating,
            sleepScheduleRating,
            guestPreferenceRating
        };

        if (rating is < 1 or > 5 ||
            detailedRatings.Any(value => !value.HasValue || value.Value is < 1 or > 5)) {
            TempData["ReviewMessage"] = "Choose a score from 1 to 5 for every review category.";
            return RedirectToAction("Details", "Profile", new { id = reviewedUserId });
        }

        var review = new Review {
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            Rating = rating,
            SmokingRating = smokingRating,
            PetsRating = petsRating,
            CleanlinessRating = cleanlinessRating,
            SleepScheduleRating = sleepScheduleRating,
            GuestPreferenceRating = guestPreferenceRating,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        if (!TryValidateModel(review)) {
            TempData["ReviewMessage"] =
                "All review ratings must be from 1 to 5 and the comment must be at most 1000 characters.";
            return RedirectToAction("Details", "Profile", new { id = reviewedUserId });
        }

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        TempData["ReviewMessage"] = "Your review was added.";
        return RedirectToAction("Details", "Profile", new { id = reviewedUserId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id) {
        var reviewerId = CurrentUserId();
        if (reviewerId == null) {
            return Challenge();
        }

        var review = await _context.Reviews.FindAsync(id);
        if (review == null || review.ReviewerId != reviewerId) {
            return NotFound();
        }

        var reviewedUserId = review.ReviewedUserId;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Profile", new { id = reviewedUserId });
    }

    private string? CurrentUserId() {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
