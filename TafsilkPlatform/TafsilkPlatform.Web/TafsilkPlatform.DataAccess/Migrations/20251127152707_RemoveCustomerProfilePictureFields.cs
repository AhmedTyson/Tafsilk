using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TafsilkPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCustomerProfilePictureFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePictureContentType",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "ProfilePictureData",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "CustomerProfiles");

            migrationBuilder.Sql("UPDATE CustomerProfiles SET Gender = NULL WHERE Gender NOT IN ('Male', 'Female')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CustomerProfiles_Gender",
                table: "CustomerProfiles",
                sql: "[Gender] IS NULL OR [Gender] IN ('Male', 'Female')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CustomerProfiles_Gender",
                table: "CustomerProfiles");

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
                name: "ProfilePictureUrl",
                table: "CustomerProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
