using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddfieldRejecteOfStoreOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "StoreOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectedByUserId",
                table: "StoreOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoreOrders_RejectedByUserId",
                table: "StoreOrders",
                column: "RejectedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreOrder_RejectedByUser",
                table: "StoreOrders",
                column: "RejectedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreOrder_RejectedByUser",
                table: "StoreOrders");

            migrationBuilder.DropIndex(
                name: "IX_StoreOrders_RejectedByUserId",
                table: "StoreOrders");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "StoreOrders");

            migrationBuilder.DropColumn(
                name: "RejectedByUserId",
                table: "StoreOrders");
        }
    }
}
