using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShipmentDamageReportToOneToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipment_DamageReport",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_DamageReportId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DamageReportId",
                table: "Shipments");

            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentId",
                table: "DamageReports",
                type: "uuid",
                nullable: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DamageReport_Shipment",
                table: "DamageReports");

            migrationBuilder.DropIndex(
                name: "IX_DamageReports_ShipmentId",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "ShipmentId",
                table: "DamageReports");

            migrationBuilder.AddColumn<Guid>(
                name: "DamageReportId",
                table: "Shipments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_DamageReportId",
                table: "Shipments",
                column: "DamageReportId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipment_DamageReport",
                table: "Shipments",
                column: "DamageReportId",
                principalTable: "DamageReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
