using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnhancedShipmentTrackingTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReceivedAt",
                table: "Shipments",
                newName: "WarehouseReceivedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnPickedUpAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StoreReceivedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnPickedUpAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "StoreReceivedAt",
                table: "Shipments");

            migrationBuilder.RenameColumn(
                name: "WarehouseReceivedAt",
                table: "Shipments",
                newName: "ReceivedAt");
        }
    }
}
