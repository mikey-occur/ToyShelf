using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentItemAllocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssignmentShelfOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentShelfOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShelfOrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocatedQuantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentShelfOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentShelfOrderItems_AssignmentShelfOrders_AssignmentS~",
                        column: x => x.AssignmentShelfOrderId,
                        principalTable: "AssignmentShelfOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentShelfOrderItems_ShelfOrderItems_ShelfOrderItemId",
                        column: x => x.ShelfOrderItemId,
                        principalTable: "ShelfOrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentStoreOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentStoreOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreOrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocatedQuantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentStoreOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentStoreOrderItems_AssignmentStoreOrders_AssignmentS~",
                        column: x => x.AssignmentStoreOrderId,
                        principalTable: "AssignmentStoreOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentStoreOrderItems_StoreOrderItems_StoreOrderItemId",
                        column: x => x.StoreOrderItemId,
                        principalTable: "StoreOrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentShelfOrderItems_AssignmentShelfOrderId_ShelfOrder~",
                table: "AssignmentShelfOrderItems",
                columns: new[] { "AssignmentShelfOrderId", "ShelfOrderItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentShelfOrderItems_ShelfOrderItemId",
                table: "AssignmentShelfOrderItems",
                column: "ShelfOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentStoreOrderItems_AssignmentStoreOrderId_StoreOrder~",
                table: "AssignmentStoreOrderItems",
                columns: new[] { "AssignmentStoreOrderId", "StoreOrderItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentStoreOrderItems_StoreOrderItemId",
                table: "AssignmentStoreOrderItems",
                column: "StoreOrderItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentShelfOrderItems");

            migrationBuilder.DropTable(
                name: "AssignmentStoreOrderItems");
        }
    }
}
