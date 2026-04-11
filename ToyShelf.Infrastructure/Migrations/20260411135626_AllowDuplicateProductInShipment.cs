using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowDuplicateProductInShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShipmentItems_ShipmentId_ProductColorId",
                table: "ShipmentItems");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_ShipmentId",
                table: "ShipmentItems",
                column: "ShipmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShipmentItems_ShipmentId",
                table: "ShipmentItems");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_ShipmentId_ProductColorId",
                table: "ShipmentItems",
                columns: new[] { "ShipmentId", "ProductColorId" },
                unique: true);
        }
    }
}
