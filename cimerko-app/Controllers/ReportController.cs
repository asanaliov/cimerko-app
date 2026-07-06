using System.Security.Claims;
using cimerko_app.Data;
using cimerko_app.Models;
using cimerko_app.Models.Enums;
using cimerko_app.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Controllers;

[Authorize]
public class ReportController : Controller {
    private readonly ApplicationDbContext _context;

    public ReportController(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<IActionResult> Listing(int id) {
        var listing = await _context.Listings
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);
        if (listing == null) {
            return NotFound();
        }

        return View("Create", new CreateReportViewModel {
            TargetType = ReportTargetType.Listing,
            ListingId = listing.Id,
            TargetName = listing.Title
        });
    }

    public async Task<IActionResult> UserProfile(string id) {
        if (id == CurrentUserId()) {
            return BadRequest();
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);
        if (user == null) {
            return NotFound();
        }

        return View("Create", new CreateReportViewModel {
            TargetType = ReportTargetType.User,
            ReportedUserId = user.Id,
            TargetName = string.IsNullOrWhiteSpace(user.FullName)
                ? user.Email ?? "Cimerko member"
                : user.FullName
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReportViewModel model) {
        var reporterId = CurrentUserId();
        if (reporterId == null) {
            return Challenge();
        }

        Report report;
        string redirectAction;
        string redirectController;
        object redirectValues;

        if (model.TargetType == ReportTargetType.Listing && model.ListingId.HasValue) {
            var listing = await _context.Listings
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == model.ListingId.Value);
            if (listing == null) {
                return NotFound();
            }

            if (listing.OwnerId == reporterId) {
                return BadRequest();
            }

            model.TargetName = listing.Title;
            report = new Report {
                ReporterId = reporterId,
                TargetType = ReportTargetType.Listing,
                ListingId = listing.Id
            };
            redirectController = "Listing";
            redirectAction = "Details";
            redirectValues = new { id = listing.Id };
        }
        else if (model.TargetType == ReportTargetType.User &&
                 !string.IsNullOrWhiteSpace(model.ReportedUserId)) {
            if (model.ReportedUserId == reporterId) {
                return BadRequest();
            }

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == model.ReportedUserId);
            if (user == null) {
                return NotFound();
            }

            model.TargetName = string.IsNullOrWhiteSpace(user.FullName)
                ? user.Email ?? "Cimerko member"
                : user.FullName;
            report = new Report {
                ReporterId = reporterId,
                TargetType = ReportTargetType.User,
                ReportedUserId = user.Id
            };
            redirectController = "Profile";
            redirectAction = "Details";
            redirectValues = new { id = user.Id };
        }
        else {
            return BadRequest();
        }

        if (!Enum.IsDefined(model.Reason)) {
            ModelState.AddModelError(nameof(model.Reason), "Choose a valid report reason.");
        }

        if (!ModelState.IsValid) {
            return View(model);
        }

        var alreadyReported = await _context.Reports.AnyAsync(existing =>
            existing.ReporterId == reporterId &&
            existing.Status == ReportStatus.Open &&
            existing.TargetType == report.TargetType &&
            existing.ListingId == report.ListingId &&
            existing.ReportedUserId == report.ReportedUserId);
        if (alreadyReported) {
            TempData["ReportMessage"] = "You already have an open report for this item.";
            return RedirectToAction(redirectAction, redirectController, redirectValues);
        }

        report.Reason = model.Reason;
        report.Details = string.IsNullOrWhiteSpace(model.Details) ? null : model.Details.Trim();
        report.CreatedAt = DateTime.UtcNow;
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        TempData["ReportMessage"] = "Your report was sent to the admin team.";
        return RedirectToAction(redirectAction, redirectController, redirectValues);
    }

    private string? CurrentUserId() {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
