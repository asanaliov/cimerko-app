using cimerko_app.Models;
using cimerko_app.Services;

namespace Tests;

public class ProfileCompatibilityCalculatorTests {
    [Fact]
    public void Calculate_returns_one_hundred_for_identical_complete_preferences() {
        var first = CreateProfile();
        var second = CreateProfile();

        var result = ProfileCompatibilityCalculator.Calculate(first, second);

        Assert.Equal(100, result.Score);
        Assert.Equal(5, result.ComparedPreferences);
        Assert.Equal(5, result.StrongMatches.Count);
        Assert.Empty(result.PossibleConflicts);
    }

    [Fact]
    public void Calculate_returns_no_score_when_too_few_preferences_can_be_compared() {
        var first = new RoommateProfile {
            SmokingPreference = "No smoking"
        };
        var second = new RoommateProfile {
            SmokingPreference = "No smoking"
        };

        var result = ProfileCompatibilityCalculator.Calculate(first, second);

        Assert.Null(result.Score);
        Assert.Equal(1, result.ComparedPreferences);
    }

    [Fact]
    public void Calculate_marks_opposing_preferences_as_possible_conflicts() {
        var first = CreateProfile();
        var second = CreateProfile();
        second.SmokingPreference = "Smoking is okay";
        second.PetsPreference = "No pets";
        second.SleepSchedule = "Night owl";

        var result = ProfileCompatibilityCalculator.Calculate(first, second);

        Assert.NotNull(result.Score);
        Assert.True(result.Score < 70);
        Assert.Contains("Smoking", result.PossibleConflicts);
        Assert.Contains("Pets", result.PossibleConflicts);
        Assert.Contains("Sleep schedule", result.PossibleConflicts);
    }

    [Fact]
    public void Calculate_is_symmetric() {
        var first = CreateProfile();
        var second = CreateProfile();
        second.CleanlinessLevel = "Balanced";
        second.GuestPreference = "Occasionally";

        var forward = ProfileCompatibilityCalculator.Calculate(first, second);
        var reverse = ProfileCompatibilityCalculator.Calculate(second, first);

        Assert.Equal(forward.Score, reverse.Score);
        Assert.Equal(forward.StrongMatches, reverse.StrongMatches);
        Assert.Equal(forward.PossibleConflicts, reverse.PossibleConflicts);
    }

    private static RoommateProfile CreateProfile() {
        return new RoommateProfile {
            SmokingPreference = "No smoking",
            PetsPreference = "Pets are welcome",
            CleanlinessLevel = "Very tidy",
            SleepSchedule = "Early bird",
            GuestPreference = "Rarely"
        };
    }
}
