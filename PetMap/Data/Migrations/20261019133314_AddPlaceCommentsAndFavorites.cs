using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMap.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaceCommentsAndFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "PlaceReports");

            migrationBuilder.CreateTable(
                name: "PlaceComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlaceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AuthorName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    GuestPasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ImagePathsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LikesCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaceComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaceComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PlaceComments_Places_PlaceId",
                        column: x => x.PlaceId,
                        principalTable: "Places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaceFavorites",
                columns: table => new
                {
                    PlaceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaceFavorites", x => new { x.PlaceId, x.UserId });
                    table.ForeignKey(
                        name: "FK_PlaceFavorites_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaceFavorites_Places_PlaceId",
                        column: x => x.PlaceId,
                        principalTable: "Places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaceComments_PlaceId_CreatedAt",
                table: "PlaceComments",
                columns: new[] { "PlaceId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PlaceComments_UserId",
                table: "PlaceComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceFavorites_UserId",
                table: "PlaceFavorites",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaceComments");

            migrationBuilder.DropTable(
                name: "PlaceFavorites");

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "PlaceReports",
                type: "int",
                nullable: true);
        }
    }
}
