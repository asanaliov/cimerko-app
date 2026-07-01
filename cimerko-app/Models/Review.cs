using System.ComponentModel.DataAnnotations;

namespace cimerko_app.Models;

public class Review {
    public int Id { get; set; }

    [Required]
    public string ReviewerId { get; set; } = string.Empty;

    public ApplicationUser? Reviewer { get; set; }

    [Required]
    public string ReviewedUserId { get; set; } = string.Empty;

    public ApplicationUser? ReviewedUser { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Range(1, 5)]
    [Display(Name = "Smoking compatibility")]
    public int? SmokingRating { get; set; }

    [Range(1, 5)]
    [Display(Name = "Pets compatibility")]
    public int? PetsRating { get; set; }

    [Range(1, 5)]
    [Display(Name = "Cleanliness")]
    public int? CleanlinessRating { get; set; }

    [Range(1, 5)]
    [Display(Name = "Sleep schedule compatibility")]
    public int? SleepScheduleRating { get; set; }

    [Range(1, 5)]
    [Display(Name = "Guest habits")]
    public int? GuestPreferenceRating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
