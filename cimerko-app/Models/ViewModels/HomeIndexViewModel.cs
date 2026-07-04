using cimerko_app.Models;

namespace cimerko_app.Models.ViewModels;

public class HomeIndexViewModel {
    public IReadOnlyList<Listing> LatestListings { get; set; } = Array.Empty<Listing>();
}
