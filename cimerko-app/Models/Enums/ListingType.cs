using System.ComponentModel.DataAnnotations;

namespace cimerko_app.Models.Enums;

public enum ListingType {
    [Display(Name = "Looking for a place")]
    LookingForPlace = 1,

    [Display(Name = "Looking for a roommate")]
    LookingForRoommate = 2
}

public static class ListingTypeExtensions {
    public static string GetDisplayName(this ListingType listingType) {
        return listingType switch {
            ListingType.LookingForPlace => "Looking for a place",
            ListingType.LookingForRoommate => "Looking for a roommate",
            _ => "Housing listing"
        };
    }
}
