using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cimerko_app.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeListingIntentTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE Listings
                SET Type = CASE
                    WHEN Type = 3 THEN 1
                    WHEN Type IN (1, 2) THEN 2
                    ELSE Type
                END;
                """);

            migrationBuilder.Sql(
                """
                UPDATE Listings
                SET RoommatesNeeded = NULL
                WHERE Type = 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE Listings
                SET RoommatesNeeded = 1
                WHERE RoommatesNeeded IS NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE Listings
                SET Type = CASE
                    WHEN Type = 1 THEN 3
                    WHEN Type = 2 THEN 2
                    ELSE Type
                END;
                """);
        }
    }
}
