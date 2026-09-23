using cimerko_app.Models;
using cimerko_app.Models.Enums;
using cimerko_app.Models.ViewModels;

namespace cimerko_app.Services;

public static class ListingSearch {
    public static IQueryable<Listing> Apply(IQueryable<Listing> query, ListingSearchFilters filters) {
        if (filters.Type.HasValue) {
            query = query.Where(listing => listing.Type == filters.Type.Value);
        }

        if (filters.MinimumBudget.HasValue) {
            query = query.Where(listing => listing.MonthlyRent >= filters.MinimumBudget.Value);
        }

        if (filters.MaximumBudget.HasValue) {
            query = query.Where(listing => listing.MonthlyRent <= filters.MaximumBudget.Value);
        }

        if (filters.BedroomCount.HasValue) {
            query = query.Where(listing => listing.BedroomCount == filters.BedroomCount.Value);
        }

        if (filters.TenantTypePreference.HasValue) {
            query = query.Where(listing =>
                listing.Type == ListingType.PlaceForRent &&
                listing.TenantTypePreference == filters.TenantTypePreference.Value);
        }

        if (filters.RentalSmokingPolicy.HasValue) {
            query = query.Where(listing =>
                listing.Type == ListingType.PlaceForRent &&
                listing.RentalSmokingPolicy == filters.RentalSmokingPolicy.Value);
        }

        if (filters.RentalPetPolicy.HasValue) {
            query = query.Where(listing =>
                listing.Type == ListingType.PlaceForRent &&
                listing.RentalPetPolicy == filters.RentalPetPolicy.Value);
        }

        if (filters.RoommateGenderPreference.HasValue) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.RoommateGenderPreference == filters.RoommateGenderPreference.Value);
        }

        if (filters.RoommateHousingPlan.HasValue) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.RoommateHousingPlan == filters.RoommateHousingPlan.Value);
        }

        if (filters.RoommatePetFriendly) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.RoommatePetFriendly);
        }

        if (filters.RoommateSmokeFree) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.RoommateSmokeFree);
        }

        if (filters.RoommateEarlyBird) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.RoommateEarlyBird);
        }

        if (filters.RoommateNightOwl) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.RoommateNightOwl);
        }

        if (filters.RoommateTidy) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.RoommateTidy);
        }

        if (filters.RoommateGuestsWelcome) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.RoommateGuestsWelcome);
        }

        if (!string.IsNullOrWhiteSpace(filters.SmokingPreference)) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.Owner!.RoommateProfile != null &&
                listing.Owner.RoommateProfile.SmokingPreference == filters.SmokingPreference);
        }

        if (!string.IsNullOrWhiteSpace(filters.PetsPreference)) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.Owner!.RoommateProfile != null &&
                listing.Owner.RoommateProfile.PetsPreference == filters.PetsPreference);
        }

        if (!string.IsNullOrWhiteSpace(filters.CleanlinessLevel)) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.Owner!.RoommateProfile != null &&
                listing.Owner.RoommateProfile.CleanlinessLevel == filters.CleanlinessLevel);
        }

        if (!string.IsNullOrWhiteSpace(filters.SleepSchedule)) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.Owner!.RoommateProfile != null &&
                listing.Owner.RoommateProfile.SleepSchedule == filters.SleepSchedule);
        }

        if (!string.IsNullOrWhiteSpace(filters.GuestPreference)) {
            query = query.Where(listing =>
                listing.Type == ListingType.LookingForRoommate &&
                listing.Owner!.RoommateProfile != null &&
                listing.Owner.RoommateProfile.GuestPreference == filters.GuestPreference);
        }

        if (filters.AvailableNow) {
            var today = DateTime.UtcNow.Date;
            query = query.Where(listing =>
                !listing.AvailableFrom.HasValue || listing.AvailableFrom.Value <= today);
        }

        if (filters.HasImages) {
            query = query.Where(listing => listing.Images.Any());
        }

        return query;
    }

    // Text matching runs in memory so it can ignore case and diacritics (SQLite can't).
    public static bool MatchesText(Listing listing, ListingSearchFilters filters) {
        return TextSearch.MatchesAll(
                   TextSearch.Terms(filters.Title),
                   listing.Title,
                   listing.Description,
                   listing.City,
                   listing.Address) &&
               TextSearch.MatchesAll(TextSearch.Terms(filters.City), listing.City, listing.Address);
    }
}
