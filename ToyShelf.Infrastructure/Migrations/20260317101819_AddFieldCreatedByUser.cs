using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldCreatedByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentAssignment_AssignedByUser",
                table: "ShipmentAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentAssignment_Shipper",
                table: "ShipmentAssignments");

            migrationBuilder.AlterColumn<Guid>(
                name: "ShipperId",
                table: "ShipmentAssignments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedByUserId",
                table: "ShipmentAssignments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ShipmentAssignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentAssignments_CreatedByUserId",
                table: "ShipmentAssignments",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentAssignment_AssignedByUser",
                table: "ShipmentAssignments",
                column: "AssignedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentAssignment_CreatedByUser",
                table: "ShipmentAssignments",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentAssignment_Shipper",
                table: "ShipmentAssignments",
                column: "ShipperId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentAssignment_AssignedByUser",
                table: "ShipmentAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentAssignment_CreatedByUser",
                table: "ShipmentAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentAssignment_Shipper",
                table: "ShipmentAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentAssignments_CreatedByUserId",
                table: "ShipmentAssignments");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ShipmentAssignments");

            migrationBuilder.AlterColumn<Guid>(
                name: "ShipperId",
                table: "ShipmentAssignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedByUserId",
                table: "ShipmentAssignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentAssignment_AssignedByUser",
                table: "ShipmentAssignments",
                column: "AssignedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentAssignment_Shipper",
                table: "ShipmentAssignments",
                column: "ShipperId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
