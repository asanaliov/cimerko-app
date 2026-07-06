namespace cimerko_app.Models.Enums;

public enum ListingModerationStatus {
    Pending = 0,

    Approved = 1,

    Rejected = 2,

    Inactive = 3
}

public static class ListingModerationStatusExtensions {
    public static string GetDisplayName(this ListingModerationStatus status) {
        return status switch {
            ListingModerationStatus.Pending => "Pending",
            ListingModerationStatus.Approved => "Approved",
            ListingModerationStatus.Rejected => "Rejected",
            ListingModerationStatus.Inactive => "Inactive",
            _ => "Unknown"
        };
    }
}
