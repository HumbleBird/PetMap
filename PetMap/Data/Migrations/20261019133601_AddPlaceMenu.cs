using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMap.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaceMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Menu",
                table: "Places",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Menu",
                table: "Places");
        }
    }
}
