using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteDbCommissionPolicyAndPriceSegment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductColor_PriceSegment",
                table: "ProductColors");

            migrationBuilder.DropTable(
                name: "CommissionPolicies");

            migrationBuilder.DropTable(
                name: "PriceSegments");

            migrationBuilder.DropIndex(
                name: "IX_ProductColors_PriceSegmentId",
                table: "ProductColors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PriceSegments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MaxPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MinPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceSegments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommissionPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerTierId = table.Column<Guid>(type: "uuid", nullable: false),
                    PriceSegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommissionRate = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionPolicy_PartnerTier",
                        column: x => x.PartnerTierId,
                        principalTable: "PartnerTiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommissionPolicy_PriceSegment",
                        column: x => x.PriceSegmentId,
                        principalTable: "PriceSegments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductColors_PriceSegmentId",
                table: "ProductColors",
                column: "PriceSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionPolicies_PartnerTierId_PriceSegmentId",
                table: "CommissionPolicies",
                columns: new[] { "PartnerTierId", "PriceSegmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissionPolicies_PriceSegmentId",
                table: "CommissionPolicies",
                column: "PriceSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceSegments_Code",
                table: "PriceSegments",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceSegments_MinPrice_MaxPrice",
                table: "PriceSegments",
                columns: new[] { "MinPrice", "MaxPrice" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductColor_PriceSegment",
                table: "ProductColors",
                column: "PriceSegmentId",
                principalTable: "PriceSegments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
