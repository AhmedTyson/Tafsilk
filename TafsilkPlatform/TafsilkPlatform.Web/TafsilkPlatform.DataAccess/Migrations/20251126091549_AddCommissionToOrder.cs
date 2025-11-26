using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TafsilkPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCommissionToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use raw SQL to check if columns exist before adding them (Idempotent)
            
            // 1. PrimaryImageUrl on Products
            migrationBuilder.Sql(@"
                IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'PrimaryImageUrl' AND Object_ID = Object_ID(N'Products'))
                BEGIN
                    ALTER TABLE Products ADD PrimaryImageUrl nvarchar(500) NULL;
                END
            ");

            // 2. CommissionAmount on Orders
            migrationBuilder.Sql(@"
                IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'CommissionAmount' AND Object_ID = Object_ID(N'Orders'))
                BEGIN
                    ALTER TABLE Orders ADD CommissionAmount float NOT NULL DEFAULT 0.0;
                END
            ");

            // 3. CommissionRate on Orders
            migrationBuilder.Sql(@"
                IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'CommissionRate' AND Object_ID = Object_ID(N'Orders'))
                BEGIN
                    ALTER TABLE Orders ADD CommissionRate float NOT NULL DEFAULT 0.0;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryImageUrl",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CommissionAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CommissionRate",
                table: "Orders");
        }
    }
}
