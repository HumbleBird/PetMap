using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMap.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaceRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlaceRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlaceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaceRatings", rating => rating.Id);
                    table.CheckConstraint("CK_PlaceRatings_Score_Range", "[Score] >= 1 AND [Score] <= 5");
                    table.ForeignKey(
                        name: "FK_PlaceRatings_AspNetUsers_UserId",
                        column: rating => rating.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaceRatings_Places_PlaceId",
                        column: rating => rating.PlaceId,
                        principalTable: "Places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaceRatings_PlaceId_UserId",
                table: "PlaceRatings",
                columns: new[] { "PlaceId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaceRatings_UserId",
                table: "PlaceRatings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PlaceRatings");
        }
    }
}
