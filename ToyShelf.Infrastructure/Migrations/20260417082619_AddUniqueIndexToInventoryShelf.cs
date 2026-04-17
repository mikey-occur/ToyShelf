using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToInventoryShelf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryShelves_InventoryLocationId",
                table: "InventoryShelves");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryShelves_InventoryLocationId_ShelfTypeId_Status",
                table: "InventoryShelves",
                columns: new[] { "InventoryLocationId", "ShelfTypeId", "Status" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryShelves_InventoryLocationId_ShelfTypeId_Status",
                table: "InventoryShelves");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryShelves_InventoryLocationId",
                table: "InventoryShelves",
                column: "InventoryLocationId");
        }
    }
}
