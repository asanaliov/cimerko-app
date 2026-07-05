namespace cimerko_app.Models.ViewModels;

public class AdminDashboardViewModel {
    public int UserCount { get; set; }

    public int ListingCount { get; set; }

    public int ActiveListingCount { get; set; }

    public int PendingRequestCount { get; set; }

    public int ReviewCount { get; set; }

    public IReadOnlyList<ApplicationUser> RecentUsers { get; set; } = [];

    public IReadOnlyList<Listing> RecentListings { get; set; } = [];
}
