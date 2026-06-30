using System.Security.Claims;
using cimerko_app.Data;
using cimerko_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Controllers;

[Authorize]
public class ProfileController : Controller {
    private readonly ApplicationDbContext _context;

    public ProfileController(ApplicationDbContext context) {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(string? id) {
        if (id == null) {
            return NotFound();
        }

        var user = await _context.Users
            .Include(item => item.RoommateProfile)
            .Include(item => item.ReviewsReceived)
            .ThenInclude(review => review.Reviewer)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user == null) {
            return NotFound();
        }

        return View(user);
    }

    public IActionResult MyProfile() {
        var userId = CurrentUserId();
        if (userId == null) {
            return Challenge();
        }

        return RedirectToAction(nameof(Details), new { id = userId });
    }

    public async Task<IActionResult> Edit() {
        var userId = CurrentUserId();
        if (userId == null) {
            return Challenge();
        }

        var user = await _context.Users
            .Include(item => item.RoommateProfile)
            .FirstOrDefaultAsync(item => item.Id == userId);

        if (user == null) {
            return NotFound();
        }

        ViewBag.FullName = user.FullName;
        ViewBag.ProfileImageUrl = user.ProfileImageUrl;

        return View(user.RoommateProfile ?? new RoommateProfile {
            UserId = userId,
            Age = 18
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        string fullName,
        string? profileImageUrl,
        [Bind("Bio,Age,Gender,University,StudyProgram")] RoommateProfile formProfile) {
        var userId = CurrentUserId();
        if (userId == null) {
            return Challenge();
        }

        ModelState.Remove(nameof(RoommateProfile.UserId));

        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 100) {
            ModelState.AddModelError(nameof(fullName), "Full name is required and must be at most 100 characters.");
        }

        if (!ModelState.IsValid) {
            ViewBag.FullName = fullName;
            ViewBag.ProfileImageUrl = profileImageUrl;
            return View(formProfile);
        }

        var user = await _context.Users
            .Include(item => item.RoommateProfile)
            .FirstOrDefaultAsync(item => item.Id == userId);

        if (user == null) {
            return NotFound();
        }

        user.FullName = fullName.Trim();
        user.ProfileImageUrl = string.IsNullOrWhiteSpace(profileImageUrl)
            ? null
            : profileImageUrl.Trim();

        var profile = user.RoommateProfile;
        if (profile == null) {
            profile = new RoommateProfile {
                UserId = userId
            };
            _context.RoommateProfiles.Add(profile);
        }

        profile.Bio = formProfile.Bio;
        profile.Age = formProfile.Age;
        profile.Gender = formProfile.Gender;
        profile.University = formProfile.University;
        profile.StudyProgram = formProfile.StudyProgram;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = userId });
    }

    private string? CurrentUserId() {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
