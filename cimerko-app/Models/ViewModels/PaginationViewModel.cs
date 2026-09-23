namespace cimerko_app.Models.ViewModels;

public record PaginationViewModel(int Page, int TotalPages) {
    public const int DefaultPageSize = 12;

    public static int TotalPagesFor(int totalCount, int pageSize = DefaultPageSize) {
        return Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public static int ClampPage(int page, int totalPages) {
        return Math.Clamp(page, 1, totalPages);
    }
}
