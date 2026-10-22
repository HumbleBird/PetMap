using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMap.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityCoursesAndCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PetCourses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    District = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Neighborhood = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetCourses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PetCourses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaceCollections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    City = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    District = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Neighborhood = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Memo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaceCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaceCollections_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaceViews",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlaceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ViewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaceViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaceViews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PlaceViews_Places_PlaceId",
                        column: x => x.PlaceId,
                        principalTable: "Places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PetCourseFavorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetCourseFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PetCourseFavorites_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PetCourseFavorites_PetCourses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "PetCourses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PetCourseLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetCourseLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PetCourseLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PetCourseLikes_PetCourses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "PetCourses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PetCourseStops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    PlaceId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Memo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetCourseStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PetCourseStops_PetCourses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "PetCourses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PetCourseStops_Places_PlaceId",
                        column: x => x.PlaceId,
                        principalTable: "Places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlaceCollectionItems",
                columns: table => new
                {
                    CollectionId = table.Column<int>(type: "int", nullable: false),
                    PlaceId = table.Column<int>(type: "int", nullable: false),
                    Memo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaceCollectionItems", x => new { x.CollectionId, x.PlaceId });
                    table.ForeignKey(
                        name: "FK_PlaceCollectionItems_PlaceCollections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "PlaceCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaceCollectionItems_Places_PlaceId",
                        column: x => x.PlaceId,
                        principalTable: "Places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PetCourseFavorites_UserId",
                table: "PetCourseFavorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PetCourseFavorites_CourseId_UserId",
                table: "PetCourseFavorites",
                columns: new[] { "CourseId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PetCourseLikes_UserId",
                table: "PetCourseLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PetCourseLikes_CourseId_UserId",
                table: "PetCourseLikes",
                columns: new[] { "CourseId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PetCourses_IsPublic_CreatedAtUtc",
                table: "PetCourses",
                columns: new[] { "IsPublic", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PetCourses_UserId",
                table: "PetCourses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PetCourseStops_CourseId_PlaceId",
                table: "PetCourseStops",
                columns: new[] { "CourseId", "PlaceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PetCourseStops_CourseId_SortOrder",
                table: "PetCourseStops",
                columns: new[] { "CourseId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PetCourseStops_PlaceId",
                table: "PetCourseStops",
                column: "PlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceCollectionItems_PlaceId",
                table: "PlaceCollectionItems",
                column: "PlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceCollections_UserId_Name",
                table: "PlaceCollections",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaceViews_PlaceId_ViewedAtUtc",
                table: "PlaceViews",
                columns: new[] { "PlaceId", "ViewedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PlaceViews_UserId",
                table: "PlaceViews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PetCourseFavorites");

            migrationBuilder.DropTable(
                name: "PetCourseLikes");

            migrationBuilder.DropTable(
                name: "PetCourseStops");

            migrationBuilder.DropTable(
                name: "PlaceCollectionItems");

            migrationBuilder.DropTable(
                name: "PlaceViews");

            migrationBuilder.DropTable(
                name: "PetCourses");

            migrationBuilder.DropTable(
                name: "PlaceCollections");

        }
    }
}
