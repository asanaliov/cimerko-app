using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cimerko_app.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddListingPreferencesAndBedrooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BedroomCount",
                table: "Listings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RentalPetPolicy",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RentalSmokingPolicy",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RoommateGenderPreference",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantTypePreference",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BedroomCount",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RentalPetPolicy",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RentalSmokingPolicy",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RoommateGenderPreference",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "TenantTypePreference",
                table: "Listings");
        }
    }
}
