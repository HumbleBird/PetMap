using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMap.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaceReportRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "PlaceReports",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "PlaceReports");
        }
    }
}
