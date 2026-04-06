using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeToShipmentAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DamageReportId",
                table: "ShipmentAssignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ShipmentAssignments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentAssignments_DamageReportId",
                table: "ShipmentAssignments",
                column: "DamageReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentAssignment_DamageReport",
                table: "ShipmentAssignments",
                column: "DamageReportId",
                principalTable: "DamageReports",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentAssignment_DamageReport",
                table: "ShipmentAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentAssignments_DamageReportId",
                table: "ShipmentAssignments");

            migrationBuilder.DropColumn(
                name: "DamageReportId",
                table: "ShipmentAssignments");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ShipmentAssignments");
        }
    }
}
