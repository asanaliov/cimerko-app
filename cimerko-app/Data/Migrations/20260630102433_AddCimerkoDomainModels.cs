using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cimerko_app.Data.Migrations {
    /// <inheritdoc />
    public partial class AddCimerkoDomainModels : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDemoUser",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Listings",
                columns: table => new {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    MonthlyRent = table.Column<decimal>(type: "TEXT", nullable: false),
                    RoomCount = table.Column<int>(type: "INTEGER", nullable: false),
                    AvailableFrom = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Listings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Listings_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReviewerId = table.Column<string>(type: "TEXT", nullable: false),
                    ReviewedUserId = table.Column<string>(type: "TEXT", nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_AspNetUsers_ReviewedUserId",
                        column: x => x.ReviewedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_AspNetUsers_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoommateProfiles",
                columns: table => new {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Bio = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Age = table.Column<int>(type: "INTEGER", nullable: false),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    University = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    StudyProgram = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_RoommateProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoommateProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListingRequests",
                columns: table => new {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ListingId = table.Column<int>(type: "INTEGER", nullable: false),
                    SenderId = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_ListingRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListingRequests_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ListingRequests_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedListings",
                columns: table => new {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ListingId = table.Column<int>(type: "INTEGER", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_SavedListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedListings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedListings_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListingRequests_ListingId",
                table: "ListingRequests",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_ListingRequests_SenderId",
                table: "ListingRequests",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_OwnerId",
                table: "Listings",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewedUserId",
                table: "Reviews",
                column: "ReviewedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewerId",
                table: "Reviews",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_RoommateProfiles_UserId",
                table: "RoommateProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedListings_ListingId",
                table: "SavedListings",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedListings_UserId_ListingId",
                table: "SavedListings",
                columns: new[] { "UserId", "ListingId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "ListingRequests");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "RoommateProfiles");

            migrationBuilder.DropTable(
                name: "SavedListings");

            migrationBuilder.DropTable(
                name: "Listings");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsDemoUser",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "AspNetUsers");
        }
    }
}
