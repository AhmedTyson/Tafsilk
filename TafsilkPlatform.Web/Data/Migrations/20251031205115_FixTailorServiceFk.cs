using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TafsilkPlatform.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixTailorServiceFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceTokens_Users",
                table: "DeviceTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceTokens_Users_UserId1",
                table: "DeviceTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Disputes_Orders",
                table: "Disputes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId1",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderImages_Orders_OrderId",
                table: "OrderImages");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId1",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioImages_TailorProfiles",
                table: "PortfolioImages");

            migrationBuilder.DropForeignKey(
                name: "FK_RevenueReports_TailorProfiles",
                table: "RevenueReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_CustomerProfiles",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Orders",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_TailorProfiles",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_TailorBadges_TailorProfiles",
                table: "TailorBadges");

            migrationBuilder.DropForeignKey(
                name: "FK_TailorServices_TailorProfiles",
                table: "TailorServices");

            migrationBuilder.DropForeignKey(
                name: "FK_TailorServices_TailorProfiles_TailorId1",
                table: "TailorServices");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActivityLogs_Users",
                table: "UserActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActivityLogs_Users_UserId1",
                table: "UserActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallet_Users_UserId",
                table: "Wallet");

            migrationBuilder.DropIndex(
                name: "IX_UserActivityLogs_Action",
                table: "UserActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_UserActivityLogs_CreatedAt",
                table: "UserActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_UserActivityLogs_EntityType",
                table: "UserActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_UserActivityLogs_User_Action_Date",
                table: "UserActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_UserActivityLogs_UserId1",
                table: "UserActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_TailorServices_ServiceName",
                table: "TailorServices");

            migrationBuilder.DropIndex(
                name: "IX_TailorServices_TailorId1",
                table: "TailorServices");

            migrationBuilder.DropIndex(
                name: "IX_TailorBadges_TailorId",
                table: "TailorBadges");

            migrationBuilder.DropIndex(
                name: "IX_SystemMessages_AudienceType",
                table: "SystemMessages");

            migrationBuilder.DropIndex(
                name: "IX_SystemMessages_CreatedAt",
                table: "SystemMessages");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioImages_IsBeforeAfter",
                table: "PortfolioImages");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioImages_TailorId_UploadedAt",
                table: "PortfolioImages");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioImages_UploadedAt",
                table: "PortfolioImages");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderId1",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_SentAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Type",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_User_Read_Date",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId1",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_ErrorLogs_CreatedAt",
                table: "ErrorLogs");

            migrationBuilder.DropIndex(
                name: "IX_ErrorLogs_Severity",
                table: "ErrorLogs");

            migrationBuilder.DropIndex(
                name: "IX_ErrorLogs_Severity_CreatedAt",
                table: "ErrorLogs");

            migrationBuilder.DropIndex(
                name: "IX_DeviceTokens_Platform",
                table: "DeviceTokens");

            migrationBuilder.DropIndex(
                name: "IX_DeviceTokens_Token",
                table: "DeviceTokens");

            migrationBuilder.DropIndex(
                name: "IX_DeviceTokens_UserId1",
                table: "DeviceTokens");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserActivityLogs");

            migrationBuilder.DropColumn(
                name: "TailorId1",
                table: "TailorServices");

            migrationBuilder.DropColumn(
                name: "OrderId1",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "DeviceTokens");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserActivityLogs",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false,
                oldComment: "Soft delete flag");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "UserActivityLogs",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(45)",
                oldMaxLength: 45,
                oldComment: "IP address max 45 chars, supports IPv6");

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                table: "UserActivityLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldComment: "Entity type max 50 chars");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "UserActivityLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldComment: "Action description is required, max 100 chars");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceName",
                table: "TailorServices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldComment: "Service name is required, max 100 chars");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "TailorServices",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false,
                oldComment: "Soft delete flag");

            migrationBuilder.AlterColumn<int>(
                name: "EstimatedDuration",
                table: "TailorServices",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Estimated duration is required in minutes");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TailorServices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldComment: "Description max 500 chars");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "TailorBadges",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false,
                oldComment: "Soft delete flag");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EarnedAt",
                table: "TailorBadges",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getutcdate())");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TailorBadges",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldComment: "Description max 500 chars");

            migrationBuilder.AlterColumn<string>(
                name: "BadgeName",
                table: "TailorBadges",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldComment: "Badge name is required, max 150 chars");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "SystemMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldComment: "Message title is required, max 200 chars");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "SystemMessages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false,
                oldComment: "Soft delete flag");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SystemMessages",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getutcdate())");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "SystemMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldComment: "Message content is required, max 4000 chars");

            migrationBuilder.AlterColumn<string>(
                name: "AudienceType",
                table: "SystemMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Rating",
                table: "Reviews",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Rating is required");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Reviews",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false,
                oldComment: "Soft delete flag");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Reviews",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldComment: "Comment cannot exceed 1000 characters");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "RevenueReports",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false,
                oldComment: "Soft delete flag");

            migrationBuilder.AlterColumn<int>(
                name: "Score",
                table: "RatingDimensions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Score is required");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "RatingDimensions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false,
                oldComment: "Soft delete flag");

            migrationBuilder.AlterColumn<string>(
                name: "DimensionName",
                table: "RatingDimensions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldComment: "Dimension name is required, max 100 chars");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "PortfolioImages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false,
                oldComment: "Soft delete flag");

            migrationBuilder.AlterColumn<bool>(
                name: "IsBeforeAfter",
                table: "PortfolioImages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "Indicates if image is before/after");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldComment: "Notification title is required, max 200 chars");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldComment: "Notification message is required, max 2000 chars");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false,
                oldComment: "Soft delete flag");

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "ErrorLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Error",
                oldComment: "Severity level required, default 'Error', max 20 chars");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "ErrorLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldComment: "Error message is required, max 2000 chars");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ErrorLogs",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false,
                oldComment: "Soft delete flag");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ErrorLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getutcdate())");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Disputes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ResolutionDetails",
                table: "Disputes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Disputes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Disputes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Disputes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getutcdate())");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Disputes",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "(newid())");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "DeviceTokens",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false,
                oldComment: "Soft delete flag");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceTokens_Users_UserId",
                table: "DeviceTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Disputes_Orders_OrderId",
                table: "Disputes",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderImages_Orders_OrderId",
                table: "OrderImages",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioImages_TailorProfiles_TailorId",
                table: "PortfolioImages",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RevenueReports_TailorProfiles_TailorId",
                table: "RevenueReports",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_CustomerProfiles_CustomerId",
                table: "Reviews",
                column: "CustomerId",
                principalTable: "CustomerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Orders_OrderId",
                table: "Reviews",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_TailorProfiles_TailorId",
                table: "Reviews",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TailorServices_TailorProfiles_TailorId",
                table: "TailorServices",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivityLogs_Users_UserId",
                table: "UserActivityLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallet_Users_UserId",
                table: "Wallet",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceTokens_Users_UserId",
                table: "DeviceTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Disputes_Orders_OrderId",
                table: "Disputes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderImages_Orders_OrderId",
                table: "OrderImages");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioImages_TailorProfiles_TailorId",
                table: "PortfolioImages");

            migrationBuilder.DropForeignKey(
                name: "FK_RevenueReports_TailorProfiles_TailorId",
                table: "RevenueReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_CustomerProfiles_CustomerId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Orders_OrderId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_TailorProfiles_TailorId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_TailorServices_TailorProfiles_TailorId",
                table: "TailorServices");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActivityLogs_Users_UserId",
                table: "UserActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallet_Users_UserId",
                table: "Wallet");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserActivityLogs",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "UserActivityLogs",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: false,
                comment: "IP address max 45 chars, supports IPv6",
                oldClrType: typeof(string),
                oldType: "nvarchar(45)",
                oldMaxLength: 45);

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                table: "UserActivityLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                comment: "Entity type max 50 chars",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "UserActivityLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                comment: "Action description is required, max 100 chars",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "UserActivityLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ServiceName",
                table: "TailorServices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                comment: "Service name is required, max 100 chars",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "TailorServices",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "EstimatedDuration",
                table: "TailorServices",
                type: "int",
                nullable: false,
                comment: "Estimated duration is required in minutes",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TailorServices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                comment: "Description max 500 chars",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<Guid>(
                name: "TailorId1",
                table: "TailorServices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "TailorBadges",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EarnedAt",
                table: "TailorBadges",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getutcdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TailorBadges",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                comment: "Description max 500 chars",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BadgeName",
                table: "TailorBadges",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                comment: "Badge name is required, max 150 chars",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "SystemMessages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                comment: "Message title is required, max 200 chars",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "SystemMessages",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SystemMessages",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getutcdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "SystemMessages",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                comment: "Message content is required, max 4000 chars",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AudienceType",
                table: "SystemMessages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Rating",
                table: "Reviews",
                type: "int",
                nullable: false,
                comment: "Rating is required",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Reviews",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Reviews",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "Comment cannot exceed 1000 characters",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "RevenueReports",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "Score",
                table: "RatingDimensions",
                type: "int",
                nullable: false,
                comment: "Score is required",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "RatingDimensions",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "DimensionName",
                table: "RatingDimensions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                comment: "Dimension name is required, max 100 chars",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "PortfolioImages",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsBeforeAfter",
                table: "PortfolioImages",
                type: "bit",
                nullable: false,
                comment: "Indicates if image is before/after",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId1",
                table: "OrderItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                comment: "Notification title is required, max 200 chars",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                comment: "Notification message is required, max 2000 chars",
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "ErrorLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Error",
                comment: "Severity level required, default 'Error', max 20 chars",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "ErrorLogs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                comment: "Error message is required, max 2000 chars",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ErrorLogs",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ErrorLogs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getutcdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Disputes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ResolutionDetails",
                table: "Disputes",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Disputes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Disputes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Disputes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getutcdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Disputes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "(newid())",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "DeviceTokens",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "DeviceTokens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_Action",
                table: "UserActivityLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_CreatedAt",
                table: "UserActivityLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_EntityType",
                table: "UserActivityLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_User_Action_Date",
                table: "UserActivityLogs",
                columns: new[] { "UserId", "Action", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_UserId1",
                table: "UserActivityLogs",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_TailorServices_ServiceName",
                table: "TailorServices",
                column: "ServiceName");

            migrationBuilder.CreateIndex(
                name: "IX_TailorServices_TailorId1",
                table: "TailorServices",
                column: "TailorId1");

            migrationBuilder.CreateIndex(
                name: "IX_TailorBadges_TailorId",
                table: "TailorBadges",
                column: "TailorId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemMessages_AudienceType",
                table: "SystemMessages",
                column: "AudienceType");

            migrationBuilder.CreateIndex(
                name: "IX_SystemMessages_CreatedAt",
                table: "SystemMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioImages_IsBeforeAfter",
                table: "PortfolioImages",
                column: "IsBeforeAfter");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioImages_TailorId_UploadedAt",
                table: "PortfolioImages",
                columns: new[] { "TailorId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioImages_UploadedAt",
                table: "PortfolioImages",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId1",
                table: "OrderItems",
                column: "OrderId1");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SentAt",
                table: "Notifications",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type",
                table: "Notifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_User_Read_Date",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId1",
                table: "Notifications",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_CreatedAt",
                table: "ErrorLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_Severity",
                table: "ErrorLogs",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_Severity_CreatedAt",
                table: "ErrorLogs",
                columns: new[] { "Severity", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_Platform",
                table: "DeviceTokens",
                column: "Platform");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_Token",
                table: "DeviceTokens",
                column: "Devicetoken");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_UserId1",
                table: "DeviceTokens",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceTokens_Users",
                table: "DeviceTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceTokens_Users_UserId1",
                table: "DeviceTokens",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Disputes_Orders",
                table: "Disputes",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId1",
                table: "Notifications",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderImages_Orders_OrderId",
                table: "OrderImages",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId1",
                table: "OrderItems",
                column: "OrderId1",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioImages_TailorProfiles",
                table: "PortfolioImages",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RevenueReports_TailorProfiles",
                table: "RevenueReports",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_CustomerProfiles",
                table: "Reviews",
                column: "CustomerId",
                principalTable: "CustomerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Orders",
                table: "Reviews",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_TailorProfiles",
                table: "Reviews",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TailorBadges_TailorProfiles",
                table: "TailorBadges",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TailorServices_TailorProfiles",
                table: "TailorServices",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TailorServices_TailorProfiles_TailorId1",
                table: "TailorServices",
                column: "TailorId1",
                principalTable: "TailorProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivityLogs_Users",
                table: "UserActivityLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivityLogs_Users_UserId1",
                table: "UserActivityLogs",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Wallet_Users_UserId",
                table: "Wallet",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
