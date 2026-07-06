using System.ComponentModel.DataAnnotations;
using cimerko_app.Models.Enums;

namespace cimerko_app.Models.ViewModels;

public class CreateReportViewModel {
    public ReportTargetType TargetType { get; set; }

    public int? ListingId { get; set; }

    public string? ReportedUserId { get; set; }

    public string TargetName { get; set; } = string.Empty;

    [Display(Name = "Reason")]
    public ReportReason Reason { get; set; }

    [MaxLength(1000)]
    [Display(Name = "Additional details")]
    public string? Details { get; set; }
}
