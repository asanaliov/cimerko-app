using System.Collections.Generic;
using cimerko_app.Models;

namespace cimerko_app.Models.ViewModels
{
    public class RoommateSearchResultViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public int? CompatibilityScore { get; set; }
    }

    public class RoommateSearchViewModel
    {
        public List<RoommateSearchResultViewModel> Results { get; set; } = new List<RoommateSearchResultViewModel>();
    }
}
