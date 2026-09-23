using System.Globalization;
using System.Text;

namespace cimerko_app.Services;

public static class TextSearch {
    public static string Normalize(string? value) {
        if (string.IsNullOrWhiteSpace(value)) {
            return string.Empty;
        }

        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed) {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark) {
                builder.Append(character == 'đ' ? 'd' : character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string[] Terms(string? query) {
        return Normalize(query).Split(
            [' ', ',', '.', '-', '/'],
            StringSplitOptions.RemoveEmptyEntries);
    }

    // Every term has to appear in at least one of the fields, so "room karpos" finds a room in Karpoš.
    public static bool MatchesAll(IReadOnlyCollection<string> terms, params string?[] fields) {
        if (terms.Count == 0) {
            return true;
        }

        var text = Normalize(string.Join(' ', fields));
        return terms.All(text.Contains);
    }
}
