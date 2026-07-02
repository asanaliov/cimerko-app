using System.ComponentModel.DataAnnotations;

namespace cimerko_app.Models;

public class ListingImage {
    public int Id { get; set; }

    public int ListingId { get; set; }

    public Listing? Listing { get; set; }

    [Required]
    [MaxLength(300)]
    public string ImageUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsPrimary { get; set; }
}
