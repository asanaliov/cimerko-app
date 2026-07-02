namespace cimerko_app.Models.ViewModels;

public class ProfileCompletionViewModel {
    public ProfileCompletionViewModel(
        int profileCompletionPercentage,
        IReadOnlyList<string> missingProfileFields) {
        ProfileCompletionPercentage = profileCompletionPercentage;
        MissingProfileFields = missingProfileFields;
    }

    public int ProfileCompletionPercentage { get; }

    public IReadOnlyList<string> MissingProfileFields { get; }

    public bool IsProfileComplete => MissingProfileFields.Count == 0;
}
