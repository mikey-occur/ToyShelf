using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShipmentAssignmentAndAddShipper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_ShipmentAssignments_ShipmentAssignmentId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_ShipmentAssignmentId",
                table: "Shipments");

            migrationBuilder.AddColumn<Guid>(
                name: "ShipperId",
                table: "Shipments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShipmentAssignmentId",
                table: "Shipments",
                column: "ShipmentAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShipperId",
                table: "Shipments",
                column: "ShipperId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipment_Assignment",
                table: "Shipments",
                column: "ShipmentAssignmentId",
                principalTable: "ShipmentAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipment_Shipper",
                table: "Shipments",
                column: "ShipperId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipment_Assignment",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipment_Shipper",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_ShipmentAssignmentId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_ShipperId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShipperId",
                table: "Shipments");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShipmentAssignmentId",
                table: "Shipments",
                column: "ShipmentAssignmentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_ShipmentAssignments_ShipmentAssignmentId",
                table: "Shipments",
                column: "ShipmentAssignmentId",
                principalTable: "ShipmentAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
