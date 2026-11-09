using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMap.Data.Migrations;

public partial class AddUserNickname : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Nickname",
            table: "AspNetUsers",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: true);

        migrationBuilder.Sql("UPDATE dbo.AspNetUsers SET Nickname = LEFT(COALESCE(NULLIF(UserName, N''), N'회원'), 30) WHERE Nickname IS NULL;");

        migrationBuilder.AlterColumn<string>(
            name: "Nickname",
            table: "AspNetUsers",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(30)",
            oldMaxLength: 30,
            oldNullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Nickname", table: "AspNetUsers");
    }
}
