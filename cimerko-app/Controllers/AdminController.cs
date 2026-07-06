using System.Security.Claims;
using cimerko_app.Data;
using cimerko_app.Models;
using cimerko_app.Models.Enums;
using cimerko_app.Models.ViewModels;
using cimerko_app.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminController : Controller {
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser>? _userManager;
    private readonly LocalImageStorage? _imageStorage;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser>? userManager = null,
        LocalImageStorage? imageStorage = null) {
        _context = context;
        _userManager = userManager;
        _imageStorage = imageStorage;
    }

    public async Task<IActionResult> Index() {
        var now = DateTime.UtcNow;
        var startOfWeek = now.Date.AddDays(-((7 + (int)now.DayOfWeek - (int)DayOfWeek.Monday) % 7));
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var model = new AdminDashboardViewModel {
            UserCount = await _context.Users.CountAsync(),
            ListingCount = await _context.Listings.CountAsync(),
            PendingListingCount = await _context.Listings
                .CountAsync(listing => listing.ModerationStatus == ListingModerationStatus.Pending),
            OpenReportCount = await _context.Reports
                .CountAsync(report => report.Status == ReportStatus.Open),
            NewUsersThisWeek = await _context.Users.CountAsync(user => user.CreatedAt >= startOfWeek),
            NewUsersThisMonth = await _context.Users.CountAsync(user => user.CreatedAt >= startOfMonth),
            RecentPendingListings = await _context.Listings
                .AsNoTracking()
                .Include(listing => listing.Owner)
                .Where(listing => listing.ModerationStatus == ListingModerationStatus.Pending)
                .OrderBy(listing => listing.CreatedAt)
                .Take(5)
                .ToListAsync(),
            RecentOpenReports = await _context.Reports
                .AsNoTracking()
                .Include(report => report.Reporter)
                .Include(report => report.Listing)
                .Include(report => report.ReportedUser)
                .Include(report => report.Review)
                .Where(report => report.Status == ReportStatus.Open)
                .OrderByDescending(report => report.CreatedAt)
                .Take(5)
                .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> Users(string? search) {
        var query = _context.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) {
            search = search.Trim();
            query = query.Where(user =>
                user.FullName.Contains(search) ||
                (user.Email != null && user.Email.Contains(search)));
        }

        var users = await query
            .OrderByDescending(user => user.CreatedAt)
            .ToListAsync();
        var rolesByUser = await (
                from userRole in _context.UserRoles
                join role in _context.Roles on userRole.RoleId equals role.Id
                select new { userRole.UserId, Role = role.Name! })
            .ToListAsync();
        var roleLookup = rolesByUser
            .GroupBy(item => item.UserId)
            .ToDictionary(group => group.Key, group => group.First().Role);
        var now = DateTimeOffset.UtcNow;

        return View(new AdminUsersViewModel {
            Search = search,
            Users = users.Select(user => new AdminUserRowViewModel {
                User = user,
                Role = roleLookup.GetValueOrDefault(user.Id, "No role"),
                IsBlocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > now
            }).ToList()
        });
    }

    public async Task<IActionResult> Listings(AdminListingsViewModel model) {
        var query = _context.Listings
            .AsNoTracking()
            .Include(listing => listing.Owner)
            .Include(listing => listing.Reports)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(model.City)) {
            model.City = model.City.Trim();
            query = query.Where(listing => listing.City.Contains(model.City));
        }

        if (model.MinimumPrice.HasValue) {
            query = query.Where(listing => listing.MonthlyRent >= model.MinimumPrice.Value);
        }

        if (model.MaximumPrice.HasValue) {
            query = query.Where(listing => listing.MonthlyRent <= model.MaximumPrice.Value);
        }

        if (model.Type.HasValue) {
            query = query.Where(listing => listing.Type == model.Type.Value);
        }

        if (model.Status.HasValue) {
            query = query.Where(listing => listing.ModerationStatus == model.Status.Value);
        }

        if (model.ReportedOnly) {
            query = query.Where(listing =>
                listing.Reports.Any(report => report.Status == ReportStatus.Open));
        }

        var listings = await query
            .OrderByDescending(listing => listing.CreatedAt)
            .ToListAsync();
        model.Listings = listings.Select(listing => new AdminListingRowViewModel {
            Listing = listing,
            OpenReportCount = listing.Reports.Count(report => report.Status == ReportStatus.Open)
        }).ToList();

        return View(model);
    }

    public async Task<IActionResult> Reports(ReportStatus? status = ReportStatus.Open) {
        var query = _context.Reports
            .AsNoTracking()
            .Include(report => report.Reporter)
            .Include(report => report.Listing)
            .Include(report => report.ReportedUser)
            .Include(report => report.Review)
            .AsQueryable();

        if (status.HasValue) {
            query = query.Where(report => report.Status == status.Value);
        }

        return View(new AdminReportsViewModel {
            Status = status,
            Reports = await query
                .OrderBy(report => report.Status)
                .ThenByDescending(report => report.CreatedAt)
                .ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveListing(int id) {
        return await SetListingStatus(
            id,
            ListingModerationStatus.Approved,
            isActive: true,
            "Listing approved.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectListing(int id) {
        return await SetListingStatus(
            id,
            ListingModerationStatus.Rejected,
            isActive: false,
            "Listing rejected.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateListing(int id) {
        return await SetListingStatus(
            id,
            ListingModerationStatus.Inactive,
            isActive: false,
            "Listing marked as inactive.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteListing(int id) {
        var listing = await _context.Listings
            .Include(item => item.Images)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (listing == null) {
            return NotFound();
        }

        var imageUrls = listing.Images.Select(image => image.ImageUrl).ToList();
        _context.Listings.Remove(listing);
        await _context.SaveChangesAsync();

        foreach (var imageUrl in imageUrls) {
            DeleteLocalListingImage(imageUrl);
        }

        TempData["AdminMessage"] = "Listing deleted.";
        return RedirectToAction(nameof(Listings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BlockUser(string id) {
        if (id == CurrentUserId()) {
            TempData["AdminMessage"] = "You cannot block your own admin account.";
            return RedirectToAction(nameof(Users));
        }

        var user = await RequireUserManager().FindByIdAsync(id);
        if (user == null) {
            return NotFound();
        }

        var lockoutEnabledResult = await RequireUserManager().SetLockoutEnabledAsync(user, true);
        if (!lockoutEnabledResult.Succeeded) {
            TempData["AdminMessage"] = IdentityErrors(lockoutEnabledResult);
            return RedirectToAction(nameof(Users));
        }

        var result = await RequireUserManager().SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        if (!result.Succeeded) {
            TempData["AdminMessage"] = IdentityErrors(result);
            return RedirectToAction(nameof(Users));
        }

        await RequireUserManager().UpdateSecurityStampAsync(user);
        TempData["AdminMessage"] = $"{user.Email ?? user.FullName} was blocked.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnblockUser(string id) {
        var user = await RequireUserManager().FindByIdAsync(id);
        if (user == null) {
            return NotFound();
        }

        var result = await RequireUserManager().SetLockoutEndDateAsync(user, null);
        TempData["AdminMessage"] = result.Succeeded
            ? $"{user.Email ?? user.FullName} was unblocked."
            : IdentityErrors(result);
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(string id, string role) {
        if (!AppRoles.All.Contains(role, StringComparer.Ordinal)) {
            return BadRequest();
        }

        if (id == CurrentUserId()) {
            TempData["AdminMessage"] = "You cannot change your own admin role.";
            return RedirectToAction(nameof(Users));
        }

        var userManager = RequireUserManager();
        var user = await userManager.FindByIdAsync(id);
        if (user == null) {
            return NotFound();
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded) {
            TempData["AdminMessage"] = IdentityErrors(removeResult);
            return RedirectToAction(nameof(Users));
        }

        var addResult = await userManager.AddToRoleAsync(user, role);
        if (!addResult.Succeeded) {
            if (currentRoles.Count > 0) {
                await userManager.AddToRolesAsync(user, currentRoles);
            }

            TempData["AdminMessage"] = IdentityErrors(addResult);
            return RedirectToAction(nameof(Users));
        }

        await userManager.UpdateSecurityStampAsync(user);
        TempData["AdminMessage"] = $"{user.Email ?? user.FullName} is now a {role}.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id) {
        if (id == CurrentUserId()) {
            TempData["AdminMessage"] = "You cannot delete your own admin account.";
            return RedirectToAction(nameof(Users));
        }

        var userManager = RequireUserManager();
        var user = await _context.Users
            .Include(item => item.Listings)
            .ThenInclude(listing => listing.Images)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (user == null) {
            return NotFound();
        }

        if (await userManager.IsInRoleAsync(user, AppRoles.Admin)) {
            var admins = await userManager.GetUsersInRoleAsync(AppRoles.Admin);
            if (admins.Count <= 1) {
                TempData["AdminMessage"] = "The last admin account cannot be deleted.";
                return RedirectToAction(nameof(Users));
            }
        }

        var profileImageUrl = user.ProfileImageUrl;
        var listingImageUrls = user.Listings
            .SelectMany(listing => listing.Images)
            .Select(image => image.ImageUrl)
            .ToList();

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var reports = await _context.Reports
            .Where(report => report.ReporterId == id || report.ReportedUserId == id)
            .ToListAsync();
        var reviews = await _context.Reviews
            .Where(review => review.ReviewerId == id || review.ReviewedUserId == id)
            .ToListAsync();
        var sentRequests = await _context.ListingRequests
            .Where(request => request.SenderId == id)
            .ToListAsync();

        _context.Reports.RemoveRange(reports);
        _context.Reviews.RemoveRange(reviews);
        _context.ListingRequests.RemoveRange(sentRequests);
        await _context.SaveChangesAsync();

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded) {
            await transaction.RollbackAsync();
            TempData["AdminMessage"] = IdentityErrors(result);
            return RedirectToAction(nameof(Users));
        }

        await transaction.CommitAsync();
        DeleteLocalProfileImage(profileImageUrl);
        foreach (var imageUrl in listingImageUrls) {
            DeleteLocalListingImage(imageUrl);
        }

        TempData["AdminMessage"] = $"{user.Email ?? user.FullName} was deleted.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResolveReport(int id) {
        return await SetReportStatus(id, ReportStatus.Resolved, "Report resolved.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DismissReport(int id) {
        return await SetReportStatus(id, ReportStatus.Dismissed, "Report dismissed.");
    }

    private async Task<IActionResult> SetListingStatus(
        int id,
        ListingModerationStatus status,
        bool isActive,
        string message) {
        var listing = await _context.Listings.FindAsync(id);
        if (listing == null) {
            return NotFound();
        }

        listing.ModerationStatus = status;
        listing.IsActive = isActive;
        await _context.SaveChangesAsync();
        TempData["AdminMessage"] = message;
        return RedirectToAction(nameof(Listings));
    }

    private async Task<IActionResult> SetReportStatus(
        int id,
        ReportStatus status,
        string message) {
        var report = await _context.Reports.FindAsync(id);
        if (report == null) {
            return NotFound();
        }

        report.Status = status;
        report.ResolvedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        TempData["AdminMessage"] = message;
        return RedirectToAction(nameof(Reports));
    }

    private UserManager<ApplicationUser> RequireUserManager() {
        return _userManager ??
               throw new InvalidOperationException("User management services are not available.");
    }

    private string? CurrentUserId() {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private static string IdentityErrors(IdentityResult result) {
        return string.Join(" ", result.Errors.Select(error => error.Description));
    }

    private void DeleteLocalListingImage(string? imageUrl) {
        _imageStorage?.DeleteLocalImage(
            imageUrl,
            ("/uploads/listings/", "uploads/listings"),
            ("/images/listings/", "images/listings"),
            ("/uploads/listing-images/", "uploads/listing-images"));
    }

    private void DeleteLocalProfileImage(string? imageUrl) {
        _imageStorage?.DeleteLocalImage(
            imageUrl,
            ("/uploads/profiles/", "uploads/profiles"),
            ("/images/profiles/", "images/profiles"),
            ("/uploads/profile-images/", "uploads/profile-images"));
    }
}
