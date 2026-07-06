using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cimerko_app.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModerationStatus",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE Listings
                SET ModerationStatus = CASE
                    WHEN IsActive = 1 THEN 1
                    ELSE 3
                END
                """);

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReporterId = table.Column<string>(type: "TEXT", nullable: false),
                    TargetType = table.Column<int>(type: "INTEGER", nullable: false),
                    ListingId = table.Column<int>(type: "INTEGER", nullable: true),
                    ReportedUserId = table.Column<string>(type: "TEXT", nullable: true),
                    ReviewId = table.Column<int>(type: "INTEGER", nullable: true),
                    Reason = table.Column<int>(type: "INTEGER", nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_AspNetUsers_ReportedUserId",
                        column: x => x.ReportedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reports_AspNetUsers_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reports_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reports_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ListingId",
                table: "Reports",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportedUserId",
                table: "Reports",
                column: "ReportedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReporterId",
                table: "Reports",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReviewId",
                table: "Reports",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Status_CreatedAt",
                table: "Reports",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropColumn(
                name: "ModerationStatus",
                table: "Listings");
        }
    }
}
