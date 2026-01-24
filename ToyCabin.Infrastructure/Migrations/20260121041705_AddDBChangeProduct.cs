using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyCabin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDBChangeProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Model3DUrl",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "QrCode",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ProductColors",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model3DUrl",
                table: "ProductColors",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrCode",
                table: "ProductColors",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ProductColors");

            migrationBuilder.DropColumn(
                name: "Model3DUrl",
                table: "ProductColors");

            migrationBuilder.DropColumn(
                name: "QrCode",
                table: "ProductColors");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model3DUrl",
                table: "Products",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrCode",
                table: "Products",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
