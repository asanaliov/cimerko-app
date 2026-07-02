using System.ComponentModel.DataAnnotations;
using cimerko_app.Models;

namespace cimerko_app.Models.ViewModels;

public class ListingIndexViewModel {
    public string? Title { get; set; }

    public string? City { get; set; }

    [Display(Name = "Minimum budget")]
    [Range(0, 100000)]
    public decimal? MinimumBudget { get; set; }

    [Display(Name = "Maximum budget")]
    [Range(0, 100000)]
    public decimal? MaximumBudget { get; set; }

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

    public IReadOnlyList<Listing> Listings { get; set; } = Array.Empty<Listing>();

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(Title) ||
        !string.IsNullOrWhiteSpace(City) ||
        MinimumBudget.HasValue ||
        MaximumBudget.HasValue ||
        !string.IsNullOrWhiteSpace(SmokingPreference) ||
        !string.IsNullOrWhiteSpace(PetsPreference) ||
        !string.IsNullOrWhiteSpace(CleanlinessLevel) ||
        !string.IsNullOrWhiteSpace(SleepSchedule) ||
        !string.IsNullOrWhiteSpace(GuestPreference);
}
