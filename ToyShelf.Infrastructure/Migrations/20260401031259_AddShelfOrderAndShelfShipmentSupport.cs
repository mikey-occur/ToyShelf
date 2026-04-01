using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShelfOrderAndShelfShipmentSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentAssignment_StoreOrder",
                table: "ShipmentAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipment_StoreOrder",
                table: "Shipments");

            migrationBuilder.AlterColumn<Guid>(
                name: "StoreOrderId",
                table: "Shipments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ShelfOrderId",
                table: "Shipments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StoreOrderId",
                table: "ShipmentAssignments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ShelfOrderId",
                table: "ShipmentAssignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShelfOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StoreLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AdminNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShelfOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShelfOrder_ApprovedByUser",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShelfOrder_RejectedByUser",
                        column: x => x.RejectedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShelfOrder_RequestedByUser",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShelfOrder_StoreLocation",
                        column: x => x.StoreLocationId,
                        principalTable: "InventoryLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShelfShipmentItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShelfTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpectedQuantity = table.Column<int>(type: "integer", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShelfShipmentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShelfShipmentItem_ShelfType",
                        column: x => x.ShelfTypeId,
                        principalTable: "ShelfTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShelfShipmentItem_Shipment",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShelfOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShelfOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShelfTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    FulfilledQuantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShelfTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShelfOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShelfOrderItem_ShelfOrder",
                        column: x => x.ShelfOrderId,
                        principalTable: "ShelfOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShelfOrderItem_ShelfType",
                        column: x => x.ShelfTypeId,
                        principalTable: "ShelfTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShelfOrderId",
                table: "Shipments",
                column: "ShelfOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentAssignments_ShelfOrderId",
                table: "ShipmentAssignments",
                column: "ShelfOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ShelfOrderItems_ShelfOrderId",
                table: "ShelfOrderItems",
                column: "ShelfOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ShelfOrderItems_ShelfTypeId",
                table: "ShelfOrderItems",
                column: "ShelfTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ShelfOrders_ApprovedByUserId",
                table: "ShelfOrders",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShelfOrders_Code",
                table: "ShelfOrders",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShelfOrders_RejectedByUserId",
                table: "ShelfOrders",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShelfOrders_RequestedByUserId",
                table: "ShelfOrders",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShelfOrders_StoreLocationId",
                table: "ShelfOrders",
                column: "StoreLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShelfShipmentItems_ShelfTypeId",
                table: "ShelfShipmentItems",
                column: "ShelfTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ShelfShipmentItems_ShipmentId",
                table: "ShelfShipmentItems",
                column: "ShipmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentAssignment_ShelfOrder",
                table: "ShipmentAssignments",
                column: "ShelfOrderId",
                principalTable: "ShelfOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentAssignment_StoreOrder",
                table: "ShipmentAssignments",
                column: "StoreOrderId",
                principalTable: "StoreOrders",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentAssignment_ShelfOrder",
                table: "ShipmentAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentAssignment_StoreOrder",
                table: "ShipmentAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipment_ShelfOrder",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipment_StoreOrder",
                table: "Shipments");

            migrationBuilder.DropTable(
                name: "ShelfOrderItems");

            migrationBuilder.DropTable(
                name: "ShelfShipmentItems");

            migrationBuilder.DropTable(
                name: "ShelfOrders");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_ShelfOrderId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentAssignments_ShelfOrderId",
                table: "ShipmentAssignments");

            migrationBuilder.DropColumn(
                name: "ShelfOrderId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShelfOrderId",
                table: "ShipmentAssignments");

            migrationBuilder.AlterColumn<Guid>(
                name: "StoreOrderId",
                table: "Shipments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StoreOrderId",
                table: "ShipmentAssignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentAssignment_StoreOrder",
                table: "ShipmentAssignments",
                column: "StoreOrderId",
                principalTable: "StoreOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipment_StoreOrder",
                table: "Shipments",
                column: "StoreOrderId",
                principalTable: "StoreOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
