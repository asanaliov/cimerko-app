using System.Text.Json;
using cimerko_app.Data;
using cimerko_app.Models;
using cimerko_app.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Services;

public static class SavedSearchAlerts {
    public static string Serialize(ListingSearchFilters filters) {
        return JsonSerializer.Serialize(filters);
    }

    public static ListingSearchFilters Deserialize(string filtersJson) {
        return JsonSerializer.Deserialize<ListingSearchFilters>(filtersJson) ?? new ListingSearchFilters();
    }

    // Sends at most one notification per user when a newly approved listing matches their saved searches.
    public static async Task NotifyAsync(
        ApplicationDbContext context,
        NotificationService notificationService,
        Listing listing,
        string? actorId) {
        var searchesByUser = (await context.SavedSearches
                .Where(search => search.UserId != listing.OwnerId)
                .ToListAsync())
            .GroupBy(search => search.UserId);

        foreach (var userSearches in searchesByUser) {
            foreach (var search in userSearches) {
                var filters = Deserialize(search.FiltersJson);
                var matches = ListingSearch.MatchesText(listing, filters) &&
                              await ListingSearch
                                  .Apply(context.Listings.Where(item => item.Id == listing.Id), filters)
                                  .AnyAsync();
                if (!matches) {
                    continue;
                }

                notificationService.Add(
                    userSearches.Key,
                    actorId,
                    "New listing for your saved search",
                    $"{listing.Title} in {listing.City} matches your search: {search.Name}.",
                    $"/Listing/Details/{listing.Id}",
                    listing.Id);
                break;
            }
        }
    }
}
