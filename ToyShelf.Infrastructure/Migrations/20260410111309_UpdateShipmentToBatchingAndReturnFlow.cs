using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShipmentToBatchingAndReturnFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DamageReport_ShipmentAssignment",
                table: "DamageReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipment_ShelfOrder",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipment_StoreOrder",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_ShelfOrderId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_StoreOrderId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_DamageReports_ShipmentAssignmentId",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "ShelfOrderId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "StoreOrderId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShipmentAssignmentId",
                table: "DamageReports");

            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentId",
                table: "StoreOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReturn",
                table: "Shipments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductColorId",
                table: "ShipmentItems",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "DamageReportItemId",
                table: "ShipmentItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShelfId",
                table: "ShipmentItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoreOrderItemId",
                table: "ShipmentItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShelfOrderItemId",
                table: "ShelfShipmentItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentId",
                table: "ShelfOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssignmentDamageReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DamageReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentAssignmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentDamageReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentDamageReports_DamageReports_DamageReportId",
                        column: x => x.DamageReportId,
                        principalTable: "DamageReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentDamageReports_ShipmentAssignments_ShipmentAssignm~",
                        column: x => x.ShipmentAssignmentId,
                        principalTable: "ShipmentAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoreOrders_ShipmentId",
                table: "StoreOrders",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_DamageReportItemId",
                table: "ShipmentItems",
                column: "DamageReportItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_ShelfId",
                table: "ShipmentItems",
                column: "ShelfId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_StoreOrderItemId",
                table: "ShipmentItems",
                column: "StoreOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ShelfShipmentItems_ShelfOrderItemId",
                table: "ShelfShipmentItems",
                column: "ShelfOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ShelfOrders_ShipmentId",
                table: "ShelfOrders",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentDamageReports_DamageReportId",
                table: "AssignmentDamageReports",
                column: "DamageReportId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentDamageReports_ShipmentAssignmentId_DamageReportId",
                table: "AssignmentDamageReports",
                columns: new[] { "ShipmentAssignmentId", "DamageReportId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ShelfOrder_Shipment",
                table: "ShelfOrders",
                column: "ShipmentId",
                principalTable: "Shipments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShelfShipmentItem_ShelfOrderItem",
                table: "ShelfShipmentItems",
                column: "ShelfOrderItemId",
                principalTable: "ShelfOrderItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentItem_DamageReportItem",
                table: "ShipmentItems",
                column: "DamageReportItemId",
                principalTable: "DamageReportItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentItem_Shelf",
                table: "ShipmentItems",
                column: "ShelfId",
                principalTable: "Shelves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentItem_StoreOrderItem",
                table: "ShipmentItems",
                column: "StoreOrderItemId",
                principalTable: "StoreOrderItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreOrder_Shipment",
                table: "StoreOrders",
                column: "ShipmentId",
                principalTable: "Shipments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShelfOrder_Shipment",
                table: "ShelfOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ShelfShipmentItem_ShelfOrderItem",
                table: "ShelfShipmentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentItem_DamageReportItem",
                table: "ShipmentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentItem_Shelf",
                table: "ShipmentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentItem_StoreOrderItem",
                table: "ShipmentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreOrder_Shipment",
                table: "StoreOrders");

            migrationBuilder.DropTable(
                name: "AssignmentDamageReports");

            migrationBuilder.DropIndex(
                name: "IX_StoreOrders_ShipmentId",
                table: "StoreOrders");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentItems_DamageReportItemId",
                table: "ShipmentItems");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentItems_ShelfId",
                table: "ShipmentItems");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentItems_StoreOrderItemId",
                table: "ShipmentItems");

            migrationBuilder.DropIndex(
                name: "IX_ShelfShipmentItems_ShelfOrderItemId",
                table: "ShelfShipmentItems");

            migrationBuilder.DropIndex(
                name: "IX_ShelfOrders_ShipmentId",
                table: "ShelfOrders");

            migrationBuilder.DropColumn(
                name: "ShipmentId",
                table: "StoreOrders");

            migrationBuilder.DropColumn(
                name: "IsReturn",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DamageReportItemId",
                table: "ShipmentItems");

            migrationBuilder.DropColumn(
                name: "ShelfId",
                table: "ShipmentItems");

            migrationBuilder.DropColumn(
                name: "StoreOrderItemId",
                table: "ShipmentItems");

            migrationBuilder.DropColumn(
                name: "ShelfOrderItemId",
                table: "ShelfShipmentItems");

            migrationBuilder.DropColumn(
                name: "ShipmentId",
                table: "ShelfOrders");

            migrationBuilder.AddColumn<Guid>(
                name: "ShelfOrderId",
                table: "Shipments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoreOrderId",
                table: "Shipments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductColorId",
                table: "ShipmentItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentAssignmentId",
                table: "DamageReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShelfOrderId",
                table: "Shipments",
                column: "ShelfOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_StoreOrderId",
                table: "Shipments",
                column: "StoreOrderId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Shipment_ShelfOrder",
                table: "Shipments",
                column: "ShelfOrderId",
                principalTable: "ShelfOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipment_StoreOrder",
                table: "Shipments",
                column: "StoreOrderId",
                principalTable: "StoreOrders",
                principalColumn: "Id");
        }
    }
}
