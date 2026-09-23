using System.ComponentModel.DataAnnotations;
using cimerko_app.Models;
using cimerko_app.Models.Enums;

namespace cimerko_app.Models.ViewModels;

public class ListingIndexViewModel : ListingSearchFilters {
    public string? Sort { get; set; }

    public int Page { get; set; } = 1;

    public IReadOnlyList<Listing> Listings { get; set; } = Array.Empty<Listing>();

    public int TotalCount { get; set; }

    public PaginationViewModel Pagination { get; set; } = new(1, 1);

    public HashSet<int> SavedListingIds { get; set; } = [];

    public int AdvancedFilterCount =>
        new[] {
            MinimumBudget.HasValue || MaximumBudget.HasValue,
            BedroomCount.HasValue && !(Type == ListingType.PlaceForRent && BedroomCount == 0),
            TenantTypePreference.HasValue,
            RentalSmokingPolicy.HasValue,
            RentalPetPolicy.HasValue,
            RoommateGenderPreference.HasValue,
            RoommateHousingPlan.HasValue,
            RoommatePetFriendly,
            RoommateSmokeFree,
            RoommateEarlyBird,
            RoommateNightOwl,
            RoommateTidy,
            RoommateGuestsWelcome,
            AvailableNow,
            HasImages
        }.Count(isActive => isActive);
}

public static class ListingSort {
    public const string Newest = "newest";
    public const string PriceLow = "price-low";
    public const string PriceHigh = "price-high";
    public const string AvailableSoonest = "available";

    public static readonly (string Value, string Label)[] Options = [
        (Newest, "Newest"),
        (PriceLow, "Lowest price"),
        (PriceHigh, "Highest price"),
        (AvailableSoonest, "Available soonest")
    ];
}
