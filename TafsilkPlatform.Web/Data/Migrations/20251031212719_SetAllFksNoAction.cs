using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TafsilkPlatform.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SetAllFksNoAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_Users_UserId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Admins_AdminId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_BannedUsers_Users_UserId",
                table: "BannedUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_RFQs_RFQId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_TailorProfiles_TailorId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceTokens_Users_UserId",
                table: "DeviceTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Disputes_Orders_OrderId",
                table: "Disputes");

            migrationBuilder.DropForeignKey(
                name: "FK_Disputes_Users_OpenedByUserId",
                table: "Disputes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderImages_Orders_OrderId",
                table: "OrderImages");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderImages_Orders_OrderId1",
                table: "OrderImages");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioImages_TailorProfiles_TailorId",
                table: "PortfolioImages");

            migrationBuilder.DropForeignKey(
                name: "FK_RatingDimensions_Reviews_ReviewId",
                table: "RatingDimensions");

            migrationBuilder.DropForeignKey(
                name: "FK_RefundRequests_Orders_OrderId",
                table: "RefundRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RefundRequests_Users_RequestedBy",
                table: "RefundRequests");

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
                name: "FK_RFQBids_RFQs_RFQId",
                table: "RFQBids");

            migrationBuilder.DropForeignKey(
                name: "FK_RFQBids_TailorProfiles_TailorId",
                table: "RFQBids");

            migrationBuilder.DropForeignKey(
                name: "FK_RFQs_CorporateAccounts_CorporateAccountId",
                table: "RFQs");

            migrationBuilder.DropForeignKey(
                name: "FK_TailorServices_TailorProfiles_TailorId",
                table: "TailorServices");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActivityLogs_Users_UserId",
                table: "UserActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallet_Users_UserId",
                table: "Wallet");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_Users_UserId",
                table: "Admins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Admins_AdminId",
                table: "AuditLogs",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BannedUsers_Users_UserId",
                table: "BannedUsers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_RFQs_RFQId",
                table: "Contracts",
                column: "RFQId",
                principalTable: "RFQs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_TailorProfiles_TailorId",
                table: "Contracts",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceTokens_Users_UserId",
                table: "DeviceTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Disputes_Orders_OrderId",
                table: "Disputes",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Disputes_Users_OpenedByUserId",
                table: "Disputes",
                column: "OpenedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderImages_Orders_OrderId",
                table: "OrderImages",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderImages_Orders_OrderId1",
                table: "OrderImages",
                column: "OrderId1",
                principalTable: "Orders",
                principalColumn: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioImages_TailorProfiles_TailorId",
                table: "PortfolioImages",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RatingDimensions_Reviews_ReviewId",
                table: "RatingDimensions",
                column: "ReviewId",
                principalTable: "Reviews",
                principalColumn: "ReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefundRequests_Orders_OrderId",
                table: "RefundRequests",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefundRequests_Users_RequestedBy",
                table: "RefundRequests",
                column: "RequestedBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RevenueReports_TailorProfiles_TailorId",
                table: "RevenueReports",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_CustomerProfiles_CustomerId",
                table: "Reviews",
                column: "CustomerId",
                principalTable: "CustomerProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Orders_OrderId",
                table: "Reviews",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_TailorProfiles_TailorId",
                table: "Reviews",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RFQBids_RFQs_RFQId",
                table: "RFQBids",
                column: "RFQId",
                principalTable: "RFQs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RFQBids_TailorProfiles_TailorId",
                table: "RFQBids",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RFQs_CorporateAccounts_CorporateAccountId",
                table: "RFQs",
                column: "CorporateAccountId",
                principalTable: "CorporateAccounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TailorServices_TailorProfiles_TailorId",
                table: "TailorServices",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivityLogs_Users_UserId",
                table: "UserActivityLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Wallet_Users_UserId",
                table: "Wallet",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_Users_UserId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Admins_AdminId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_BannedUsers_Users_UserId",
                table: "BannedUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_RFQs_RFQId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_TailorProfiles_TailorId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceTokens_Users_UserId",
                table: "DeviceTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Disputes_Orders_OrderId",
                table: "Disputes");

            migrationBuilder.DropForeignKey(
                name: "FK_Disputes_Users_OpenedByUserId",
                table: "Disputes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderImages_Orders_OrderId",
                table: "OrderImages");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderImages_Orders_OrderId1",
                table: "OrderImages");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioImages_TailorProfiles_TailorId",
                table: "PortfolioImages");

            migrationBuilder.DropForeignKey(
                name: "FK_RatingDimensions_Reviews_ReviewId",
                table: "RatingDimensions");

            migrationBuilder.DropForeignKey(
                name: "FK_RefundRequests_Orders_OrderId",
                table: "RefundRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RefundRequests_Users_RequestedBy",
                table: "RefundRequests");

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
                name: "FK_RFQBids_RFQs_RFQId",
                table: "RFQBids");

            migrationBuilder.DropForeignKey(
                name: "FK_RFQBids_TailorProfiles_TailorId",
                table: "RFQBids");

            migrationBuilder.DropForeignKey(
                name: "FK_RFQs_CorporateAccounts_CorporateAccountId",
                table: "RFQs");

            migrationBuilder.DropForeignKey(
                name: "FK_TailorServices_TailorProfiles_TailorId",
                table: "TailorServices");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActivityLogs_Users_UserId",
                table: "UserActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallet_Users_UserId",
                table: "Wallet");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_Users_UserId",
                table: "Admins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Admins_AdminId",
                table: "AuditLogs",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BannedUsers_Users_UserId",
                table: "BannedUsers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_RFQs_RFQId",
                table: "Contracts",
                column: "RFQId",
                principalTable: "RFQs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_TailorProfiles_TailorId",
                table: "Contracts",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_Disputes_Users_OpenedByUserId",
                table: "Disputes",
                column: "OpenedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
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
                name: "FK_OrderImages_Orders_OrderId1",
                table: "OrderImages",
                column: "OrderId1",
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
                name: "FK_RatingDimensions_Reviews_ReviewId",
                table: "RatingDimensions",
                column: "ReviewId",
                principalTable: "Reviews",
                principalColumn: "ReviewId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefundRequests_Orders_OrderId",
                table: "RefundRequests",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefundRequests_Users_RequestedBy",
                table: "RefundRequests",
                column: "RequestedBy",
                principalTable: "Users",
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
                name: "FK_RFQBids_RFQs_RFQId",
                table: "RFQBids",
                column: "RFQId",
                principalTable: "RFQs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RFQBids_TailorProfiles_TailorId",
                table: "RFQBids",
                column: "TailorId",
                principalTable: "TailorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RFQs_CorporateAccounts_CorporateAccountId",
                table: "RFQs",
                column: "CorporateAccountId",
                principalTable: "CorporateAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
    }
}
