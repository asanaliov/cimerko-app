using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cimerko_app.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddListingContactAndRoommatePreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Listings",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RoommateEarlyBird",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RoommateGuestsWelcome",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RoommateHousingPlan",
                table: "Listings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RoommateNightOwl",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RoommatePetFriendly",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RoommateSmokeFree",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RoommateTidy",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RoommateEarlyBird",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RoommateGuestsWelcome",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RoommateHousingPlan",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RoommateNightOwl",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RoommatePetFriendly",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RoommateSmokeFree",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RoommateTidy",
                table: "Listings");
        }
    }
}
