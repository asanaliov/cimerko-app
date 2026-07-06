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
public class ListingController : Controller {
    private const int MaxListingImages = 5;
    private const long MaxListingImageSize = 5 * 1024 * 1024;
    private const string ListingImageUrlPrefix = "/uploads/listings/";
    private const string ListingImageDirectory = "uploads/listings";

    private readonly ApplicationDbContext _context;
    private readonly LocalImageStorage _imageStorage;

    public ListingController(
        ApplicationDbContext context,
        LocalImageStorage imageStorage) {
        _context = context;
        _imageStorage = imageStorage;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(ListingIndexViewModel model) {
        if (model.MinimumBudget.HasValue &&
            model.MaximumBudget.HasValue &&
            model.MinimumBudget.Value > model.MaximumBudget.Value) {
            ModelState.AddModelError(
                nameof(model.MaximumBudget),
                "Maximum budget must be greater than or equal to minimum budget.");
        }

        var query = _context.Listings
            .Include(listing => listing.Owner)
            .ThenInclude(owner => owner!.RoommateProfile)
            .Include(listing => listing.Images)
            .Where(listing =>
                listing.IsActive &&
                listing.ModerationStatus == ListingModerationStatus.Approved)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(model.Title)) {
            var title = model.Title.Trim();
            query = query.Where(listing => listing.Title.Contains(title));
        }

        if (!string.IsNullOrWhiteSpace(model.City)) {
            var city = model.City.Trim();
            query = query.Where(listing => listing.City.Contains(city));
        }

        if (model.Type.HasValue) {
            query = query.Where(listing => listing.Type == model.Type.Value);
        }

        if (model.MinimumBudget.HasValue) {
            query = query.Where(listing => listing.MonthlyRent >= model.MinimumBudget.Value);
        }

        if (model.MaximumBudget.HasValue) {
            query = query.Where(listing => listing.MonthlyRent <= model.MaximumBudget.Value);
        }

        if (model.BedroomCount.HasValue) {
            query = query.Where(listing => listing.BedroomCount == model.BedroomCount.Value);
        }

        if (model.TenantTypePreference.HasValue) {
            query = query.Where(listing =>
                listing.Type == ListingType.PlaceForRent &&
                listing.TenantTypePreference == model.TenantTypePreference.Value);
        }

        if (model.RentalSmokingPolicy.HasValue) {
            query = query.Where(listing =>
                listing.Type == ListingType.PlaceForRent &&
                listing.RentalSmokingPolicy == model.RentalSmokingPolicy.Value);
        }

        if (model.RentalPetPolicy.HasValue) {
            query = query.Where(listing =>
                listing.Type == ListingType.PlaceForRent &&
                listing.RentalPetPolicy == model.RentalPetPolicy.Value);
        }

        if (model.RoommateGenderPreference.HasValue) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.RoommateGenderPreference == model.RoommateGenderPreference.Value);
        }

        if (model.RoommateHousingPlan.HasValue) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.RoommateHousingPlan == model.RoommateHousingPlan.Value);
        }

        if (model.RoommatePetFriendly) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.RoommatePetFriendly);
        }

        if (model.RoommateSmokeFree) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.RoommateSmokeFree);
        }

        if (model.RoommateEarlyBird) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.RoommateEarlyBird);
        }

        if (model.RoommateNightOwl) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.RoommateNightOwl);
        }

        if (model.RoommateTidy) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.RoommateTidy);
        }

        if (model.RoommateGuestsWelcome) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.RoommateGuestsWelcome);
        }

        if (!string.IsNullOrWhiteSpace(model.SmokingPreference)) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.Owner!.RoommateProfile != null &&
                listing.Owner.RoommateProfile.SmokingPreference == model.SmokingPreference);
        }

        if (!string.IsNullOrWhiteSpace(model.PetsPreference)) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.Owner!.RoommateProfile != null &&
                listing.Owner.RoommateProfile.PetsPreference == model.PetsPreference);
        }

        if (!string.IsNullOrWhiteSpace(model.CleanlinessLevel)) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.Owner!.RoommateProfile != null &&
                listing.Owner.RoommateProfile.CleanlinessLevel == model.CleanlinessLevel);
        }

        if (!string.IsNullOrWhiteSpace(model.SleepSchedule)) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.Owner!.RoommateProfile != null &&
                listing.Owner.RoommateProfile.SleepSchedule == model.SleepSchedule);
        }

        if (!string.IsNullOrWhiteSpace(model.GuestPreference)) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.Owner!.RoommateProfile != null &&
                listing.Owner.RoommateProfile.GuestPreference == model.GuestPreference);
        }

        if (model.AvailableNow) {
            var today = DateTime.UtcNow.Date;
            query = query.Where(listing =>
                !listing.AvailableFrom.HasValue || listing.AvailableFrom.Value <= today);
        }

        if (model.HasImages) {
            query = query.Where(listing => listing.Images.Any());
        }

        model.Listings = await query
            .OrderByDescending(listing => listing.CreatedAt)
            .ToListAsync();

        var userId = CurrentUserId();
        ViewBag.CurrentUserId = userId;
        if (userId != null) {
            model.SavedListingIds = await _context.SavedListings
                .Where(savedListing => savedListing.UserId == userId)
                .Select(savedListing => savedListing.ListingId)
                .ToHashSetAsync();
        }

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
        var isAdmin = User.IsInRole(AppRoles.Admin);
        if ((!listing.IsActive ||
             listing.ModerationStatus != ListingModerationStatus.Approved) &&
            !isOwner &&
            !isAdmin) {
            return NotFound();
        }

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
                ActiveListingCount = user.Listings.Count(ownerListing =>
                    ownerListing.IsActive &&
                    ownerListing.ModerationStatus == ListingModerationStatus.Approved)
            })
            .FirstOrDefaultAsync();

        return View(new ListingDetailsViewModel {
            Listing = listing,
            IsOwner = isOwner || isAdmin,
            IsSaved = isSaved,
            ExistingRequest = existingRequest,
            OwnerReviewCount = ownerSummary?.ReviewCount ?? 0,
            OwnerAverageRating = ownerSummary?.AverageRating,
            OwnerActiveListingCount = ownerSummary?.ActiveListingCount ?? 0
        });
    }

    public IActionResult Create(ListingType? type = null) {
        var selectedType = type is ListingType.PlaceForRent or ListingType.LookingForRoommate
            ? type.Value
            : default;

        return View(new Listing {
            Type = selectedType,
            RoomCount = 1
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,Description,Type,City,Address,ContactPhone,MonthlyRent,RoomCount,BedroomCount,RoommatesNeeded,TenantTypePreference,RentalSmokingPolicy,RentalPetPolicy,RoommateGenderPreference,RoommateHousingPlan,RoommatePetFriendly,RoommateSmokeFree,RoommateEarlyBird,RoommateNightOwl,RoommateTidy,RoommateGuestsWelcome,AvailableFrom")]
        Listing listing,
        List<IFormFile>? listingImages,
        bool isStudio = false) {
        var userId = CurrentUserId();
        if (userId == null) {
            return Challenge();
        }

        listing.OwnerId = userId;
        listing.CreatedAt = DateTime.UtcNow;
        listing.IsActive = false;
        listing.ModerationStatus = ListingModerationStatus.Pending;
        listing.ContactPhone = listing.ContactPhone?.Trim() ?? string.Empty;
        if (listing.Type is ListingType.PlaceForRent or ListingType.LookingForRoommate && isStudio) {
            listing.BedroomCount = 0;
        }

        ModelState.Remove(nameof(Listing.OwnerId));
        ApplyListingTypeRules(listing);

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
                    _imageStorage.DeleteFile(path);
                }

                throw;
            }

            TempData["ListingMessage"] = "Your listing was submitted for admin approval.";
            return RedirectToAction(nameof(Details), new { id = listing.Id });
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
        if (listing == null || !CanManageListing(listing)) {
            return NotFound();
        }

        return View(listing);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Title,Description,Type,City,Address,ContactPhone,MonthlyRent,RoomCount,BedroomCount,RoommatesNeeded,TenantTypePreference,RentalSmokingPolicy,RentalPetPolicy,RoommateGenderPreference,RoommateHousingPlan,RoommatePetFriendly,RoommateSmokeFree,RoommateEarlyBird,RoommateNightOwl,RoommateTidy,RoommateGuestsWelcome,AvailableFrom,IsActive")]
        Listing formListing,
        List<IFormFile>? listingImages,
        List<int>? removeImageIds,
        bool isStudio = false) {
        if (id != formListing.Id) {
            return NotFound();
        }

        var listing = await _context.Listings
            .Include(item => item.Images)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (listing == null || !CanManageListing(listing)) {
            return NotFound();
        }

        ModelState.Remove(nameof(Listing.OwnerId));
        formListing.ContactPhone = formListing.ContactPhone?.Trim() ?? string.Empty;
        if (formListing.Type is ListingType.PlaceForRent or ListingType.LookingForRoommate && isStudio) {
            formListing.BedroomCount = 0;
        }

        ApplyListingTypeRules(formListing);
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
        listing.ContactPhone = formListing.ContactPhone;
        listing.MonthlyRent = formListing.MonthlyRent;
        listing.RoomCount = formListing.RoomCount;
        listing.BedroomCount = formListing.BedroomCount;
        listing.RoommatesNeeded = formListing.RoommatesNeeded;
        listing.TenantTypePreference = formListing.TenantTypePreference;
        listing.RentalSmokingPolicy = formListing.RentalSmokingPolicy;
        listing.RentalPetPolicy = formListing.RentalPetPolicy;
        listing.RoommateGenderPreference = formListing.RoommateGenderPreference;
        listing.RoommateHousingPlan = formListing.RoommateHousingPlan;
        listing.RoommatePetFriendly = formListing.RoommatePetFriendly;
        listing.RoommateSmokeFree = formListing.RoommateSmokeFree;
        listing.RoommateEarlyBird = formListing.RoommateEarlyBird;
        listing.RoommateNightOwl = formListing.RoommateNightOwl;
        listing.RoommateTidy = formListing.RoommateTidy;
        listing.RoommateGuestsWelcome = formListing.RoommateGuestsWelcome;
        listing.AvailableFrom = formListing.AvailableFrom;
        if (User.IsInRole(AppRoles.Admin)) {
            listing.IsActive =
                formListing.IsActive &&
                listing.ModerationStatus == ListingModerationStatus.Approved;
            if (!formListing.IsActive) {
                listing.ModerationStatus = ListingModerationStatus.Inactive;
            }
        }
        else {
            listing.IsActive = false;
            listing.ModerationStatus = formListing.IsActive
                ? ListingModerationStatus.Pending
                : ListingModerationStatus.Inactive;
        }

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
                _imageStorage.DeleteFile(path);
            }

            throw;
        }

        foreach (var image in imagesToRemove) {
            DeleteLocalListingImage(image.ImageUrl);
        }

        TempData["ListingMessage"] = User.IsInRole(AppRoles.Admin)
            ? "Listing updated."
            : formListing.IsActive
                ? "Your changes were saved and submitted for admin approval."
                : "Your listing was marked as inactive.";
        return RedirectToAction(nameof(Details), new { id = listing.Id });
    }

    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var listing = await _context.Listings
            .Include(item => item.Owner)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (listing == null || !CanManageListing(listing)) {
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
        if (listing == null || !CanManageListing(listing)) {
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

        return User.IsInRole(AppRoles.Admin)
            ? RedirectToAction("Listings", "Admin")
            : RedirectToAction(nameof(Index));
    }

    private string? CurrentUserId() {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private bool CanManageListing(Listing listing) {
        return listing.OwnerId == CurrentUserId() || User.IsInRole(AppRoles.Admin);
    }

    private void ApplyListingTypeRules(Listing listing) {
        if (listing.Type == ListingType.LookingForRoommate) {
            listing.TenantTypePreference = TenantTypePreference.NoPreference;
            listing.RentalSmokingPolicy = RentalSmokingPolicy.NotSpecified;
            listing.RentalPetPolicy = RentalPetPolicy.NotSpecified;
            ModelState.Remove(nameof(Listing.TenantTypePreference));
            ModelState.Remove(nameof(Listing.RentalSmokingPolicy));
            ModelState.Remove(nameof(Listing.RentalPetPolicy));

            if (!listing.RoommatesNeeded.HasValue) {
                ModelState.AddModelError(
                    nameof(Listing.RoommatesNeeded),
                    "Enter how many roommates you need.");
            }

            if (!listing.RoommateHousingPlan.HasValue) {
                ModelState.AddModelError(
                    nameof(Listing.RoommateHousingPlan),
                    "Choose whether you already have a place or want to search together.");
            }

            return;
        }

        if (listing.Type == ListingType.PlaceForRent) {
            listing.RoommatesNeeded = null;
            listing.RoommateGenderPreference = RoommateGenderPreference.NoPreference;
            listing.RoommateHousingPlan = null;
            listing.RoommatePetFriendly = false;
            listing.RoommateSmokeFree = false;
            listing.RoommateEarlyBird = false;
            listing.RoommateNightOwl = false;
            listing.RoommateTidy = false;
            listing.RoommateGuestsWelcome = false;
            ModelState.Remove(nameof(Listing.RoommatesNeeded));
            ModelState.Remove(nameof(Listing.RoommateGenderPreference));
            ModelState.Remove(nameof(Listing.RoommateHousingPlan));

            if (!listing.BedroomCount.HasValue) {
                ModelState.AddModelError(
                    nameof(Listing.BedroomCount),
                    "Enter the number of bedrooms. Use 0 for a studio.");
            }
        }
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

            var extension = await _imageStorage.DetectExtensionAsync(image);
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

    private Task<StoredImage> SaveListingImageAsync(
        IFormFile image,
        string extension) {
        return _imageStorage.SaveAsync(
            image,
            extension,
            ListingImageDirectory,
            ListingImageUrlPrefix,
            HttpContext.RequestAborted);
    }

    private void DeleteLocalListingImage(string? imageUrl) {
        _imageStorage.DeleteLocalImage(
            imageUrl,
            (ListingImageUrlPrefix, ListingImageDirectory),
            ("/images/listings/", "images/listings"),
            ("/uploads/listing-images/", "uploads/listing-images"));
    }
}
