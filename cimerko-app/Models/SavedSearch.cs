using System.ComponentModel.DataAnnotations;

namespace cimerko_app.Models;

public class SavedSearch {
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // The ListingSearchFilters as JSON, so the search can be re-run and matched against new listings.
    [Required]
    [MaxLength(2000)]
    public string FiltersJson { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
