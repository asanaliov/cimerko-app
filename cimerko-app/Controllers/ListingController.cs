using System.Security.Claims;
using cimerko_app.Data;
using cimerko_app.Models;
using cimerko_app.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Controllers;

[Authorize]
public class ListingController : Controller {
    private const int MaxListingImages = 5;
    private const long MaxListingImageSize = 5 * 1024 * 1024;
    private const string ListingImageUrlPrefix = "/uploads/listing-images/";

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ListingController(ApplicationDbContext context, IWebHostEnvironment environment) {
        _context = context;
        _environment = environment;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(ListingIndexViewModel model) {
        var query = _context.Listings
            .Include(listing => listing.Owner)
            .ThenInclude(owner => owner!.RoommateProfile)
            .Include(listing => listing.Images)
            .Where(listing => listing.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(model.Title)) {
            var title = model.Title.Trim();
            query = query.Where(listing => listing.Title.Contains(title));
        }

        if (!string.IsNullOrWhiteSpace(model.City)) {
            var city = model.City.Trim();
            query = query.Where(listing => listing.City.Contains(city));
        }

        if (model.MinimumBudget.HasValue) {
            query = query.Where(listing => listing.MonthlyRent >= model.MinimumBudget.Value);
        }

        if (model.MaximumBudget.HasValue) {
            query = query.Where(listing => listing.MonthlyRent <= model.MaximumBudget.Value);
        }

        if (!string.IsNullOrWhiteSpace(model.SmokingPreference)) {
            query = query.Where(listing =>
                listing.Owner!.RoommateProfile != null &&
                listing.Owner.RoommateProfile.SmokingPreference == model.SmokingPreference);
        }

        if (!string.IsNullOrWhiteSpace(model.PetsPreference)) {
            query = query.Where(listing =>
                listing.Owner!.RoommateProfile != null &&
                listing.Owner.RoommateProfile.PetsPreference == model.PetsPreference);
        }

        if (!string.IsNullOrWhiteSpace(model.CleanlinessLevel)) {
            query = query.Where(listing =>
                listing.Owner!.RoommateProfile != null &&
                listing.Owner.RoommateProfile.CleanlinessLevel == model.CleanlinessLevel);
        }

        if (!string.IsNullOrWhiteSpace(model.SleepSchedule)) {
            query = query.Where(listing =>
                listing.Owner!.RoommateProfile != null &&
                listing.Owner.RoommateProfile.SleepSchedule == model.SleepSchedule);
        }

        if (!string.IsNullOrWhiteSpace(model.GuestPreference)) {
            query = query.Where(listing =>
                listing.Owner!.RoommateProfile != null &&
                listing.Owner.RoommateProfile.GuestPreference == model.GuestPreference);
        }

        model.Listings = await query
            .OrderByDescending(listing => listing.CreatedAt)
            .ToListAsync();

        return View(model);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var listing = await _context.Listings
            .Include(item => item.Owner)
            .ThenInclude(owner => owner!.RoommateProfile)
            .Include(item => item.Images)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        if (listing == null) {
            return NotFound();
        }

        var userId = CurrentUserId();
        var isOwner = userId == listing.OwnerId;
        var isSaved = userId != null && !isOwner &&
                      await _context.SavedListings.AnyAsync(savedListing =>
                          savedListing.UserId == userId && savedListing.ListingId == listing.Id);
        var existingRequest = userId == null || isOwner
            ? null
            : await _context.ListingRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(request =>
                request.SenderId == userId && request.ListingId == listing.Id);

        var ownerSummary = await _context.Users
            .Where(user => user.Id == listing.OwnerId)
            .Select(user => new {
                ReviewCount = user.ReviewsReceived.Count,
                AverageRating = user.ReviewsReceived
                    .Select(review => (double?)review.Rating)
                    .Average(),
                ActiveListingCount = user.Listings.Count(ownerListing => ownerListing.IsActive)
            })
            .FirstOrDefaultAsync();

        return View(new ListingDetailsViewModel {
            Listing = listing,
            IsOwner = isOwner,
            IsSaved = isSaved,
            ExistingRequest = existingRequest,
            OwnerReviewCount = ownerSummary?.ReviewCount ?? 0,
            OwnerAverageRating = ownerSummary?.AverageRating,
            OwnerActiveListingCount = ownerSummary?.ActiveListingCount ?? 0
        });
    }

    public IActionResult Create() {
        return View(new Listing {
            RoomCount = 1,
            RoommatesNeeded = 1
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,Description,Type,City,Address,MonthlyRent,RoomCount,RoommatesNeeded,AvailableFrom")]
        Listing listing,
        List<IFormFile>? listingImages) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) {
            return Challenge();
        }

        listing.OwnerId = userId;
        listing.CreatedAt = DateTime.UtcNow;
        listing.IsActive = true;
        ModelState.Remove(nameof(Listing.OwnerId));

        var hadUploadedImages = listingImages?.Any(image => image.Length > 0) == true;
        var validImages = await ValidateListingImagesAsync(listingImages, 0);

        if (ModelState.IsValid) {
            var savedImagePaths = new List<string>();

            try {
                foreach (var image in validImages) {
                    var savedImage = await SaveListingImageAsync(image.File, image.Extension);
                    savedImagePaths.Add(savedImage.FilePath);
                    listing.Images.Add(new ListingImage {
                        ImageUrl = savedImage.ImageUrl,
                        CreatedAt = DateTime.UtcNow,
                        IsPrimary = listing.Images.Count == 0
                    });
                }

                _context.Listings.Add(listing);
                await _context.SaveChangesAsync();
            }
            catch {
                foreach (var path in savedImagePaths) {
                    DeleteFileIfPresent(path);
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        if (hadUploadedImages) {
            ModelState.AddModelError(
                "listingImages",
                "Please select your images again after correcting the form errors.");
        }

        return View(listing);
    }

    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var listing = await _context.Listings
            .Include(item => item.Images)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (listing == null || listing.OwnerId != CurrentUserId()) {
            return NotFound();
        }

        return View(listing);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Title,Description,Type,City,Address,MonthlyRent,RoomCount,RoommatesNeeded,AvailableFrom,IsActive")]
        Listing formListing,
        List<IFormFile>? listingImages,
        List<int>? removeImageIds) {
        if (id != formListing.Id) {
            return NotFound();
        }

        var listing = await _context.Listings
            .Include(item => item.Images)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (listing == null || listing.OwnerId != CurrentUserId()) {
            return NotFound();
        }

        ModelState.Remove(nameof(Listing.OwnerId));
        var requestedRemovalIds = removeImageIds?.ToHashSet() ?? [];
        var imagesToRemove = listing.Images
            .Where(image => requestedRemovalIds.Contains(image.Id))
            .ToList();
        var remainingImageCount = listing.Images.Count - imagesToRemove.Count;
        var validImages = await ValidateListingImagesAsync(listingImages, remainingImageCount);

        if (!ModelState.IsValid) {
            formListing.Images = listing.Images;
            return View(formListing);
        }

        listing.Title = formListing.Title;
        listing.Description = formListing.Description;
        listing.Type = formListing.Type;
        listing.City = formListing.City;
        listing.Address = formListing.Address;
        listing.MonthlyRent = formListing.MonthlyRent;
        listing.RoomCount = formListing.RoomCount;
        listing.RoommatesNeeded = formListing.RoommatesNeeded;
        listing.AvailableFrom = formListing.AvailableFrom;
        listing.IsActive = formListing.IsActive;

        foreach (var image in imagesToRemove) {
            listing.Images.Remove(image);
            _context.ListingImages.Remove(image);
        }

        var savedImagePaths = new List<string>();

        try {
            foreach (var image in validImages) {
                var savedImage = await SaveListingImageAsync(image.File, image.Extension);
                savedImagePaths.Add(savedImage.FilePath);
                listing.Images.Add(new ListingImage {
                    ImageUrl = savedImage.ImageUrl,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (listing.Images.Count > 0 && listing.Images.All(image => !image.IsPrimary)) {
                listing.Images
                    .OrderBy(image => image.CreatedAt)
                    .First()
                    .IsPrimary = true;
            }

            await _context.SaveChangesAsync();
        }
        catch {
            foreach (var path in savedImagePaths) {
                DeleteFileIfPresent(path);
            }

            throw;
        }

        foreach (var image in imagesToRemove) {
            DeleteLocalListingImage(image.ImageUrl);
        }

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
        var listing = await _context.Listings
            .Include(item => item.Images)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (listing == null || listing.OwnerId != CurrentUserId()) {
            return NotFound();
        }

        var imageUrls = listing.Images
            .Select(image => image.ImageUrl)
            .ToList();

        _context.Listings.Remove(listing);
        await _context.SaveChangesAsync();

        foreach (var imageUrl in imageUrls) {
            DeleteLocalListingImage(imageUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    private string? CurrentUserId() {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private async Task<List<(IFormFile File, string Extension)>> ValidateListingImagesAsync(
        List<IFormFile>? images,
        int existingImageCount) {
        const string imageField = "listingImages";
        var uploadedImages = images?
            .Where(image => image.Length > 0)
            .ToList() ?? [];
        var validImages = new List<(IFormFile File, string Extension)>();

        if (existingImageCount + uploadedImages.Count > MaxListingImages) {
            ModelState.AddModelError(
                imageField,
                $"A listing can have at most {MaxListingImages} images.");
            return validImages;
        }

        foreach (var image in uploadedImages) {
            if (image.Length > MaxListingImageSize) {
                ModelState.AddModelError(
                    imageField,
                    "Each image must be 5 MB or smaller.");
                continue;
            }

            var extension = await DetectImageExtensionAsync(image);
            if (extension == null) {
                ModelState.AddModelError(
                    imageField,
                    "Upload only valid JPG, PNG, or WebP images.");
                continue;
            }

            validImages.Add((image, extension));
        }

        return validImages;
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

    private async Task<(string ImageUrl, string FilePath)> SaveListingImageAsync(
        IFormFile image,
        string extension) {
        var uploadsDirectory = Path.Combine(WebRootPath(), "uploads", "listing-images");
        Directory.CreateDirectory(uploadsDirectory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsDirectory, fileName);

        try {
            await using var output = System.IO.File.Create(filePath);
            await image.CopyToAsync(output, HttpContext.RequestAborted);
        }
        catch {
            DeleteFileIfPresent(filePath);
            throw;
        }

        return ($"{ListingImageUrlPrefix}{fileName}", filePath);
    }

    private static void DeleteFileIfPresent(string? path) {
        if (path != null && System.IO.File.Exists(path)) {
            System.IO.File.Delete(path);
        }
    }

    private void DeleteLocalListingImage(string? imageUrl) {
        if (string.IsNullOrWhiteSpace(imageUrl) ||
            !imageUrl.StartsWith(ListingImageUrlPrefix, StringComparison.Ordinal)) {
            return;
        }

        var fileName = Path.GetFileName(imageUrl);
        if (imageUrl != $"{ListingImageUrlPrefix}{fileName}") {
            return;
        }

        DeleteFileIfPresent(Path.Combine(WebRootPath(), "uploads", "listing-images", fileName));
    }

    private string WebRootPath() {
        return _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
    }
}
