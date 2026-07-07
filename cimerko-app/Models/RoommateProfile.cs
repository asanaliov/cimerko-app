using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using cimerko_app.Models.Validation;

namespace cimerko_app.Models;

public class RoommateProfile {
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    [MaxLength(1000)]
    public string? Bio { get; set; }

    [EmailAddress]
    [MaxLength(256)]
    [Display(Name = "Contact email")]
    public string? ContactEmail { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date of birth")]
    [ValidDateOfBirth]
    public DateOnly? DateOfBirth { get; set; }

    [Column("Age")]
    public int LegacyAge { get; private set; }

    [NotMapped]
    public int? Age => DateOfBirth.HasValue
        ? CalculateAge(DateOfBirth.Value, DateOnly.FromDateTime(DateTime.Today))
        : LegacyAge > 0
            ? LegacyAge
            : null;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(30)]
    [RegularExpression(
        "^(Male|Female|Prefer not to say)$",
        ErrorMessage = "Choose Male, Female, or Prefer not to say.")]
    public string? Gender { get; set; }

    [MaxLength(100)]
    public string? University { get; set; }

    [MaxLength(100)]
    [Display(Name = "Faculty")]
    public string? StudyProgram { get; set; }

    [MaxLength(30)]
    [Display(Name = "Smoking preference")]
    public string? SmokingPreference { get; set; }

    [MaxLength(30)]
    [Display(Name = "Pets preference")]
    public string? PetsPreference { get; set; }

    [MaxLength(30)]
    [Display(Name = "Cleanliness level")]
    public string? CleanlinessLevel { get; set; }

    [MaxLength(30)]
    [Display(Name = "Sleep schedule")]
    public string? SleepSchedule { get; set; }

    [MaxLength(30)]
    [Display(Name = "Guest preference")]
    public string? GuestPreference { get; set; }

    public static int CalculateAge(DateOnly dateOfBirth, DateOnly onDate) {
        var age = onDate.Year - dateOfBirth.Year;
        if (dateOfBirth > onDate.AddYears(-age)) {
            age--;
        }

        return age;
    }
}
