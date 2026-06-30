using System.Security.Claims;
using cimerko_app.Data;
using cimerko_app.Models;
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
    public async Task<IActionResult> Create(string reviewedUserId, int rating, string? comment) {
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

        var review = new Review {
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            Rating = rating,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        if (!TryValidateModel(review)) {
            TempData["ReviewMessage"] = "Rating must be from 1 to 5 and the comment must be at most 1000 characters.";
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
