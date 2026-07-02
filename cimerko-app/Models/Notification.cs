using System.ComponentModel.DataAnnotations;

namespace cimerko_app.Models;

public class Notification {
    public int Id { get; set; }

    [Required]
    public string RecipientId { get; set; } = string.Empty;

    public ApplicationUser? Recipient { get; set; }

    [MaxLength(100)]
    public string? ActorId { get; set; }

    public ApplicationUser? Actor { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public int? ListingId { get; set; }

    public Listing? Listing { get; set; }

    public int? ListingRequestId { get; set; }

    public ListingRequest? ListingRequest { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
