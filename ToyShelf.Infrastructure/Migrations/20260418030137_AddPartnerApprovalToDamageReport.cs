using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerApprovalToDamageReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PartnerAdminApprovedAt",
                table: "DamageReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PartnerAdminApprovedByUserId",
                table: "DamageReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DamageReports_PartnerAdminApprovedByUserId",
                table: "DamageReports",
                column: "PartnerAdminApprovedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DamageReport_PartnerAdminApprovedByUser",
                table: "DamageReports",
                column: "PartnerAdminApprovedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DamageReport_PartnerAdminApprovedByUser",
                table: "DamageReports");

            migrationBuilder.DropIndex(
                name: "IX_DamageReports_PartnerAdminApprovedByUserId",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "PartnerAdminApprovedAt",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "PartnerAdminApprovedByUserId",
                table: "DamageReports");
        }
    }
}
