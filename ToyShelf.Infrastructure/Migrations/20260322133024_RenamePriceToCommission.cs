using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
	/// <inheritdoc />
	public partial class RenamePriceToCommission : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// === 1. ĐỔI TÊN BẢNG (Giữ nguyên data) ===
			migrationBuilder.RenameTable(name: "PriceTables", newName: "CommissionTables");
			migrationBuilder.RenameTable(name: "PriceTableApplies", newName: "CommissionTableApplies");
			migrationBuilder.RenameTable(name: "PriceItems", newName: "CommissionItems");

			// === 2. ĐỔI TÊN CỘT KHÓA NGOẠI ===
			migrationBuilder.RenameColumn(name: "PriceTableId", table: "CommissionTableApplies", newName: "CommissionTableId");
			migrationBuilder.RenameColumn(name: "PriceTableId", table: "CommissionItems", newName: "CommissionTableId");

			// === 3. XÓA KHÓA NGOẠI CŨ (Tên bảng lúc này đã là Commission...) ===
			migrationBuilder.DropForeignKey(name: "FK_PriceItem_PriceSegment", table: "CommissionItems");
			migrationBuilder.DropForeignKey(name: "FK_PriceItem_PriceTable", table: "CommissionItems");
			migrationBuilder.DropForeignKey(name: "FK_PriceTableApply_Partner", table: "CommissionTableApplies");
			migrationBuilder.DropForeignKey(name: "FK_PriceTableApply_PriceTable", table: "CommissionTableApplies");
			migrationBuilder.DropForeignKey(name: "FK_PriceTable_PartnerTier", table: "CommissionTables");

			// === 4. THÊM KHÓA NGOẠI MỚI CHUẨN CHỈ ===
			migrationBuilder.AddForeignKey(
				name: "FK_CommissionItem_CommissionTable",
				table: "CommissionItems",
				column: "CommissionTableId",
				principalTable: "CommissionTables",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_CommissionItem_PriceSegment",
				table: "CommissionItems",
				column: "PriceSegmentId",
				principalTable: "PriceSegments",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);

			migrationBuilder.AddForeignKey(
				name: "FK_CommissionTableApply_CommissionTable",
				table: "CommissionTableApplies",
				column: "CommissionTableId",
				principalTable: "CommissionTables",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_CommissionTableApply_Partner",
				table: "CommissionTableApplies",
				column: "PartnerId",
				principalTable: "Partners",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_CommissionTable_PartnerTier",
				table: "CommissionTables",
				column: "PartnerTierId",
				principalTable: "PartnerTiers",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			// === 1. XÓA KHÓA NGOẠI MỚI ===
			migrationBuilder.DropForeignKey(name: "FK_CommissionItem_CommissionTable", table: "CommissionItems");
			migrationBuilder.DropForeignKey(name: "FK_CommissionItem_PriceSegment", table: "CommissionItems");
			migrationBuilder.DropForeignKey(name: "FK_CommissionTableApply_CommissionTable", table: "CommissionTableApplies");
			migrationBuilder.DropForeignKey(name: "FK_CommissionTableApply_Partner", table: "CommissionTableApplies");
			migrationBuilder.DropForeignKey(name: "FK_CommissionTable_PartnerTier", table: "CommissionTables");

			// === 2. THÊM LẠI KHÓA NGOẠI CŨ ===
			migrationBuilder.AddForeignKey(
				name: "FK_PriceItem_PriceSegment",
				table: "CommissionItems",
				column: "PriceSegmentId",
				principalTable: "PriceSegments",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);

			migrationBuilder.AddForeignKey(
				name: "FK_PriceItem_PriceTable",
				table: "CommissionItems",
				column: "CommissionTableId",
				principalTable: "CommissionTables",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_PriceTableApply_Partner",
				table: "CommissionTableApplies",
				column: "PartnerId",
				principalTable: "Partners",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_PriceTableApply_PriceTable",
				table: "CommissionTableApplies",
				column: "CommissionTableId",
				principalTable: "CommissionTables",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_PriceTable_PartnerTier",
				table: "CommissionTables",
				column: "PartnerTierId",
				principalTable: "PartnerTiers",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);

			// === 3. TRẢ LẠI TÊN CỘT VỀ NHƯ CŨ ===
			migrationBuilder.RenameColumn(name: "CommissionTableId", table: "CommissionTableApplies", newName: "PriceTableId");
			migrationBuilder.RenameColumn(name: "CommissionTableId", table: "CommissionItems", newName: "PriceTableId");

			// === 4. TRẢ LẠI TÊN BẢNG VỀ NHƯ CŨ ===
			migrationBuilder.RenameTable(name: "CommissionItems", newName: "PriceItems");
			migrationBuilder.RenameTable(name: "CommissionTableApplies", newName: "PriceTableApplies");
			migrationBuilder.RenameTable(name: "CommissionTables", newName: "PriceTables");
		}
	}
}