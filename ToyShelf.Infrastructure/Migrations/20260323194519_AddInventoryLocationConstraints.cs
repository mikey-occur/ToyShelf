using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryLocationConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryLocations_StoreId",
                table: "InventoryLocations");

            migrationBuilder.DropIndex(
                name: "IX_InventoryLocations_WarehouseId",
                table: "InventoryLocations");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLocations_StoreId",
                table: "InventoryLocations",
                column: "StoreId",
                unique: true,
                filter: "\"StoreId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLocations_WarehouseId",
                table: "InventoryLocations",
                column: "WarehouseId",
                unique: true,
                filter: "\"WarehouseId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InventoryLocation_OnlyOneOwner",
                table: "InventoryLocations",
                sql: "(\"WarehouseId\" IS NOT NULL AND \"StoreId\" IS NULL) OR (\"WarehouseId\" IS NULL AND \"StoreId\" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryLocations_StoreId",
                table: "InventoryLocations");

            migrationBuilder.DropIndex(
                name: "IX_InventoryLocations_WarehouseId",
                table: "InventoryLocations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InventoryLocation_OnlyOneOwner",
                table: "InventoryLocations");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLocations_StoreId",
                table: "InventoryLocations",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLocations_WarehouseId",
                table: "InventoryLocations",
                column: "WarehouseId");
        }
    }
}
