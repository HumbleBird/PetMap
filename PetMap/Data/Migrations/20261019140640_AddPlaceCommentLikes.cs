using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMap.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaceCommentLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlaceCommentLikes",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaceCommentLikes", x => new { x.CommentId, x.UserId });
                    table.ForeignKey(
                        name: "FK_PlaceCommentLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaceCommentLikes_PlaceComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "PlaceComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 기존 LikesCount는 회원별 좋아요 기록 없이 증가했으므로 새 기록 기준으로 초기화한다.
            migrationBuilder.Sql("UPDATE [PlaceComments] SET [LikesCount] = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceCommentLikes_UserId",
                table: "PlaceCommentLikes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaceCommentLikes");
        }
    }
}
