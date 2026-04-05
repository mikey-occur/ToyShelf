using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShelfIdToShelfShipmentItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShelfShipmentItem_ShelfType",
                table: "ShelfShipmentItems");

            migrationBuilder.DropColumn(
                name: "ExpectedQuantity",
                table: "ShelfShipmentItems");

            migrationBuilder.DropColumn(
                name: "ReceivedQuantity",
                table: "ShelfShipmentItems");

            migrationBuilder.RenameColumn(
                name: "ShelfTypeId",
                table: "ShelfShipmentItems",
                newName: "ShelfId");

            migrationBuilder.RenameIndex(
                name: "IX_ShelfShipmentItems_ShelfTypeId",
                table: "ShelfShipmentItems",
                newName: "IX_ShelfShipmentItems_ShelfId");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ShelfShipmentItems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_ShelfShipmentItem_Shelf",
                table: "ShelfShipmentItems",
                column: "ShelfId",
                principalTable: "Shelves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShelfShipmentItem_Shelf",
                table: "ShelfShipmentItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ShelfShipmentItems");

            migrationBuilder.RenameColumn(
                name: "ShelfId",
                table: "ShelfShipmentItems",
                newName: "ShelfTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_ShelfShipmentItems_ShelfId",
                table: "ShelfShipmentItems",
                newName: "IX_ShelfShipmentItems_ShelfTypeId");

            migrationBuilder.AddColumn<int>(
                name: "ExpectedQuantity",
                table: "ShelfShipmentItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReceivedQuantity",
                table: "ShelfShipmentItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_ShelfShipmentItem_ShelfType",
                table: "ShelfShipmentItems",
                column: "ShelfTypeId",
                principalTable: "ShelfTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
