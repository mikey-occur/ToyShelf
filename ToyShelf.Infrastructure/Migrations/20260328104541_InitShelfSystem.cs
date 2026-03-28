using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitShelfSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "Shelves");

            migrationBuilder.AddColumn<Guid>(
                name: "ShelfTypeId",
                table: "Shelves",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ShelfTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Width = table.Column<double>(type: "double precision", nullable: false),
                    Height = table.Column<double>(type: "double precision", nullable: false),
                    Depth = table.Column<double>(type: "double precision", nullable: false),
                    TotalLevels = table.Column<int>(type: "integer", nullable: false),
                    SuitableProductCategoryTypes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayGuideline = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShelfTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shelfTypeLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShelfTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClearanceHeight = table.Column<double>(type: "double precision", nullable: false),
                    MaxWeightCapacity = table.Column<double>(type: "double precision", nullable: false),
                    RecommendedCapacity = table.Column<int>(type: "integer", nullable: false),
                    SuitableProductCategoryTypes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayGuideline = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shelfTypeLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShelfTypeLevel_ShelfType",
                        column: x => x.ShelfTypeId,
                        principalTable: "ShelfTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shelves_ShelfTypeId",
                table: "Shelves",
                column: "ShelfTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_shelfTypeLevels_ShelfTypeId",
                table: "shelfTypeLevels",
                column: "ShelfTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shelf_ShelfType",
                table: "Shelves",
                column: "ShelfTypeId",
                principalTable: "ShelfTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shelf_ShelfType",
                table: "Shelves");

            migrationBuilder.DropTable(
                name: "shelfTypeLevels");

            migrationBuilder.DropTable(
                name: "ShelfTypes");

            migrationBuilder.DropIndex(
                name: "IX_Shelves_ShelfTypeId",
                table: "Shelves");

            migrationBuilder.DropColumn(
                name: "ShelfTypeId",
                table: "Shelves");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Shelves",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
