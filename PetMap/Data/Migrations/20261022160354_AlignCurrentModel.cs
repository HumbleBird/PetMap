using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMap.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlignCurrentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FolderName",
                table: "PlaceFavorites",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Memo",
                table: "PlaceFavorites",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FolderName",
                table: "PlaceFavorites");

            migrationBuilder.DropColumn(
                name: "Memo",
                table: "PlaceFavorites");
        }
    }
}
