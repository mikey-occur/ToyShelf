using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMinorInfoEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "FromLocationId",
                table: "Shipments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShipperId",
                table: "Shipments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoreOrderId",
                table: "Shipments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShipperId",
                table: "Shipments",
                column: "ShipperId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_StoreOrderId",
                table: "Shipments",
                column: "StoreOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipment_StoreOrder",
                table: "Shipments",
                column: "StoreOrderId",
                principalTable: "StoreOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipment_User",
                table: "Shipments",
                column: "ShipperId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipment_StoreOrder",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipment_User",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_ShipperId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_StoreOrderId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShippedAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShipperId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "StoreOrderId",
                table: "Shipments");

            migrationBuilder.AlterColumn<Guid>(
                name: "FromLocationId",
                table: "Shipments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }
    }
}
