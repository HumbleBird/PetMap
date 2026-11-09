using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMap.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovePetEntryCriteriaAndAddRestaurantType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE dbo.Places
                SET Description = CASE
                    WHEN NULLIF(LTRIM(RTRIM(Description)), N'') IS NULL
                        THEN N'반려동물 이용 안내: ' + PetEntryCriteria
                    ELSE Description + CHAR(13) + CHAR(10) + N'반려동물 이용 안내: ' + PetEntryCriteria
                END
                WHERE NULLIF(LTRIM(RTRIM(PetEntryCriteria)), N'') IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "PetEntryCriteria",
                table: "Places");

            migrationBuilder.AddColumn<string>(
                name: "RestaurantType",
                table: "PlaceReports",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RestaurantType",
                table: "PlaceReports");

            migrationBuilder.AddColumn<string>(
                name: "PetEntryCriteria",
                table: "Places",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
