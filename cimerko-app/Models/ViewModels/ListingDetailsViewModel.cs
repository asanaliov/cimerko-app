using cimerko_app.Models;

namespace cimerko_app.Models.ViewModels;

public class ListingDetailsViewModel {
    public Listing Listing { get; set; } = new();

    public bool IsOwner { get; set; }

    public bool IsSaved { get; set; }

    public ListingRequest? ExistingRequest { get; set; }

    public int OwnerReviewCount { get; set; }

    public double? OwnerAverageRating { get; set; }

    public int OwnerActiveListingCount { get; set; }
}
