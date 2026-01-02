using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyCabin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cabin_Store",
                table: "Cabins");

            migrationBuilder.AddColumn<Guid>(
                name: "PartnerId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PartnerId",
                table: "Stores",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "STANDARD"),
                    RevenueSharePercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, comment: "Revenue share percentage from 0 to 100"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserStores",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreRole = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStores", x => new { x.UserId, x.StoreId });
                    table.ForeignKey(
                        name: "FK_UserStore_Store",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserStore_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_PartnerId",
                table: "Users",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_Code",
                table: "Stores",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_PartnerId",
                table: "Stores",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStores_StoreId",
                table: "UserStores",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cabin_Store",
                table: "Cabins",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Store_Partner",
                table: "Stores",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Partner",
                table: "Users",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cabin_Store",
                table: "Cabins");

            migrationBuilder.DropForeignKey(
                name: "FK_Store_Partner",
                table: "Stores");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Partner",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Partners");

            migrationBuilder.DropTable(
                name: "UserStores");

            migrationBuilder.DropIndex(
                name: "IX_Users_PartnerId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Stores_Code",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_PartnerId",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "Stores");

            migrationBuilder.AddForeignKey(
                name: "FK_Cabin_Store",
                table: "Cabins",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");
        }
    }
}
