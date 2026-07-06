using System.ComponentModel.DataAnnotations;
using cimerko_app.Models.Enums;

namespace cimerko_app.Models;

public class Report {
    public int Id { get; set; }

    [Required]
    public string ReporterId { get; set; } = string.Empty;

    public ApplicationUser? Reporter { get; set; }

    public ReportTargetType TargetType { get; set; }

    public int? ListingId { get; set; }

    public Listing? Listing { get; set; }

    public string? ReportedUserId { get; set; }

    public ApplicationUser? ReportedUser { get; set; }

    public int? ReviewId { get; set; }

    public Review? Review { get; set; }

    public ReportReason Reason { get; set; }

    [MaxLength(1000)]
    public string? Details { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Open;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }
}
