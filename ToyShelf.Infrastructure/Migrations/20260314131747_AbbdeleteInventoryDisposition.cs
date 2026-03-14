using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AbbdeleteInventoryDisposition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventory_Disposition",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransaction_FromDisposition",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransaction_ToDisposition",
                table: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "InventoryDispositions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_FromDispositionId",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_ToDispositionId",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_DispositionId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_InventoryLocationId_ProductColorId_DispositionId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "FromDispositionId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "ToDispositionId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "DispositionId",
                table: "Inventories");

            migrationBuilder.AddColumn<string>(
                name: "FromStatus",
                table: "InventoryTransactions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToStatus",
                table: "InventoryTransactions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Inventories",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_InventoryLocationId_ProductColorId_Status",
                table: "Inventories",
                columns: new[] { "InventoryLocationId", "ProductColorId", "Status" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventories_InventoryLocationId_ProductColorId_Status",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "FromStatus",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "ToStatus",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Inventories");

            migrationBuilder.AddColumn<Guid>(
                name: "FromDispositionId",
                table: "InventoryTransactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ToDispositionId",
                table: "InventoryTransactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DispositionId",
                table: "Inventories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "InventoryDispositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryDispositions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_FromDispositionId",
                table: "InventoryTransactions",
                column: "FromDispositionId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ToDispositionId",
                table: "InventoryTransactions",
                column: "ToDispositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_DispositionId",
                table: "Inventories",
                column: "DispositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_InventoryLocationId_ProductColorId_DispositionId",
                table: "Inventories",
                columns: new[] { "InventoryLocationId", "ProductColorId", "DispositionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDispositions_Code",
                table: "InventoryDispositions",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_Disposition",
                table: "Inventories",
                column: "DispositionId",
                principalTable: "InventoryDispositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransaction_FromDisposition",
                table: "InventoryTransactions",
                column: "FromDispositionId",
                principalTable: "InventoryDispositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransaction_ToDisposition",
                table: "InventoryTransactions",
                column: "ToDispositionId",
                principalTable: "InventoryDispositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
