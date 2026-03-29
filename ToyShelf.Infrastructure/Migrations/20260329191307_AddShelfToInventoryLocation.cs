using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShelfToInventoryLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shelf_Partner",
                table: "Shelves");

            migrationBuilder.DropForeignKey(
                name: "FK_Shelf_Store",
                table: "Shelves");

            migrationBuilder.DropIndex(
                name: "IX_Shelves_PartnerId",
                table: "Shelves");

            migrationBuilder.DropIndex(
                name: "IX_Shelves_StoreId",
                table: "Shelves");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "Shelves");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Shelves");

            migrationBuilder.AddColumn<Guid>(
                name: "InventoryLocationId",
                table: "Shelves",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Shelves_InventoryLocationId",
                table: "Shelves",
                column: "InventoryLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shelf_InventoryLocation",
                table: "Shelves",
                column: "InventoryLocationId",
                principalTable: "InventoryLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shelf_InventoryLocation",
                table: "Shelves");

            migrationBuilder.DropIndex(
                name: "IX_Shelves_InventoryLocationId",
                table: "Shelves");

            migrationBuilder.DropColumn(
                name: "InventoryLocationId",
                table: "Shelves");

            migrationBuilder.AddColumn<Guid>(
                name: "PartnerId",
                table: "Shelves",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Shelves",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shelves_PartnerId",
                table: "Shelves",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Shelves_StoreId",
                table: "Shelves",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shelf_Partner",
                table: "Shelves",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Shelf_Store",
                table: "Shelves",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");
        }
    }
}
