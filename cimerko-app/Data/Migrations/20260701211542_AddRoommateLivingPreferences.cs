using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cimerko_app.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoommateLivingPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CleanlinessLevel",
                table: "RoommateProfiles",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestPreference",
                table: "RoommateProfiles",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PetsPreference",
                table: "RoommateProfiles",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SleepSchedule",
                table: "RoommateProfiles",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmokingPreference",
                table: "RoommateProfiles",
                type: "TEXT",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CleanlinessLevel",
                table: "RoommateProfiles");

            migrationBuilder.DropColumn(
                name: "GuestPreference",
                table: "RoommateProfiles");

            migrationBuilder.DropColumn(
                name: "PetsPreference",
                table: "RoommateProfiles");

            migrationBuilder.DropColumn(
                name: "SleepSchedule",
                table: "RoommateProfiles");

            migrationBuilder.DropColumn(
                name: "SmokingPreference",
                table: "RoommateProfiles");
        }
    }
}
