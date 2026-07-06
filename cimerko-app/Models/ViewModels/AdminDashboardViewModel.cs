using cimerko_app.Models.Enums;

namespace cimerko_app.Models.ViewModels;

public class AdminDashboardViewModel {
    public int UserCount { get; set; }

    public int ListingCount { get; set; }

    public int PendingListingCount { get; set; }

    public int OpenReportCount { get; set; }

    public int NewUsersThisWeek { get; set; }

    public int NewUsersThisMonth { get; set; }

    public IReadOnlyList<Listing> RecentPendingListings { get; set; } = [];

    public IReadOnlyList<Report> RecentOpenReports { get; set; } = [];
}

public class AdminUsersViewModel {
    public string? Search { get; set; }

    public IReadOnlyList<AdminUserRowViewModel> Users { get; set; } = [];
}

public class AdminUserRowViewModel {
    public required ApplicationUser User { get; set; }

    public string Role { get; set; } = "No role";

    public bool IsBlocked { get; set; }
}

public class AdminListingsViewModel {
    public string? City { get; set; }

    public decimal? MinimumPrice { get; set; }

    public decimal? MaximumPrice { get; set; }

    public ListingType? Type { get; set; }

    public ListingModerationStatus? Status { get; set; }

    public bool ReportedOnly { get; set; }

    public IReadOnlyList<AdminListingRowViewModel> Listings { get; set; } = [];
}

public class AdminListingRowViewModel {
    public required Listing Listing { get; set; }

    public int OpenReportCount { get; set; }
}

public class AdminReportsViewModel {
    public ReportStatus? Status { get; set; } = ReportStatus.Open;

    public IReadOnlyList<Report> Reports { get; set; } = [];
}
