using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerEmailToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomerEmail",
                table: "Orders",
                newName: "CustomerEmail");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_UserPhone",
                table: "Orders",
                newName: "IX_Orders_UserEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomerEmail",
                table: "Orders",
                newName: "CustomerEmail");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_UserEmail",
                table: "Orders",
                newName: "IX_Orders_UserPhone");
        }
    }
}
