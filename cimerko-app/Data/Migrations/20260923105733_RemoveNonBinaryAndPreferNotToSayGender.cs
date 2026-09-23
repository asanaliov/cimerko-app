using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cimerko_app.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNonBinaryAndPreferNotToSayGender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Gender is now Male or Female only, and the "Non-binary people" roommate preference (3) was removed.
            migrationBuilder.Sql("UPDATE RoommateProfiles SET Gender = NULL WHERE Gender = 'Prefer not to say';");
            migrationBuilder.Sql("UPDATE Listings SET RoommateGenderPreference = 0 WHERE RoommateGenderPreference = 3;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The removed values cannot be restored.
        }
    }
}
