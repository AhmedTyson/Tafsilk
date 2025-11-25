using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TafsilkPlatform.Web.Migrations
{
    /// <summary>
    /// Migration to add Stripe integration fields to Payment table
    /// Run: dotnet ef migrations add AddStripeFieldsToPayment
    /// </summary>
    public partial class AddStripeFieldsToPayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Add Stripe integration fields
            migrationBuilder.AddColumn<string>(
                name: "ProviderTransactionId",
                table: "Payment",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderCustomerId",
                table: "Payment",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "Payment",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "Internal");

            migrationBuilder.AddColumn<string>(
                name: "ProviderMetadata",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardLast4",
                table: "Payment",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardBrand",
                table: "Payment",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Payment",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "SAR");

            migrationBuilder.AddColumn<bool>(
                name: "ThreeDSecureUsed",
                table: "Payment",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "Payment",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundedAmount",
                table: "Payment",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RefundedAt",
                table: "Payment",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Payment",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Payment",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Payment",
                type: "datetimeoffset",
                nullable: true);

            // ✅ Create index on ProviderTransactionId for faster lookups
            migrationBuilder.CreateIndex(
                name: "IX_Payment_ProviderTransactionId",
                table: "Payment",
                column: "ProviderTransactionId");

            // ✅ Create index on CreatedAt for reporting
            migrationBuilder.CreateIndex(
                name: "IX_Payment_CreatedAt",
                table: "Payment",
                column: "CreatedAt");

            // ✅ Create index on PaymentStatus for filtering
            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentStatus",
                table: "Payment",
                column: "PaymentStatus");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ Remove indexes
            migrationBuilder.DropIndex(
                name: "IX_Payment_ProviderTransactionId",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Payment_CreatedAt",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Payment_PaymentStatus",
                table: "Payment");

            // ✅ Remove columns
            migrationBuilder.DropColumn(name: "ProviderTransactionId", table: "Payment");
            migrationBuilder.DropColumn(name: "ProviderCustomerId", table: "Payment");
            migrationBuilder.DropColumn(name: "Provider", table: "Payment");
            migrationBuilder.DropColumn(name: "ProviderMetadata", table: "Payment");
            migrationBuilder.DropColumn(name: "CardLast4", table: "Payment");
            migrationBuilder.DropColumn(name: "CardBrand", table: "Payment");
            migrationBuilder.DropColumn(name: "Currency", table: "Payment");
            migrationBuilder.DropColumn(name: "ThreeDSecureUsed", table: "Payment");
            migrationBuilder.DropColumn(name: "FailureReason", table: "Payment");
            migrationBuilder.DropColumn(name: "RefundedAmount", table: "Payment");
            migrationBuilder.DropColumn(name: "RefundedAt", table: "Payment");
            migrationBuilder.DropColumn(name: "Notes", table: "Payment");
            migrationBuilder.DropColumn(name: "CreatedAt", table: "Payment");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "Payment");
        }
    }
}
