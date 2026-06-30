using System.ComponentModel.DataAnnotations;

namespace cimerko_app.Models;

public class SavedListing {
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public int ListingId { get; set; }

    public Listing? Listing { get; set; }

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
