using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cimerko_app.Data.Migrations
{
    /// <inheritdoc />
    public partial class PreventDuplicateListingRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "ListingRequests"
                WHERE "Id" NOT IN (
                    SELECT MIN("Id")
                    FROM "ListingRequests"
                    GROUP BY "SenderId", "ListingId"
                );
                """);

            migrationBuilder.DropIndex(
                name: "IX_ListingRequests_SenderId",
                table: "ListingRequests");

            migrationBuilder.CreateIndex(
                name: "IX_ListingRequests_SenderId_ListingId",
                table: "ListingRequests",
                columns: new[] { "SenderId", "ListingId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ListingRequests_SenderId_ListingId",
                table: "ListingRequests");

            migrationBuilder.CreateIndex(
                name: "IX_ListingRequests_SenderId",
                table: "ListingRequests",
                column: "SenderId");
        }
    }
}
