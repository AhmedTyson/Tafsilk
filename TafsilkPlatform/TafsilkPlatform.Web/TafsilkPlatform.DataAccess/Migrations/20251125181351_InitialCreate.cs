using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TafsilkPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdempotencyKeys",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    LastAccessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Endpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Method = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotencyKeys", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Permissions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__3214EC07CB85E41E", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    EmailVerificationTokenExpires = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PasswordResetTokenExpires = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BannedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BanReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BanExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__3214EC07AA820590", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProfilePictureData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ProfilePictureContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Customer__3214EC07880E7F94", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerProfiles_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TailorProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ShopName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ShopDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Specialization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    ExperienceYears = table.Column<int>(type: "int", nullable: true),
                    PricingRange = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BusinessHours = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FacebookUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InstagramUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TwitterUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProfilePictureData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ProfilePictureContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AverageRating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TailorPr__3214EC07A3FCF42C", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TailorProfiles_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Street = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserAddr__3214EC07DDE0E48B", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAddresses_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerLoyalty",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    LifetimePoints = table.Column<int>(type: "int", nullable: false),
                    Tier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Bronze"),
                    TotalOrders = table.Column<int>(type: "int", nullable: false),
                    ReferralsCount = table.Column<int>(type: "int", nullable: false),
                    ReferralCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ReferredBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
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
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GarmentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Chest = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Waist = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Hips = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ShoulderWidth = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    SleeveLength = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    InseamLength = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    OutseamLength = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    NeckCircumference = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ArmLength = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ThighCircumference = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ThobeLength = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    AbayaLength = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CustomMeasurementsJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                name: "ShoppingCarts",
                columns: table => new
                {
                    CartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCarts", x => x.CartId);
                    table.ForeignKey(
                        name: "FK_ShoppingCarts_CustomerProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DueDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TotalPrice = table.Column<double>(type: "float", nullable: false),
                    OrderType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TailorQuote = table.Column<double>(type: "float", nullable: true),
                    TailorQuoteNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuoteProvidedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RequiresDeposit = table.Column<bool>(type: "bit", nullable: false),
                    DepositAmount = table.Column<double>(type: "float", nullable: true),
                    DepositPaid = table.Column<bool>(type: "bit", nullable: false),
                    DepositPaidAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    MeasurementsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FulfillmentMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TailorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_CustomerProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_TailorProfiles_TailorId",
                        column: x => x.TailorId,
                        principalTable: "TailorProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PortfolioImages",
                columns: table => new
                {
                    PortfolioImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TailorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EstimatedPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsBeforeAfter = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioImages", x => x.PortfolioImageId);
                    table.ForeignKey(
                        name: "FK_PortfolioImages_TailorProfiles_TailorId",
                        column: x => x.TailorId,
                        principalTable: "TailorProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountedPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Size = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Material = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    SalesCount = table.Column<int>(type: "int", nullable: false),
                    AverageRating = table.Column<double>(type: "float", nullable: false),
                    ReviewCount = table.Column<int>(type: "int", nullable: false),
                    PrimaryImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PrimaryImageContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AdditionalImagesJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    MetaTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TailorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_TailorProfiles_TailorId",
                        column: x => x.TailorId,
                        principalTable: "TailorProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TailorServices",
                columns: table => new
                {
                    TailorServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TailorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EstimatedDuration = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TailorServices", x => x.TailorServiceId);
                    table.ForeignKey(
                        name: "FK_TailorServices_TailorProfiles_TailorId",
                        column: x => x.TailorId,
                        principalTable: "TailorProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerLoyaltyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RelatedOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
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

            migrationBuilder.CreateTable(
                name: "Complaints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TailorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ComplaintType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Other"),
                    DesiredResolution = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Open"),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    AdminResponse = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResolvedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                name: "OrderImages",
                columns: table => new
                {
                    OrderImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImgUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderImages", x => x.OrderImageId);
                    table.ForeignKey(
                        name: "FK_OrderImages_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TailorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentType = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    PaidAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProviderTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ProviderCustomerId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProviderMetadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardLast4 = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    CardBrand = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ThreeDSecureUsed = table.Column<bool>(type: "bit", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefundedAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    RefundedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payment_CustomerProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payment_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                    table.ForeignKey(
                        name: "FK_Payment_TailorProfiles_TailorId",
                        column: x => x.TailorId,
                        principalTable: "TailorProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    CartItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SelectedSize = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SelectedColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.CartItemId);
                    table.ForeignKey(
                        name: "FK_CartItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                    table.ForeignKey(
                        name: "FK_CartItems_ShoppingCarts_CartId",
                        column: x => x.CartId,
                        principalTable: "ShoppingCarts",
                        principalColumn: "CartId");
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    OrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SelectedSize = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SelectedColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.OrderItemId);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                });

            migrationBuilder.CreateTable(
                name: "ProductReviews",
                columns: table => new
                {
                    ReviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsVerifiedPurchase = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(getutcdate())"),
                    HelpfulCount = table.Column<int>(type: "int", nullable: false)
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
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItems",
                column: "ProductId");

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
                name: "IX_CustomerProfiles_UserId",
                table: "CustomerProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ__Customer__1788CC4D90808B91",
                table: "CustomerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyKeys_ExpiresAtUtc",
                table: "IdempotencyKeys",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyKeys_Status",
                table: "IdempotencyKeys",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyKeys_UserId",
                table: "IdempotencyKeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_CreatedAt",
                table: "LoyaltyTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_CustomerLoyaltyId",
                table: "LoyaltyTransactions",
                column: "CustomerLoyaltyId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderImages_OrderId",
                table: "OrderImages",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TailorId",
                table: "Orders",
                column: "TailorId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_CustomerId",
                table: "Payment",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_OrderId",
                table: "Payment",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_TailorId",
                table: "Payment",
                column: "TailorId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioImages_TailorId",
                table: "PortfolioImages",
                column: "TailorId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category",
                table: "Products",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsAvailable",
                table: "Products",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsFeatured",
                table: "Products",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TailorId",
                table: "Products",
                column: "TailorId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_CustomerId",
                table: "ShoppingCarts",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TailorProfiles_UserId",
                table: "TailorProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ__TailorPr__1788CC4D37A4BF4A",
                table: "TailorProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TailorServices_TailorId",
                table: "TailorServices",
                column: "TailorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_UserId",
                table: "UserAddresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D10534975288F0",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "ComplaintAttachments");

            migrationBuilder.DropTable(
                name: "CustomerMeasurements");

            migrationBuilder.DropTable(
                name: "IdempotencyKeys");

            migrationBuilder.DropTable(
                name: "LoyaltyTransactions");

            migrationBuilder.DropTable(
                name: "OrderImages");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "PortfolioImages");

            migrationBuilder.DropTable(
                name: "ProductReviews");

            migrationBuilder.DropTable(
                name: "TailorServices");

            migrationBuilder.DropTable(
                name: "UserAddresses");

            migrationBuilder.DropTable(
                name: "ShoppingCarts");

            migrationBuilder.DropTable(
                name: "Complaints");

            migrationBuilder.DropTable(
                name: "CustomerLoyalty");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "CustomerProfiles");

            migrationBuilder.DropTable(
                name: "TailorProfiles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
