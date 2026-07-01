using System.Security.Claims;
using cimerko_app.Data;
using cimerko_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Controllers;

[Authorize]
public class ProfileController : Controller {
    private const long MaxProfileImageSize = 5 * 1024 * 1024;
    private const string ProfileImageUrlPrefix = "/uploads/profile-images/";

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(ApplicationDbContext context, IWebHostEnvironment environment) {
        _context = context;
        _environment = environment;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(string? id) {
        if (id == null) {
            return NotFound();
        }

        var user = await _context.Users
            .Include(item => item.RoommateProfile)
            .Include(item => item.Listings.Where(listing => listing.IsActive))
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
        IFormFile? profileImage,
        bool removeProfileImage,
        [Bind("Bio,Age,City,Gender,University,StudyProgram,SmokingPreference,PetsPreference,CleanlinessLevel,SleepSchedule,GuestPreference")]
        RoommateProfile formProfile) {
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

        ModelState.Remove(nameof(RoommateProfile.UserId));

        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 100) {
            ModelState.AddModelError(nameof(fullName), "Full name is required and must be at most 100 characters.");
        }

        string? imageExtension = null;
        if (profileImage is { Length: > 0 }) {
            if (profileImage.Length > MaxProfileImageSize) {
                ModelState.AddModelError(nameof(profileImage), "Choose an image smaller than 5 MB.");
            }
            else {
                imageExtension = await DetectImageExtensionAsync(profileImage);
                if (imageExtension == null) {
                    ModelState.AddModelError(
                        nameof(profileImage),
                        "Choose a valid JPG, PNG, or WebP image.");
                }
            }
        }

        if (!ModelState.IsValid) {
            ViewBag.FullName = fullName;
            ViewBag.ProfileImageUrl = user.ProfileImageUrl;
            return View(formProfile);
        }

        user.FullName = fullName.Trim();

        var profile = user.RoommateProfile;
        if (profile == null) {
            profile = new RoommateProfile {
                UserId = userId
            };
            _context.RoommateProfiles.Add(profile);
        }

        profile.Bio = formProfile.Bio;
        profile.Age = formProfile.Age;
        profile.City = formProfile.City;
        profile.Gender = formProfile.Gender;
        profile.University = formProfile.University;
        profile.StudyProgram = formProfile.StudyProgram;
        profile.SmokingPreference = formProfile.SmokingPreference;
        profile.PetsPreference = formProfile.PetsPreference;
        profile.CleanlinessLevel = formProfile.CleanlinessLevel;
        profile.SleepSchedule = formProfile.SleepSchedule;
        profile.GuestPreference = formProfile.GuestPreference;

        var previousImageUrl = user.ProfileImageUrl;
        string? newImagePath = null;

        if (profileImage is { Length: > 0 } && imageExtension != null) {
            var uploadsDirectory = Path.Combine(WebRootPath(), "uploads", "profile-images");
            Directory.CreateDirectory(uploadsDirectory);

            var fileName = $"{Guid.NewGuid():N}{imageExtension}";
            newImagePath = Path.Combine(uploadsDirectory, fileName);

            await using var output = System.IO.File.Create(newImagePath);
            await profileImage.CopyToAsync(output, HttpContext.RequestAborted);
            user.ProfileImageUrl = $"{ProfileImageUrlPrefix}{fileName}";
        }
        else if (removeProfileImage) {
            user.ProfileImageUrl = null;
        }

        try {
            await _context.SaveChangesAsync();
        }
        catch {
            DeleteFileIfPresent(newImagePath);
            throw;
        }

        if (previousImageUrl != user.ProfileImageUrl) {
            DeleteLocalProfileImage(previousImageUrl);
        }

        return RedirectToAction(nameof(Details), new { id = userId });
    }

    private static async Task<string?> DetectImageExtensionAsync(IFormFile image) {
        var header = new byte[12];
        await using var stream = image.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header);

        if (bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) {
            return ".jpg";
        }

        if (bytesRead >= 8 &&
            header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
            header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A) {
            return ".png";
        }

        if (bytesRead >= 12 &&
            header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
            header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50) {
            return ".webp";
        }

        return null;
    }

    private void DeleteLocalProfileImage(string? imageUrl) {
        if (string.IsNullOrWhiteSpace(imageUrl) ||
            !imageUrl.StartsWith(ProfileImageUrlPrefix, StringComparison.Ordinal)) {
            return;
        }

        var fileName = Path.GetFileName(imageUrl);
        if (imageUrl != $"{ProfileImageUrlPrefix}{fileName}") {
            return;
        }

        DeleteFileIfPresent(Path.Combine(WebRootPath(), "uploads", "profile-images", fileName));
    }

    private static void DeleteFileIfPresent(string? path) {
        if (path != null && System.IO.File.Exists(path)) {
            System.IO.File.Delete(path);
        }
    }

    private string WebRootPath() {
        return _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
    }

    private string? CurrentUserId() {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
