using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToyShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCityIdToStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CityId",
                table: "StoreCreationRequests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_StoreCreationRequests_CityId",
                table: "StoreCreationRequests",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreCreationRequest_City",
                table: "StoreCreationRequests",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreCreationRequest_City",
                table: "StoreCreationRequests");

            migrationBuilder.DropIndex(
                name: "IX_StoreCreationRequests_CityId",
                table: "StoreCreationRequests");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "StoreCreationRequests");
        }
    }
}
