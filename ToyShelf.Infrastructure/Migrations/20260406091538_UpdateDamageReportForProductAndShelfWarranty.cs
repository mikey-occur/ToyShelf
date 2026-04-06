using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDamageReportForProductAndShelfWarranty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ProductColorId",
                table: "DamageReports",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "AdminNote",
                table: "DamageReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "DamageReports",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsWarrantyClaim",
                table: "DamageReports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ReturnShipmentId",
                table: "DamageReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShelfId",
                table: "DamageReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "DamageReports",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "DamageReports",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyExpirationDate",
                table: "DamageReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DamageReports_ReturnShipmentId",
                table: "DamageReports",
                column: "ReturnShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageReports_ShelfId",
                table: "DamageReports",
                column: "ShelfId");

            migrationBuilder.AddForeignKey(
                name: "FK_DamageReport_Shelf",
                table: "DamageReports",
                column: "ShelfId",
                principalTable: "Shelves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DamageReport_Shipment",
                table: "DamageReports",
                column: "ReturnShipmentId",
                principalTable: "Shipments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DamageReport_Shelf",
                table: "DamageReports");

            migrationBuilder.DropForeignKey(
                name: "FK_DamageReport_Shipment",
                table: "DamageReports");

            migrationBuilder.DropIndex(
                name: "IX_DamageReports_ReturnShipmentId",
                table: "DamageReports");

            migrationBuilder.DropIndex(
                name: "IX_DamageReports_ShelfId",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "AdminNote",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "IsWarrantyClaim",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "ReturnShipmentId",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "ShelfId",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "WarrantyExpirationDate",
                table: "DamageReports");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductColorId",
                table: "DamageReports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
