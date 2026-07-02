namespace cimerko_app.Models.ViewModels;

public class ProfileCompletionViewModel {
    public ProfileCompletionViewModel(
        int profileCompletionPercentage,
        IReadOnlyList<MissingProfileFieldViewModel> missingProfileFields) {
        ProfileCompletionPercentage = profileCompletionPercentage;
        MissingProfileFields = missingProfileFields;
    }

    public int ProfileCompletionPercentage { get; }

    public IReadOnlyList<MissingProfileFieldViewModel> MissingProfileFields { get; }

    public bool IsProfileComplete => MissingProfileFields.Count == 0;
}

public class MissingProfileFieldViewModel {
    public MissingProfileFieldViewModel(string name, string editAnchor) {
        Name = name;
        EditAnchor = editAnchor;
    }

    public string Name { get; }

    public string EditAnchor { get; }
}
