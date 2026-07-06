using System.ComponentModel.DataAnnotations;
using cimerko_app.Models.Enums;

namespace cimerko_app.Models.ViewModels;

public class RoommateSearchResultViewModel {
    public ApplicationUser User { get; set; } = null!;

    public Listing Listing { get; set; } = null!;

    public int? CompatibilityScore { get; set; }

    public IReadOnlyList<string> StrongMatches { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> PossibleConflicts { get; set; } = Array.Empty<string>();
}

public class RoommateSearchViewModel {
    public string? City { get; set; }

    [Display(Name = "Minimum budget")]
    [Range(0, 100000)]
    public decimal? MinimumBudget { get; set; }

    [Display(Name = "Maximum budget")]
    [Range(0, 100000)]
    public decimal? MaximumBudget { get; set; }

    [Display(Name = "Their gender")]
    public string? Gender { get; set; }

    [Display(Name = "Their smoking preference")]
    public string? SmokingPreference { get; set; }

    [Display(Name = "Housing plan")]
    public RoommateHousingPlan? HousingPlan { get; set; }

    [Display(Name = "Pet-friendly")]
    public bool PetFriendly { get; set; }

    [Display(Name = "Smoke-free")]
    public bool SmokeFree { get; set; }

    [Display(Name = "Early-bird routine")]
    public bool EarlyBird { get; set; }

    [Display(Name = "Night-owl routine")]
    public bool NightOwl { get; set; }

    [Display(Name = "Tidy shared spaces")]
    public bool Tidy { get; set; }

    [Display(Name = "Guests welcome")]
    public bool GuestsWelcome { get; set; }

    [Display(Name = "Available by")]
    [DataType(DataType.Date)]
    public DateTime? AvailableBy { get; set; }

    public IReadOnlyList<RoommateSearchResultViewModel> Results { get; set; } =
        Array.Empty<RoommateSearchResultViewModel>();

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(City) ||
        MinimumBudget.HasValue ||
        MaximumBudget.HasValue ||
        !string.IsNullOrWhiteSpace(Gender) ||
        !string.IsNullOrWhiteSpace(SmokingPreference) ||
        HousingPlan.HasValue ||
        PetFriendly ||
        SmokeFree ||
        EarlyBird ||
        NightOwl ||
        Tidy ||
        GuestsWelcome ||
        AvailableBy.HasValue;
}
