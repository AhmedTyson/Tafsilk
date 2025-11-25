using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TafsilkPlatform.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTailorVerificationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TailorVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TailorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NationalIdNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullLegalName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CommercialRegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProfessionalLicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdDocumentFrontData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    IdDocumentFrontContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdDocumentBackData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    IdDocumentBackContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CommercialRegistrationData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CommercialRegistrationContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProfessionalLicenseData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ProfessionalLicenseContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByAdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TailorVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TailorVerifications_TailorProfiles_TailorProfileId",
                        column: x => x.TailorProfileId,
                        principalTable: "TailorProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TailorVerifications_Users_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TailorVerifications_ReviewedByAdminId",
                table: "TailorVerifications",
                column: "ReviewedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_TailorVerifications_Status",
                table: "TailorVerifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TailorVerifications_TailorProfileId",
                table: "TailorVerifications",
                column: "TailorProfileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TailorVerifications");
        }
    }
}
