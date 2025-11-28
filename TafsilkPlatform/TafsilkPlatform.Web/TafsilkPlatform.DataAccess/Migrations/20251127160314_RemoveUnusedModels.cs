using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TafsilkPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplaintAttachments");

            migrationBuilder.DropTable(
                name: "CustomerMeasurements");

            migrationBuilder.DropTable(
                name: "LoyaltyTransactions");

            migrationBuilder.DropTable(
                name: "ProductReviews");

            migrationBuilder.DropTable(
                name: "Complaints");

            migrationBuilder.DropTable(
                name: "CustomerLoyalty");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Complaints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TailorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdminResponse = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ComplaintType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Other"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DesiredResolution = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Open"),
                    Subject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Complaints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Complaints_CustomerProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Complaints_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                    table.ForeignKey(
                        name: "FK_Complaints_TailorProfiles_TailorId",
                        column: x => x.TailorId,
                        principalTable: "TailorProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerLoyalty",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    LifetimePoints = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    ReferralCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ReferralsCount = table.Column<int>(type: "int", nullable: false),
                    ReferredBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Tier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Bronze"),
                    TotalOrders = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerLoyalty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerLoyalty_CustomerProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerMeasurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AbayaLength = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ArmLength = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Chest = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    CustomMeasurementsJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    GarmentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Hips = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    InseamLength = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NeckCircumference = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OutseamLength = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ShoulderWidth = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    SleeveLength = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ThighCircumference = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ThobeLength = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Waist = table.Column<decimal>(type: "decimal(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerMeasurements_CustomerProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductReviews",
                columns: table => new
                {
                    ReviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(getutcdate())"),
                    HelpfulCount = table.Column<int>(type: "int", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsVerifiedPurchase = table.Column<bool>(type: "bit", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReviews", x => x.ReviewId);
                    table.ForeignKey(
                        name: "FK_ProductReviews_CustomerProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductReviews_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                    table.ForeignKey(
                        name: "FK_ProductReviews_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                });

            migrationBuilder.CreateTable(
                name: "ComplaintAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComplaintId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintAttachments_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalTable: "Complaints",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerLoyaltyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Points = table.Column<int>(type: "int", nullable: false),
                    RelatedOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyTransactions_CustomerLoyalty_CustomerLoyaltyId",
                        column: x => x.CustomerLoyaltyId,
                        principalTable: "CustomerLoyalty",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintAttachments_ComplaintId",
                table: "ComplaintAttachments",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CustomerId",
                table: "Complaints",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_OrderId",
                table: "Complaints",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_Status",
                table: "Complaints",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_TailorId",
                table: "Complaints",
                column: "TailorId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLoyalty_CustomerId",
                table: "CustomerLoyalty",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLoyalty_ReferralCode",
                table: "CustomerLoyalty",
                column: "ReferralCode");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerMeasurements_CustomerId",
                table: "CustomerMeasurements",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_CreatedAt",
                table: "LoyaltyTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_CustomerLoyaltyId",
                table: "LoyaltyTransactions",
                column: "CustomerLoyaltyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_CustomerId",
                table: "ProductReviews",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_OrderId",
                table: "ProductReviews",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_ProductId",
                table: "ProductReviews",
                column: "ProductId");
        }
    }
}
