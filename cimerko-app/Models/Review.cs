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

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
