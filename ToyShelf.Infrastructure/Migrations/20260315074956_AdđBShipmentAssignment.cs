using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdđBShipmentAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipment_ApprovedByUser",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipment_User",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreOrder_User",
                table: "StoreOrders");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_ApprovedByUserId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_ShipperId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShippedAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShipperId",
                table: "Shipments");

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                table: "StoreOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentAssignmentId",
                table: "Shipments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ShipmentAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipperId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentAssignment_AssignedByUser",
                        column: x => x.AssignedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShipmentAssignment_Shipper",
                        column: x => x.ShipperId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShipmentAssignment_StoreOrder",
                        column: x => x.StoreOrderId,
                        principalTable: "StoreOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoreOrders_ApprovedByUserId",
                table: "StoreOrders",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShipmentAssignmentId",
                table: "Shipments",
                column: "ShipmentAssignmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentAssignments_AssignedByUserId",
                table: "ShipmentAssignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentAssignments_ShipperId",
                table: "ShipmentAssignments",
                column: "ShipperId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentAssignments_StoreOrderId",
                table: "ShipmentAssignments",
                column: "StoreOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_ShipmentAssignments_ShipmentAssignmentId",
                table: "Shipments",
                column: "ShipmentAssignmentId",
                principalTable: "ShipmentAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreOrder_ApprovedByUser",
                table: "StoreOrders",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreOrder_RequestedByUser",
                table: "StoreOrders",
                column: "RequestedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_ShipmentAssignments_ShipmentAssignmentId",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreOrder_ApprovedByUser",
                table: "StoreOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreOrder_RequestedByUser",
                table: "StoreOrders");

            migrationBuilder.DropTable(
                name: "ShipmentAssignments");

            migrationBuilder.DropIndex(
                name: "IX_StoreOrders_ApprovedByUserId",
                table: "StoreOrders");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_ShipmentAssignmentId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "StoreOrders");

            migrationBuilder.DropColumn(
                name: "ShipmentAssignmentId",
                table: "Shipments");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                table: "Shipments",
                type: "uuid",
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

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ApprovedByUserId",
                table: "Shipments",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShipperId",
                table: "Shipments",
                column: "ShipperId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipment_ApprovedByUser",
                table: "Shipments",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipment_User",
                table: "Shipments",
                column: "ShipperId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreOrder_User",
                table: "StoreOrders",
                column: "RequestedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
