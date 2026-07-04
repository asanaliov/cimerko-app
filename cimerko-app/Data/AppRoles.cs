namespace cimerko_app.Data;

public static class AppRoles {
    public const string Admin = "Admin";
    public const string Student = "Student";
    public const string Landlord = "Landlord";

    public static readonly string[] All = [Admin, Student, Landlord];

    public static bool IsSelectable(string role) {
        return role is Student or Landlord;
    }
}
