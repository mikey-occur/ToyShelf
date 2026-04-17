using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerApprovalFlowToShelfOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PartnerAdminApprovedAt",
                table: "ShelfOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PartnerAdminApprovedByUserId",
                table: "ShelfOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShelfOrders_PartnerAdminApprovedByUserId",
                table: "ShelfOrders",
                column: "PartnerAdminApprovedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShelfOrder_PartnerAdminApprovedByUser",
                table: "ShelfOrders",
                column: "PartnerAdminApprovedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShelfOrder_PartnerAdminApprovedByUser",
                table: "ShelfOrders");

            migrationBuilder.DropIndex(
                name: "IX_ShelfOrders_PartnerAdminApprovedByUserId",
                table: "ShelfOrders");

            migrationBuilder.DropColumn(
                name: "PartnerAdminApprovedAt",
                table: "ShelfOrders");

            migrationBuilder.DropColumn(
                name: "PartnerAdminApprovedByUserId",
                table: "ShelfOrders");
        }
    }
}
