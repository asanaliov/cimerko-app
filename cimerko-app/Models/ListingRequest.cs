using System.ComponentModel.DataAnnotations;
using cimerko_app.Models.Enums;

namespace cimerko_app.Models;

public class ListingRequest {
    public int Id { get; set; }

    public int ListingId { get; set; }

    public Listing? Listing { get; set; }

    [Required]
    public string SenderId { get; set; } = string.Empty;

    public ApplicationUser? Sender { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
