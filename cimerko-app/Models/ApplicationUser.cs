using Microsoft.AspNetCore.Identity;

namespace cimerko_app.Models;

public class ApplicationUser : IdentityUser {
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string? Role { get; set; }
    public bool IsDemoUser { get; set; }
    public DateTime CreatedAt { get; set; }
}