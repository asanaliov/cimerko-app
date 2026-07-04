using cimerko_app.Data;
using cimerko_app.Models;

namespace cimerko_app.Services;

public class NotificationService {
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context) {
        _context = context;
    }

    public void Add(
        string recipientId,
        string? actorId,
        string title,
        string message,
        string? linkUrl = null,
        int? listingId = null,
        int? listingRequestId = null,
        ListingRequest? listingRequest = null) {
        if (string.IsNullOrWhiteSpace(recipientId)) {
            throw new ArgumentException("A notification recipient is required.", nameof(recipientId));
        }

        if (string.IsNullOrWhiteSpace(title)) {
            throw new ArgumentException("A notification title is required.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(message)) {
            throw new ArgumentException("A notification message is required.", nameof(message));
        }

        if (!IsSafeLocalUrl(linkUrl)) {
            throw new ArgumentException("Notification links must be local application paths.", nameof(linkUrl));
        }

        _context.Notifications.Add(new Notification {
            RecipientId = recipientId,
            ActorId = actorId,
            Title = title,
            Message = message,
            LinkUrl = linkUrl,
            ListingId = listingId,
            ListingRequestId = listingRequestId,
            ListingRequest = listingRequest,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static bool IsSafeLocalUrl(string? linkUrl) {
        return linkUrl == null ||
               (linkUrl.StartsWith("/", StringComparison.Ordinal) &&
                !linkUrl.StartsWith("//", StringComparison.Ordinal) &&
                !linkUrl.StartsWith("/\\", StringComparison.Ordinal));
    }
}
