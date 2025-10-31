using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TafsilkPlatform.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixShadowFKs_ModelCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderImages_Orders_OrderId1",
                table: "OrderImages");

            migrationBuilder.DropIndex(
                name: "IX_OrderImages_OrderId1",
                table: "OrderImages");

            migrationBuilder.DropColumn(
                name: "OrderId1",
                table: "OrderImages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrderId1",
                table: "OrderImages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_OrderImages_OrderId1",
                table: "OrderImages",
                column: "OrderId1");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderImages_Orders_OrderId1",
                table: "OrderImages",
                column: "OrderId1",
                principalTable: "Orders",
                principalColumn: "OrderId");
        }
    }
}
