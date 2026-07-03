using System.ComponentModel.DataAnnotations;

namespace cimerko_app.Models.Enums;

public enum ListingType {
    [Display(Name = "Place for rent")]
    PlaceForRent = 1,

    [Display(Name = "Looking for a roommate")]
    LookingForRoommate = 2
}

public static class ListingTypeExtensions {
    public static string GetDisplayName(this ListingType listingType) {
        return listingType switch {
            ListingType.PlaceForRent => "Place for rent",
            ListingType.LookingForRoommate => "Looking for a roommate",
            _ => "Housing listing"
        };
    }
}
