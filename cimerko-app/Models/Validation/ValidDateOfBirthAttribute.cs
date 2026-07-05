using System.ComponentModel.DataAnnotations;

namespace cimerko_app.Models.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class ValidDateOfBirthAttribute : ValidationAttribute {
    private const int MinimumAge = 18;
    private const int MaximumAge = 100;

    public ValidDateOfBirthAttribute()
        : base("Enter a date of birth for an age between 18 and 100.") {
    }

    public override bool IsValid(object? value) {
        if (value == null) {
            return false;
        }

        if (value is not DateOnly dateOfBirth) {
            return false;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = RoommateProfile.CalculateAge(dateOfBirth, today);
        return age is >= MinimumAge and <= MaximumAge;
    }
}
