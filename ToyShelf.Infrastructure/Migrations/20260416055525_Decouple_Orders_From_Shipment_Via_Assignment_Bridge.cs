using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Decouple_Orders_From_Shipment_Via_Assignment_Bridge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DamageReport_Shipment",
                table: "DamageReports");

            migrationBuilder.DropForeignKey(
                name: "FK_ShelfOrder_Shipment",
                table: "ShelfOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreOrder_Shipment",
                table: "StoreOrders");

            migrationBuilder.DropIndex(
                name: "IX_StoreOrders_ShipmentId",
                table: "StoreOrders");

            migrationBuilder.DropIndex(
                name: "IX_ShelfOrders_ShipmentId",
                table: "ShelfOrders");

            migrationBuilder.DropIndex(
                name: "IX_DamageReports_ShipmentId",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "ShipmentId",
                table: "StoreOrders");

            migrationBuilder.DropColumn(
                name: "ShipmentId",
                table: "ShelfOrders");

            migrationBuilder.DropColumn(
                name: "ShipmentId",
                table: "DamageReports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentId",
                table: "StoreOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentId",
                table: "ShelfOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentId",
                table: "DamageReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoreOrders_ShipmentId",
                table: "StoreOrders",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShelfOrders_ShipmentId",
                table: "ShelfOrders",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageReports_ShipmentId",
                table: "DamageReports",
                column: "ShipmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DamageReport_Shipment",
                table: "DamageReports",
                column: "ShipmentId",
                principalTable: "Shipments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShelfOrder_Shipment",
                table: "ShelfOrders",
                column: "ShipmentId",
                principalTable: "Shipments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreOrder_Shipment",
                table: "StoreOrders",
                column: "ShipmentId",
                principalTable: "Shipments",
                principalColumn: "Id");
        }
    }
}
