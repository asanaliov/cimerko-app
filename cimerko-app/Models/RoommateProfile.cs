using System.ComponentModel.DataAnnotations;

namespace cimerko_app.Models;

public class RoommateProfile {
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    [MaxLength(1000)]
    public string? Bio { get; set; }

    [Range(18, 100)]
    public int Age { get; set; }

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Gender { get; set; }

    [MaxLength(100)]
    public string? University { get; set; }

    [MaxLength(100)]
    public string? StudyProgram { get; set; }
}
