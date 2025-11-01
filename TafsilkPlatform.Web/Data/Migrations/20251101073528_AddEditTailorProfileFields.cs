using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TafsilkPlatform.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEditTailorProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AverageRating",
                table: "TailorProfiles",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BusinessHours",
                table: "TailorProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "TailorProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacebookUrl",
                table: "TailorProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramUrl",
                table: "TailorProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopDescription",
                table: "TailorProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Specialization",
                table: "TailorProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwitterUrl",
                table: "TailorProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "TailorProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "TailorProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "TailorProfiles");

            migrationBuilder.DropColumn(
                name: "BusinessHours",
                table: "TailorProfiles");

            migrationBuilder.DropColumn(
                name: "District",
                table: "TailorProfiles");

            migrationBuilder.DropColumn(
                name: "FacebookUrl",
                table: "TailorProfiles");

            migrationBuilder.DropColumn(
                name: "InstagramUrl",
                table: "TailorProfiles");

            migrationBuilder.DropColumn(
                name: "ShopDescription",
                table: "TailorProfiles");

            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "TailorProfiles");

            migrationBuilder.DropColumn(
                name: "TwitterUrl",
                table: "TailorProfiles");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "TailorProfiles");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "TailorProfiles");
        }
    }
}
