using System.ComponentModel.DataAnnotations;

namespace cimerko_app.Models.Enums;

public enum ReportReason {
    [Display(Name = "Fake listing")]
    FakeListing = 1,

    [Display(Name = "Bad or misleading photos")]
    BadPhotos = 2,

    [Display(Name = "Wrong price")]
    WrongPrice = 3,

    [Display(Name = "Scam user")]
    ScamUser = 4,

    [Display(Name = "Inappropriate content")]
    InappropriateContent = 5
}

public static class ReportReasonExtensions {
    public static string GetDisplayName(this ReportReason reason) {
        return reason switch {
            ReportReason.FakeListing => "Fake listing",
            ReportReason.BadPhotos => "Bad or misleading photos",
            ReportReason.WrongPrice => "Wrong price",
            ReportReason.ScamUser => "Scam user",
            ReportReason.InappropriateContent => "Inappropriate content",
            _ => "Other"
        };
    }
}
