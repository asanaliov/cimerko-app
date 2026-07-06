namespace cimerko_app.Models.ViewModels;

public class ListingPhotoGalleryViewModel {
    public required string DialogId { get; init; }

    public required string Title { get; init; }

    public required IReadOnlyList<ListingImage> Images { get; init; }
}
