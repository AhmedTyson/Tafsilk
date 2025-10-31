using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TafsilkPlatform.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeAndShadowFks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioImages_TailorProfiles_TailorId1",
                table: "PortfolioImages");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioImages_TailorId1",
                table: "PortfolioImages");

            migrationBuilder.DropColumn(
                name: "TailorId1",
                table: "PortfolioImages");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Reviews",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "RevenueReports",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "PortfolioImages",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Reviews",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "RevenueReports",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "PortfolioImages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TailorId1",
                table: "PortfolioImages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioImages_TailorId1",
                table: "PortfolioImages",
                column: "TailorId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioImages_TailorProfiles_TailorId1",
                table: "PortfolioImages",
                column: "TailorId1",
                principalTable: "TailorProfiles",
                principalColumn: "Id");
        }
    }
}
