using System.ComponentModel.DataAnnotations;

namespace cimerko_app.Models.Enums;

public enum TenantTypePreference {
    [Display(Name = "No preference")]
    NoPreference = 0,

    [Display(Name = "Student preferred")]
    Student = 1,

    [Display(Name = "Working professional preferred")]
    WorkingProfessional = 2
}

public enum RentalSmokingPolicy {
    [Display(Name = "Not specified")]
    NotSpecified = 0,

    [Display(Name = "Smoke-free property")]
    SmokeFree = 1,

    [Display(Name = "Outside only")]
    OutsideOnly = 2,

    [Display(Name = "Smoking allowed")]
    Allowed = 3
}

public enum RentalPetPolicy {
    [Display(Name = "Not specified")]
    NotSpecified = 0,

    [Display(Name = "No pets")]
    NoPets = 1,

    [Display(Name = "Pets allowed")]
    Allowed = 2,

    [Display(Name = "Ask the landlord")]
    AskLandlord = 3
}

public enum RoommateGenderPreference {
    [Display(Name = "No preference")]
    NoPreference = 0,

    [Display(Name = "Female")]
    Women = 1,

    [Display(Name = "Male")]
    Men = 2,

    [Display(Name = "Non-binary people")]
    NonBinary = 3
}

public enum RoommateHousingPlan {
    [Display(Name = "I already have a place")]
    HavePlace = 1,

    [Display(Name = "I want to search together")]
    SearchTogether = 2
}

public static class ListingPreferenceExtensions {
    public static string GetDisplayName(this TenantTypePreference preference) {
        return preference switch {
            TenantTypePreference.Student => "Student preferred",
            TenantTypePreference.WorkingProfessional => "Working professional preferred",
            _ => "No preference"
        };
    }

    public static string GetDisplayName(this RentalSmokingPolicy policy) {
        return policy switch {
            RentalSmokingPolicy.SmokeFree => "Smoke-free property",
            RentalSmokingPolicy.OutsideOnly => "Outside only",
            RentalSmokingPolicy.Allowed => "Smoking allowed",
            _ => "Not specified"
        };
    }

    public static string GetDisplayName(this RentalPetPolicy policy) {
        return policy switch {
            RentalPetPolicy.NoPets => "No pets",
            RentalPetPolicy.Allowed => "Pets allowed",
            RentalPetPolicy.AskLandlord => "Ask the landlord",
            _ => "Not specified"
        };
    }

    public static string GetDisplayName(this RoommateGenderPreference preference) {
        return preference switch {
            RoommateGenderPreference.Women => "Female",
            RoommateGenderPreference.Men => "Male",
            RoommateGenderPreference.NonBinary => "Non-binary people",
            _ => "No preference"
        };
    }

    public static string GetDisplayName(this RoommateHousingPlan plan) {
        return plan switch {
            RoommateHousingPlan.HavePlace => "Already has a place",
            RoommateHousingPlan.SearchTogether => "Wants to search together",
            _ => "Housing plan not specified"
        };
    }
}
