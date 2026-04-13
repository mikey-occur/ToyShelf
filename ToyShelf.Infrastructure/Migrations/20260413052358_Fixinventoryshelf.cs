using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fixinventoryshelf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryShelves_InventoryLocations_InventoryLocationId",
                table: "InventoryShelves");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryShelves_ShelfTypes_ShelfTypeId",
                table: "InventoryShelves");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InventoryShelves",
                table: "InventoryShelves");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InventoryShelves",
                table: "InventoryShelves",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryShelves_InventoryLocationId",
                table: "InventoryShelves",
                column: "InventoryLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryShelf_InventoryLocation",
                table: "InventoryShelves",
                column: "InventoryLocationId",
                principalTable: "InventoryLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryShelf_ShelfType",
                table: "InventoryShelves",
                column: "ShelfTypeId",
                principalTable: "ShelfTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryShelf_InventoryLocation",
                table: "InventoryShelves");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryShelf_ShelfType",
                table: "InventoryShelves");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InventoryShelves",
                table: "InventoryShelves");

            migrationBuilder.DropIndex(
                name: "IX_InventoryShelves_InventoryLocationId",
                table: "InventoryShelves");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InventoryShelves",
                table: "InventoryShelves",
                columns: new[] { "InventoryLocationId", "ShelfTypeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryShelves_InventoryLocations_InventoryLocationId",
                table: "InventoryShelves",
                column: "InventoryLocationId",
                principalTable: "InventoryLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryShelves_ShelfTypes_ShelfTypeId",
                table: "InventoryShelves",
                column: "ShelfTypeId",
                principalTable: "ShelfTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
