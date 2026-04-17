using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerAdminApprovalToStoreOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PartnerAdminApprovedAt",
                table: "StoreOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PartnerAdminApprovedByUserId",
                table: "StoreOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoreOrders_PartnerAdminApprovedByUserId",
                table: "StoreOrders",
                column: "PartnerAdminApprovedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreOrder_PartnerApprovedByUser",
                table: "StoreOrders",
                column: "PartnerAdminApprovedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreOrder_PartnerApprovedByUser",
                table: "StoreOrders");

            migrationBuilder.DropIndex(
                name: "IX_StoreOrders_PartnerAdminApprovedByUserId",
                table: "StoreOrders");

            migrationBuilder.DropColumn(
                name: "PartnerAdminApprovedAt",
                table: "StoreOrders");

            migrationBuilder.DropColumn(
                name: "PartnerAdminApprovedByUserId",
                table: "StoreOrders");
        }
    }
}
