using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Delete3dModelUrlAddIndexBarCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Model3DUrl",
                table: "ProductColors");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Product_Barcode",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "Model3DUrl",
                table: "ProductColors",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
