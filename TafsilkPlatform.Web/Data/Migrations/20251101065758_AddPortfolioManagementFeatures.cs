using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TafsilkPlatform.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioManagementFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "PortfolioImages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "PortfolioImages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PortfolioImages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PortfolioImages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "PortfolioImages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedPrice",
                table: "PortfolioImages",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "PortfolioImages",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "PortfolioImages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "PortfolioImages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "PortfolioImages");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "PortfolioImages");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PortfolioImages");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PortfolioImages");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "PortfolioImages");

            migrationBuilder.DropColumn(
                name: "EstimatedPrice",
                table: "PortfolioImages");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "PortfolioImages");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "PortfolioImages");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "PortfolioImages");
        }
    }
}
