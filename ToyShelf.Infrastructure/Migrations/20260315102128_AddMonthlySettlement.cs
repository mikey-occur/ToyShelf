using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlySettlement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaidOut",
                table: "CommissionHistories");

            migrationBuilder.AddColumn<Guid>(
                name: "MonthlySettlementId",
                table: "CommissionHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MonthlySettlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    TotalItems = table.Column<int>(type: "integer", nullable: false),
                    TotalCommissionAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlySettlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlySettlement_Partner",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommissionHistories_MonthlySettlementId",
                table: "CommissionHistories",
                column: "MonthlySettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlySettlements_PartnerId",
                table: "MonthlySettlements",
                column: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommissionHistory_MonthlySettlement",
                table: "CommissionHistories",
                column: "MonthlySettlementId",
                principalTable: "MonthlySettlements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommissionHistory_MonthlySettlement",
                table: "CommissionHistories");

            migrationBuilder.DropTable(
                name: "MonthlySettlements");

            migrationBuilder.DropIndex(
                name: "IX_CommissionHistories_MonthlySettlementId",
                table: "CommissionHistories");

            migrationBuilder.DropColumn(
                name: "MonthlySettlementId",
                table: "CommissionHistories");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaidOut",
                table: "CommissionHistories",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
