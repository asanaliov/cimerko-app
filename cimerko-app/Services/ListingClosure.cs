using cimerko_app.Data;
using cimerko_app.Models;
using cimerko_app.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Services;

public static class ListingClosure {
    // Takes the listing offline and lets everyone still waiting know it is no longer available.
    public static async Task CloseAsync(
        ApplicationDbContext context,
        NotificationService notificationService,
        Listing listing,
        string actorId,
        int? exceptRequestId = null) {
        listing.IsActive = false;
        listing.ModerationStatus = ListingModerationStatus.Inactive;

        var pendingRequests = await context.ListingRequests
            .Where(request =>
                request.ListingId == listing.Id &&
                request.Status == RequestStatus.Pending &&
                request.Id != exceptRequestId)
            .ToListAsync();

        foreach (var request in pendingRequests) {
            request.Status = RequestStatus.Rejected;
            notificationService.Add(
                request.SenderId,
                actorId,
                "Listing no longer available",
                $"{listing.Title} has been taken, so your request was closed.",
                $"/Listing/Details/{listing.Id}",
                listing.Id,
                request.Id);
        }
    }
}
