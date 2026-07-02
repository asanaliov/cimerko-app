using System.ComponentModel.DataAnnotations;
using cimerko_app.Models.Enums;

namespace cimerko_app.Models;

public class Listing {
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public ListingType Type { get; set; }

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Address { get; set; }

    [Range(0, 100000)]
    public decimal MonthlyRent { get; set; }

    [Range(1, 20)]
    public int RoomCount { get; set; }

    [Required]
    [Range(1, 10)]
    [Display(Name = "Roommates needed")]
    public int RoommatesNeeded { get; set; } = 1;

    public DateTime? AvailableFrom { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public ApplicationUser? Owner { get; set; }

    public ICollection<SavedListing> SavedByUsers { get; set; } = new List<SavedListing>();

    public ICollection<ListingRequest> Requests { get; set; } = new List<ListingRequest>();

    public ICollection<ListingImage> Images { get; set; } = new List<ListingImage>();
}
