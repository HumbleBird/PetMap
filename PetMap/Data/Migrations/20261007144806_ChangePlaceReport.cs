using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMap.Migrations
{
    /// <inheritdoc />
    public partial class ChangePlaceReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JibunAddress",
                table: "PlaceReports");

            migrationBuilder.DropColumn(
                name: "RoadAddress",
                table: "PlaceReports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JibunAddress",
                table: "PlaceReports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RoadAddress",
                table: "PlaceReports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
