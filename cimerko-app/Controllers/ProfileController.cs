using System.Security.Claims;
using cimerko_app.Data;
using cimerko_app.Models;
using cimerko_app.Models.Enums;
using cimerko_app.Models.ViewModels;
using cimerko_app.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Controllers;

[Authorize]
public class ProfileController : Controller {
    private const long MaxProfileImageSize = 5 * 1024 * 1024;
    private const string ProfileImageUrlPrefix = "/uploads/profiles/";
    private const string ProfileImageDirectory = "uploads/profiles";

    private readonly ApplicationDbContext _context;
    private readonly LocalImageStorage _imageStorage;

    public ProfileController(
        ApplicationDbContext context,
        LocalImageStorage imageStorage) {
        _context = context;
        _imageStorage = imageStorage;
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

        var visitorId = CurrentUserId();
        ViewBag.CurrentUserId = visitorId;
        ViewBag.IsOwnProfile = visitorId == id;
        var canWriteReview = false;
        if (visitorId != null && visitorId != id) {
            var alreadyReviewed = await _context.Reviews.AnyAsync(review =>
                review.ReviewerId == visitorId &&
                review.ReviewedUserId == id);

            canWriteReview = !alreadyReviewed &&
                             await _context.ListingRequests.AnyAsync(request =>
                                 request.Status == RequestStatus.Accepted &&
                                 ((request.SenderId == visitorId && request.Listing!.OwnerId == id) ||
                                  (request.SenderId == id && request.Listing!.OwnerId == visitorId)));
        }

        ViewBag.CanWriteReview = canWriteReview;

        int? compatibilityScore = null;
        if (visitorId != null && visitorId != id) {
            var visitorProfile = await _context.RoommateProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(profile => profile.UserId == visitorId);

            var compatibility = ProfileCompatibilityCalculator.Calculate(visitorProfile, user.RoommateProfile);
            compatibilityScore = compatibility.Score;
            ViewBag.ShowCompatibility = true;
            ViewBag.CompatibilityScore = compatibility.Score;
            ViewBag.ComparedPreferences = compatibility.ComparedPreferences;
            ViewBag.CompatibilityStrongMatches = compatibility.StrongMatches;
            ViewBag.CompatibilityPossibleConflicts = compatibility.PossibleConflicts;
        }

        ViewBag.ProfileBadges = BuildProfileBadges(user, compatibilityScore);
        if (visitorId == id) {
            ViewBag.ProfileCompletion = CalculateProfileCompletion(user);
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
            UserId = userId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        string fullName,
        IFormFile? profileImage,
        bool removeProfileImage,
        [Bind("Bio,DateOfBirth,City,Gender,University,StudyProgram,SmokingPreference,PetsPreference,CleanlinessLevel,SleepSchedule,GuestPreference")]
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
                imageExtension = await _imageStorage.DetectExtensionAsync(profileImage);
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
        profile.DateOfBirth = formProfile.DateOfBirth;
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
            var savedImage = await _imageStorage.SaveAsync(
                profileImage,
                imageExtension,
                ProfileImageDirectory,
                ProfileImageUrlPrefix,
                HttpContext.RequestAborted);
            newImagePath = savedImage.FilePath;
            user.ProfileImageUrl = savedImage.ImageUrl;
        }
        else if (removeProfileImage) {
            user.ProfileImageUrl = null;
        }

        try {
            await _context.SaveChangesAsync();
        }
        catch {
            _imageStorage.DeleteFile(newImagePath);
            throw;
        }

        if (previousImageUrl != user.ProfileImageUrl) {
            DeleteLocalProfileImage(previousImageUrl);
        }

        return RedirectToAction(nameof(Details), new { id = userId });
    }

    private void DeleteLocalProfileImage(string? imageUrl) {
        _imageStorage.DeleteLocalImage(
            imageUrl,
            (ProfileImageUrlPrefix, ProfileImageDirectory),
            ("/images/profiles/", "images/profiles"),
            ("/uploads/profile-images/", "uploads/profile-images"));
    }

    private static ProfileCompletionViewModel CalculateProfileCompletion(ApplicationUser user) {
        var profile = user.RoommateProfile;
        var profileFields = new[] {
            new {
                Name = "Profile image",
                EditAnchor = "profileImageFallback",
                IsComplete = !string.IsNullOrWhiteSpace(user.ProfileImageUrl)
            },
            new {
                Name = "Full name",
                EditAnchor = "fullName",
                IsComplete = !string.IsNullOrWhiteSpace(user.FullName)
            },
            new { Name = "Bio", EditAnchor = "Bio", IsComplete = !string.IsNullOrWhiteSpace(profile?.Bio) },
            new {
                Name = "Date of birth",
                EditAnchor = "DateOfBirth",
                IsComplete = profile?.DateOfBirth != null
            },
            new { Name = "City", EditAnchor = "City", IsComplete = !string.IsNullOrWhiteSpace(profile?.City) },
            new {
                Name = "University",
                EditAnchor = "University",
                IsComplete = !string.IsNullOrWhiteSpace(profile?.University)
            },
            new {
                Name = "Faculty",
                EditAnchor = "StudyProgram",
                IsComplete = !string.IsNullOrWhiteSpace(profile?.StudyProgram)
            },
            new {
                Name = "Smoking preference",
                EditAnchor = "SmokingPreference",
                IsComplete = !string.IsNullOrWhiteSpace(profile?.SmokingPreference)
            },
            new {
                Name = "Pets preference",
                EditAnchor = "PetsPreference",
                IsComplete = !string.IsNullOrWhiteSpace(profile?.PetsPreference)
            },
            new {
                Name = "Cleanliness level",
                EditAnchor = "CleanlinessLevel",
                IsComplete = !string.IsNullOrWhiteSpace(profile?.CleanlinessLevel)
            },
            new {
                Name = "Sleep schedule",
                EditAnchor = "SleepSchedule",
                IsComplete = !string.IsNullOrWhiteSpace(profile?.SleepSchedule)
            },
            new {
                Name = "Guest preference",
                EditAnchor = "GuestPreference",
                IsComplete = !string.IsNullOrWhiteSpace(profile?.GuestPreference)
            }
        };

        var missingFields = profileFields
            .Where(field => !field.IsComplete)
            .Select(field => new MissingProfileFieldViewModel(field.Name, field.EditAnchor))
            .ToList();
        var completedFields = profileFields.Length - missingFields.Count;
        var percentage = (int)Math.Round(
            completedFields / (double)profileFields.Length * 100,
            MidpointRounding.AwayFromZero);

        return new ProfileCompletionViewModel(percentage, missingFields);
    }

    private static IReadOnlyList<ProfileBadgeViewModel> BuildProfileBadges(
        ApplicationUser user,
        int? compatibilityScore) {
        var badges = new List<ProfileBadgeViewModel>();
        var profile = user.RoommateProfile;
        var activeListingCount = user.Listings.Count(listing => listing.IsActive);
        var reviewCount = user.ReviewsReceived.Count;
        var averageRating = reviewCount == 0
            ? 0
            : user.ReviewsReceived.Average(review => review.Rating);
        var now = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(profile?.University) ||
            !string.IsNullOrWhiteSpace(profile?.StudyProgram)) {
            badges.Add(new ProfileBadgeViewModel("Student", "profile-badge-student"));
        }

        if (CalculateProfileCompletion(user).IsProfileComplete) {
            badges.Add(new ProfileBadgeViewModel("Complete Profile", "profile-badge-complete"));
        }

        if (string.Equals(
                profile?.SmokingPreference,
                "No smoking",
                StringComparison.OrdinalIgnoreCase)) {
            badges.Add(new ProfileBadgeViewModel("Non-smoker", "profile-badge-lifestyle"));
        }

        if (string.Equals(
                profile?.PetsPreference,
                "Pets are welcome",
                StringComparison.OrdinalIgnoreCase)) {
            badges.Add(new ProfileBadgeViewModel("Pet Friendly", "profile-badge-lifestyle"));
        }

        if (string.Equals(
                profile?.GuestPreference,
                "Rarely",
                StringComparison.OrdinalIgnoreCase)) {
            badges.Add(new ProfileBadgeViewModel("Quiet Lifestyle", "profile-badge-lifestyle"));
        }

        if (activeListingCount > 0) {
            badges.Add(new ProfileBadgeViewModel("Listing Owner", "profile-badge-owner"));
            badges.Add(new ProfileBadgeViewModel(
                $"{activeListingCount} active {(activeListingCount == 1 ? "listing" : "listings")}",
                "profile-badge-listings"));
        }

        if (reviewCount >= 3 && averageRating >= 4.5) {
            badges.Add(new ProfileBadgeViewModel("Highly Rated", "profile-badge-rated"));
        }

        if (user.CreatedAt != default &&
            user.CreatedAt >= now.AddDays(-30) &&
            user.CreatedAt <= now) {
            badges.Add(new ProfileBadgeViewModel("New User", "profile-badge-new"));
        }

        if (compatibilityScore >= 85) {
            badges.Add(new ProfileBadgeViewModel("Excellent Match", "profile-badge-match"));
        }
        else if (compatibilityScore >= 70) {
            badges.Add(new ProfileBadgeViewModel("Good Match", "profile-badge-match"));
        }

        return badges;
    }

    private string? CurrentUserId() {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
