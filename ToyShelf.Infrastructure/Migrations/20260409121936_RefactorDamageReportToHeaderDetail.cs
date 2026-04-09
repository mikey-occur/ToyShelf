using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorDamageReportToHeaderDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DamageMedia_DamageReport",
                table: "DamageMedia");

            migrationBuilder.DropForeignKey(
                name: "FK_DamageReport_ProductColor",
                table: "DamageReports");

            migrationBuilder.DropForeignKey(
                name: "FK_DamageReport_Shelf",
                table: "DamageReports");

            migrationBuilder.DropIndex(
                name: "IX_DamageReports_ProductColorId",
                table: "DamageReports");

            migrationBuilder.DropIndex(
                name: "IX_DamageReports_ShelfId",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "ProductColorId",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "DamageReports");

            migrationBuilder.DropColumn(
                name: "ShelfId",
                table: "DamageReports");

            migrationBuilder.RenameColumn(
                name: "DamageReportId",
                table: "DamageMedia",
                newName: "DamageReportItemId");

            migrationBuilder.RenameIndex(
                name: "IX_DamageMedia_DamageReportId",
                table: "DamageMedia",
                newName: "IX_DamageMedia_DamageReportItemId");

            migrationBuilder.CreateTable(
                name: "DamageReportItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DamageReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    DamageItemType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProductColorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    ShelfId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamageReportItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DamageReportItem_DamageReport",
                        column: x => x.DamageReportId,
                        principalTable: "DamageReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DamageReportItem_ProductColor",
                        column: x => x.ProductColorId,
                        principalTable: "ProductColors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DamageReportItem_Shelf",
                        column: x => x.ShelfId,
                        principalTable: "Shelves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DamageReportItems_DamageReportId",
                table: "DamageReportItems",
                column: "DamageReportId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageReportItems_ProductColorId",
                table: "DamageReportItems",
                column: "ProductColorId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageReportItems_ShelfId",
                table: "DamageReportItems",
                column: "ShelfId");

            migrationBuilder.AddForeignKey(
                name: "FK_DamageMedia_DamageReportItem",
                table: "DamageMedia",
                column: "DamageReportItemId",
                principalTable: "DamageReportItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DamageMedia_DamageReportItem",
                table: "DamageMedia");

            migrationBuilder.DropTable(
                name: "DamageReportItems");

            migrationBuilder.RenameColumn(
                name: "DamageReportItemId",
                table: "DamageMedia",
                newName: "DamageReportId");

            migrationBuilder.RenameIndex(
                name: "IX_DamageMedia_DamageReportItemId",
                table: "DamageMedia",
                newName: "IX_DamageMedia_DamageReportId");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductColorId",
                table: "DamageReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "DamageReports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ShelfId",
                table: "DamageReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DamageReports_ProductColorId",
                table: "DamageReports",
                column: "ProductColorId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageReports_ShelfId",
                table: "DamageReports",
                column: "ShelfId");

            migrationBuilder.AddForeignKey(
                name: "FK_DamageMedia_DamageReport",
                table: "DamageMedia",
                column: "DamageReportId",
                principalTable: "DamageReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DamageReport_ProductColor",
                table: "DamageReports",
                column: "ProductColorId",
                principalTable: "ProductColors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DamageReport_Shelf",
                table: "DamageReports",
                column: "ShelfId",
                principalTable: "Shelves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
