using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cimerko_app.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailedReviewRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CleanlinessRating",
                table: "Reviews",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuestPreferenceRating",
                table: "Reviews",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PetsRating",
                table: "Reviews",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SleepScheduleRating",
                table: "Reviews",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SmokingRating",
                table: "Reviews",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CleanlinessRating",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "GuestPreferenceRating",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "PetsRating",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "SleepScheduleRating",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "SmokingRating",
                table: "Reviews");
        }
    }
}
