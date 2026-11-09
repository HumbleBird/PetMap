using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMap.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaceRequestApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "PlaceReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JibunAddress",
                table: "PlaceReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "PlaceReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoadAddress",
                table: "PlaceReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetPlaceId",
                table: "PlaceReports",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "PlaceReports");

            migrationBuilder.DropColumn(
                name: "JibunAddress",
                table: "PlaceReports");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "PlaceReports");

            migrationBuilder.DropColumn(
                name: "RoadAddress",
                table: "PlaceReports");

            migrationBuilder.DropColumn(
                name: "TargetPlaceId",
                table: "PlaceReports");
        }
    }
}
