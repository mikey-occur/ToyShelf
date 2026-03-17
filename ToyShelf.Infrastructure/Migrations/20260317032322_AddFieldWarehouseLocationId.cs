using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldWarehouseLocationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseLocationId",
                table: "ShipmentAssignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentAssignments_WarehouseLocationId",
                table: "ShipmentAssignments",
                column: "WarehouseLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentAssignment_InventoryLocation",
                table: "ShipmentAssignments",
                column: "WarehouseLocationId",
                principalTable: "InventoryLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentAssignment_InventoryLocation",
                table: "ShipmentAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentAssignments_WarehouseLocationId",
                table: "ShipmentAssignments");

            migrationBuilder.DropColumn(
                name: "WarehouseLocationId",
                table: "ShipmentAssignments");
        }
    }
}
