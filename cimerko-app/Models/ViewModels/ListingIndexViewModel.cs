using System.ComponentModel.DataAnnotations;
using cimerko_app.Models;
using cimerko_app.Models.Enums;

namespace cimerko_app.Models.ViewModels;

public class ListingIndexViewModel {
    public string? Title { get; set; }

    public string? City { get; set; }

    [Display(Name = "Listing type")]
    public ListingType? Type { get; set; }

    [Display(Name = "Minimum budget")]
    [Range(0, 100000)]
    public decimal? MinimumBudget { get; set; }

    [Display(Name = "Maximum budget")]
    [Range(0, 100000)]
    public decimal? MaximumBudget { get; set; }

    [Display(Name = "Bedrooms")]
    [Range(0, 20)]
    public int? BedroomCount { get; set; }

    [Display(Name = "Preferred tenant")]
    public TenantTypePreference? TenantTypePreference { get; set; }

    [Display(Name = "Rental smoking policy")]
    public RentalSmokingPolicy? RentalSmokingPolicy { get; set; }

    [Display(Name = "Rental pet policy")]
    public RentalPetPolicy? RentalPetPolicy { get; set; }

    [Display(Name = "Preferred roommate gender")]
    public RoommateGenderPreference? RoommateGenderPreference { get; set; }

    [Display(Name = "Housing plan")]
    public RoommateHousingPlan? RoommateHousingPlan { get; set; }

    [Display(Name = "Pet-friendly")]
    public bool RoommatePetFriendly { get; set; }

    [Display(Name = "Smoke-free")]
    public bool RoommateSmokeFree { get; set; }

    [Display(Name = "Early-bird routine")]
    public bool RoommateEarlyBird { get; set; }

    [Display(Name = "Night-owl routine")]
    public bool RoommateNightOwl { get; set; }

    [Display(Name = "Tidy shared spaces")]
    public bool RoommateTidy { get; set; }

    [Display(Name = "Guests welcome")]
    public bool RoommateGuestsWelcome { get; set; }

    [Display(Name = "Smoking preference")]
    public string? SmokingPreference { get; set; }

    [Display(Name = "Pets preference")]
    public string? PetsPreference { get; set; }

    [Display(Name = "Cleanliness level")]
    public string? CleanlinessLevel { get; set; }

    [Display(Name = "Sleep schedule")]
    public string? SleepSchedule { get; set; }

    [Display(Name = "Guest preference")]
    public string? GuestPreference { get; set; }

    [Display(Name = "Available now")]
    public bool AvailableNow { get; set; }

    [Display(Name = "Listings with photos")]
    public bool HasImages { get; set; }

    public IReadOnlyList<Listing> Listings { get; set; } = Array.Empty<Listing>();

    public HashSet<int> SavedListingIds { get; set; } = [];

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(Title) ||
        !string.IsNullOrWhiteSpace(City) ||
        HasAdvancedFilters;

    public bool HasAdvancedFilters =>
        Type.HasValue ||
        MinimumBudget.HasValue ||
        MaximumBudget.HasValue ||
        BedroomCount.HasValue ||
        TenantTypePreference.HasValue ||
        RentalSmokingPolicy.HasValue ||
        RentalPetPolicy.HasValue ||
        RoommateGenderPreference.HasValue ||
        RoommateHousingPlan.HasValue ||
        RoommatePetFriendly ||
        RoommateSmokeFree ||
        RoommateEarlyBird ||
        RoommateNightOwl ||
        RoommateTidy ||
        RoommateGuestsWelcome ||
        !string.IsNullOrWhiteSpace(SmokingPreference) ||
        !string.IsNullOrWhiteSpace(PetsPreference) ||
        !string.IsNullOrWhiteSpace(CleanlinessLevel) ||
        !string.IsNullOrWhiteSpace(SleepSchedule) ||
        !string.IsNullOrWhiteSpace(GuestPreference) ||
        AvailableNow ||
        HasImages;

    public bool ShouldExpandFilters =>
        MinimumBudget.HasValue ||
        MaximumBudget.HasValue ||
        (BedroomCount.HasValue &&
         !(Type == ListingType.PlaceForRent && BedroomCount == 0)) ||
        TenantTypePreference.HasValue ||
        RentalSmokingPolicy.HasValue ||
        RentalPetPolicy.HasValue ||
        RoommateGenderPreference.HasValue ||
        RoommateHousingPlan.HasValue ||
        RoommatePetFriendly ||
        RoommateSmokeFree ||
        RoommateEarlyBird ||
        RoommateNightOwl ||
        RoommateTidy ||
        RoommateGuestsWelcome ||
        !string.IsNullOrWhiteSpace(SmokingPreference) ||
        !string.IsNullOrWhiteSpace(PetsPreference) ||
        !string.IsNullOrWhiteSpace(CleanlinessLevel) ||
        !string.IsNullOrWhiteSpace(SleepSchedule) ||
        !string.IsNullOrWhiteSpace(GuestPreference) ||
        AvailableNow ||
        HasImages;
}
