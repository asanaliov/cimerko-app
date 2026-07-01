using cimerko_app.Models;

namespace cimerko_app.Services;

public static class ProfileCompatibilityCalculator {
    private const double SmokingWeight = 30;
    private const double CleanlinessWeight = 25;
    private const double SleepScheduleWeight = 20;
    private const double GuestPreferenceWeight = 15;
    private const double PetsWeight = 10;
    private const double StrongMatchThreshold = 0.65;
    private const int MinimumComparedPreferences = 2;

    public static (
        int? Score,
        int ComparedPreferences,
        IReadOnlyList<string> StrongMatches,
        IReadOnlyList<string> PossibleConflicts) Calculate(
        RoommateProfile? firstProfile,
        RoommateProfile? secondProfile) {
        if (firstProfile == null || secondProfile == null) {
            return (null, 0, Array.Empty<string>(), Array.Empty<string>());
        }

        double earnedWeight = 0;
        double availableWeight = 0;
        var comparedPreferences = 0;
        var strongMatches = new List<string>();
        var possibleConflicts = new List<string>();

        AddCategory(
            "Smoking",
            firstProfile.SmokingPreference,
            secondProfile.SmokingPreference,
            SmokingWeight,
            ScoreSmoking,
            strongMatches,
            possibleConflicts,
            ref earnedWeight,
            ref availableWeight,
            ref comparedPreferences);
        AddCategory(
            "Cleanliness",
            firstProfile.CleanlinessLevel,
            secondProfile.CleanlinessLevel,
            CleanlinessWeight,
            (first, second) => ScoreOrderedPreference(
                first,
                second,
                new[] { "Relaxed", "Balanced", "Very tidy" },
                0.7,
                0.15),
            strongMatches,
            possibleConflicts,
            ref earnedWeight,
            ref availableWeight,
            ref comparedPreferences);
        AddCategory(
            "Sleep schedule",
            firstProfile.SleepSchedule,
            secondProfile.SleepSchedule,
            SleepScheduleWeight,
            (first, second) => ScoreOrderedPreference(
                first,
                second,
                new[] { "Early bird", "Flexible", "Night owl" },
                0.65,
                0),
            strongMatches,
            possibleConflicts,
            ref earnedWeight,
            ref availableWeight,
            ref comparedPreferences);
        AddCategory(
            "Guest preference",
            firstProfile.GuestPreference,
            secondProfile.GuestPreference,
            GuestPreferenceWeight,
            (first, second) => ScoreOrderedPreference(
                first,
                second,
                new[] { "Rarely", "Occasionally", "Often" },
                0.65,
                0.2),
            strongMatches,
            possibleConflicts,
            ref earnedWeight,
            ref availableWeight,
            ref comparedPreferences);
        AddCategory(
            "Pets",
            firstProfile.PetsPreference,
            secondProfile.PetsPreference,
            PetsWeight,
            ScorePets,
            strongMatches,
            possibleConflicts,
            ref earnedWeight,
            ref availableWeight,
            ref comparedPreferences);

        if (comparedPreferences < MinimumComparedPreferences) {
            return (null, comparedPreferences, strongMatches, possibleConflicts);
        }

        var score = (int)Math.Round(
            earnedWeight / availableWeight * 100,
            MidpointRounding.AwayFromZero);

        return (score, comparedPreferences, strongMatches, possibleConflicts);
    }

    private static void AddCategory(
        string categoryLabel,
        string? firstPreference,
        string? secondPreference,
        double weight,
        Func<string, string, double> scorePreference,
        ICollection<string> strongMatches,
        ICollection<string> possibleConflicts,
        ref double earnedWeight,
        ref double availableWeight,
        ref int comparedPreferences) {
        if (string.IsNullOrWhiteSpace(firstPreference) ||
            string.IsNullOrWhiteSpace(secondPreference)) {
            return;
        }

        var categoryScore = scorePreference(firstPreference, secondPreference);
        earnedWeight += weight * categoryScore;
        availableWeight += weight;
        comparedPreferences++;

        if (categoryScore >= StrongMatchThreshold) {
            strongMatches.Add(categoryLabel);
        }
        else {
            possibleConflicts.Add(categoryLabel);
        }
    }

    private static double ScoreSmoking(string firstPreference, string secondPreference) {
        var first = Normalize(firstPreference);
        var second = Normalize(secondPreference);

        if (first == second) {
            return 1;
        }

        if (IsPair(first, second, "no smoking", "outside only")) {
            return 0.4;
        }

        if (IsPair(first, second, "outside only", "smoking is okay")) {
            return 0.5;
        }

        return 0;
    }

    private static double ScorePets(string firstPreference, string secondPreference) {
        var first = Normalize(firstPreference);
        var second = Normalize(secondPreference);

        if (first == second) {
            return 1;
        }

        if (IsPair(first, second, "no pets", "ask first")) {
            return 0.5;
        }

        if (IsPair(first, second, "ask first", "pets are welcome")) {
            return 0.75;
        }

        if (IsPair(first, second, "no pets", "pets are welcome")) {
            return 0.1;
        }

        return 0;
    }

    private static double ScoreOrderedPreference(
        string firstPreference,
        string secondPreference,
        string[] orderedValues,
        double adjacentScore,
        double oppositeScore) {
        var firstIndex = Array.FindIndex(
            orderedValues,
            value => string.Equals(value, firstPreference.Trim(), StringComparison.OrdinalIgnoreCase));
        var secondIndex = Array.FindIndex(
            orderedValues,
            value => string.Equals(value, secondPreference.Trim(), StringComparison.OrdinalIgnoreCase));

        if (firstIndex < 0 || secondIndex < 0) {
            return string.Equals(
                firstPreference.Trim(),
                secondPreference.Trim(),
                StringComparison.OrdinalIgnoreCase)
                ? 1
                : 0;
        }

        return Math.Abs(firstIndex - secondIndex) switch {
            0 => 1,
            1 => adjacentScore,
            _ => oppositeScore
        };
    }

    private static bool IsPair(
        string first,
        string second,
        string expectedFirst,
        string expectedSecond) {
        return (first == expectedFirst && second == expectedSecond) ||
               (first == expectedSecond && second == expectedFirst);
    }

    private static string Normalize(string preference) {
        return preference.Trim().ToLowerInvariant();
    }
}
