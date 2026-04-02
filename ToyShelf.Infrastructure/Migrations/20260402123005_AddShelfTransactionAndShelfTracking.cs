using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShelfTransactionAndShelfTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShelfTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShelfId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ToStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReferenceType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShelfTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShelfTransaction_FromLocation",
                        column: x => x.FromLocationId,
                        principalTable: "InventoryLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShelfTransaction_Shelf",
                        column: x => x.ShelfId,
                        principalTable: "Shelves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShelfTransaction_ToLocation",
                        column: x => x.ToLocationId,
                        principalTable: "InventoryLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShelfTransactions_FromLocationId",
                table: "ShelfTransactions",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShelfTransactions_ShelfId",
                table: "ShelfTransactions",
                column: "ShelfId");

            migrationBuilder.CreateIndex(
                name: "IX_ShelfTransactions_ToLocationId",
                table: "ShelfTransactions",
                column: "ToLocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShelfTransactions");
        }
    }
}
