using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorAssignmentToDamageReportRelationshipToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentAssignmentId",
                table: "DamageReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DamageReports_ShipmentAssignmentId",
                table: "DamageReports",
                column: "ShipmentAssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DamageReport_ShipmentAssignment",
                table: "DamageReports",
                column: "ShipmentAssignmentId",
                principalTable: "ShipmentAssignments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DamageReport_ShipmentAssignment",
                table: "DamageReports");

            migrationBuilder.DropIndex(
                name: "IX_DamageReports_ShipmentAssignmentId",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "ShipmentAssignmentId",
                table: "DamageReports");

            migrationBuilder.AddColumn<Guid>(
                name: "DamageReportId",
                table: "ShipmentAssignments",
                type: "uuid",
                nullable: true);

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
    }
}
