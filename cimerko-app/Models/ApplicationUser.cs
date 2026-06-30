using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace cimerko_app.Models;

public class ApplicationUser : IdentityUser {
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    public string? ProfileImageUrl { get; set; }

    public bool IsDemoUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public RoommateProfile? RoommateProfile { get; set; }

    public ICollection<Listing> Listings { get; set; } = new List<Listing>();

    public ICollection<SavedListing> SavedListings { get; set; } = new List<SavedListing>();

    public ICollection<ListingRequest> SentRequests { get; set; } = new List<ListingRequest>();

    public ICollection<Review> ReviewsWritten { get; set; } = new List<Review>();

    public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
}
