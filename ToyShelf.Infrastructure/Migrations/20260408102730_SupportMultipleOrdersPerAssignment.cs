using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SupportMultipleOrdersPerAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentAssignment_ShelfOrder",
                table: "ShipmentAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentAssignment_StoreOrder",
                table: "ShipmentAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentAssignments_ShelfOrderId",
                table: "ShipmentAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentAssignments_StoreOrderId",
                table: "ShipmentAssignments");

            migrationBuilder.DropColumn(
                name: "ShelfOrderId",
                table: "ShipmentAssignments");

            migrationBuilder.DropColumn(
                name: "StoreOrderId",
                table: "ShipmentAssignments");

            migrationBuilder.CreateTable(
                name: "AssignmentShelfOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShelfOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentAssignmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentShelfOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentShelfOrders_ShelfOrders_ShelfOrderId",
                        column: x => x.ShelfOrderId,
                        principalTable: "ShelfOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentShelfOrders_ShipmentAssignments_ShipmentAssignmen~",
                        column: x => x.ShipmentAssignmentId,
                        principalTable: "ShipmentAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentStoreOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentAssignmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentStoreOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentStoreOrders_ShipmentAssignments_ShipmentAssignmen~",
                        column: x => x.ShipmentAssignmentId,
                        principalTable: "ShipmentAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentStoreOrders_StoreOrders_StoreOrderId",
                        column: x => x.StoreOrderId,
                        principalTable: "StoreOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentShelfOrders_ShelfOrderId",
                table: "AssignmentShelfOrders",
                column: "ShelfOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentShelfOrders_ShipmentAssignmentId_ShelfOrderId",
                table: "AssignmentShelfOrders",
                columns: new[] { "ShipmentAssignmentId", "ShelfOrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentStoreOrders_ShipmentAssignmentId_StoreOrderId",
                table: "AssignmentStoreOrders",
                columns: new[] { "ShipmentAssignmentId", "StoreOrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentStoreOrders_StoreOrderId",
                table: "AssignmentStoreOrders",
                column: "StoreOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentShelfOrders");

            migrationBuilder.DropTable(
                name: "AssignmentStoreOrders");

            migrationBuilder.AddColumn<Guid>(
                name: "ShelfOrderId",
                table: "ShipmentAssignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoreOrderId",
                table: "ShipmentAssignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentAssignments_ShelfOrderId",
                table: "ShipmentAssignments",
                column: "ShelfOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentAssignments_StoreOrderId",
                table: "ShipmentAssignments",
                column: "StoreOrderId");

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
        }
    }
}
