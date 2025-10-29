using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TafsilkPlatform.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePictureBinaryData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureContentType",
                table: "TailorProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ProfilePictureData",
                table: "TailorProfiles",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureContentType",
                table: "CustomerProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ProfilePictureData",
                table: "CustomerProfiles",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureContentType",
                table: "CorporateAccounts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ProfilePictureData",
                table: "CorporateAccounts",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePictureContentType",
                table: "TailorProfiles");

            migrationBuilder.DropColumn(
                name: "ProfilePictureData",
                table: "TailorProfiles");

            migrationBuilder.DropColumn(
                name: "ProfilePictureContentType",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "ProfilePictureData",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "ProfilePictureContentType",
                table: "CorporateAccounts");

            migrationBuilder.DropColumn(
                name: "ProfilePictureData",
                table: "CorporateAccounts");
        }
    }
}
