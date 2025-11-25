IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;

DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql = @sql + N'ALTER TABLE [' + OBJECT_SCHEMA_NAME(fk.parent_object_id) + N'].[' + OBJECT_NAME(fk.parent_object_id) + N'] DROP CONSTRAINT [' + fk.name + N'];' + CHAR(13)
FROM sys.foreign_keys fk
WHERE fk.referenced_object_id = OBJECT_ID(N'dbo.Orders')
 AND fk.delete_referential_action =1; --1 = CASCADE

IF (@sql <> N'')
BEGIN
 EXEC sp_executesql @sql;
END


CREATE TABLE [AppSettings] (
    [Id] uniqueidentifier NOT NULL,
    [Key] nvarchar(max) NOT NULL,
    [Value] nvarchar(max) NOT NULL,
    [LastUpdated] datetime2 NOT NULL,
    CONSTRAINT [PK_AppSettings] PRIMARY KEY ([Id])
);

CREATE TABLE [ErrorLogs] (
    [ErrorLogId] uniqueidentifier NOT NULL,
    [Message] nvarchar(2000) NOT NULL,
    [StackTrace] nvarchar(max) NOT NULL,
    [Severity] nvarchar(20) NOT NULL DEFAULT N'Error',
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_ErrorLogs] PRIMARY KEY ([ErrorLogId])
);
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'Error message is required, max 2000 chars';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'ErrorLogs', 'COLUMN', N'Message';
SET @description = N'Severity level required, default ''Error'', max 20 chars';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'ErrorLogs', 'COLUMN', N'Severity';
SET @description = N'Soft delete flag';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'ErrorLogs', 'COLUMN', N'IsDeleted';

CREATE TABLE [Roles] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__Roles__3214EC07CB85E41E] PRIMARY KEY ([Id])
);

CREATE TABLE [SystemMessages] (
    [SystemMessageId] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Content] nvarchar(4000) NOT NULL,
    [AudienceType] nvarchar(50) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_SystemMessages] PRIMARY KEY ([SystemMessageId])
);
DECLARE @defaultSchema1 AS sysname;
SET @defaultSchema1 = SCHEMA_NAME();
DECLARE @description1 AS sql_variant;
SET @description1 = N'Message title is required, max 200 chars';
EXEC sp_addextendedproperty 'MS_Description', @description1, 'SCHEMA', @defaultSchema1, 'TABLE', N'SystemMessages', 'COLUMN', N'Title';
SET @description1 = N'Message content is required, max 4000 chars';
EXEC sp_addextendedproperty 'MS_Description', @description1, 'SCHEMA', @defaultSchema1, 'TABLE', N'SystemMessages', 'COLUMN', N'Content';
SET @description1 = N'Soft delete flag';
EXEC sp_addextendedproperty 'MS_Description', @description1, 'SCHEMA', @defaultSchema1, 'TABLE', N'SystemMessages', 'COLUMN', N'IsDeleted';

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [Email] nvarchar(255) NOT NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK__Users__3214EC07AA820590] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Users_Roles] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id])
);

CREATE TABLE [Admins] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Permissions] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Admins] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Admins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [BannedUsers] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Reason] nvarchar(max) NOT NULL,
    [BannedAt] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NULL,
    CONSTRAINT [PK_BannedUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BannedUsers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [CorporateAccounts] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [UserId] uniqueidentifier NOT NULL,
    [CompanyName] nvarchar(255) NOT NULL,
    [ContactPerson] nvarchar(255) NOT NULL,
    [Industry] nvarchar(100) NULL,
    [TaxNumber] nvarchar(100) NULL,
    [IsApproved] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK__Corporat__3214EC07AC002547] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CorporateAccounts_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [CustomerProfiles] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [UserId] uniqueidentifier NOT NULL,
    [FullName] nvarchar(255) NOT NULL,
    [Gender] nvarchar(20) NULL,
    [City] nvarchar(100) NULL,
    [DateOfBirth] date NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK__Customer__3214EC07880E7F94] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerProfiles_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [DeviceTokens] (
    [DeviceTokenId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Devicetoken] nvarchar(500) NOT NULL,
    [Platform] nvarchar(20) NOT NULL,
    [RegisteredAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UserId1] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_DeviceTokens] PRIMARY KEY ([DeviceTokenId]),
    CONSTRAINT [FK_DeviceTokens_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DeviceTokens_Users_UserId1] FOREIGN KEY ([UserId1]) REFERENCES [Users] ([Id])
);
DECLARE @defaultSchema2 AS sysname;
SET @defaultSchema2 = SCHEMA_NAME();
DECLARE @description2 AS sql_variant;
SET @description2 = N'Soft delete flag';
EXEC sp_addextendedproperty 'MS_Description', @description2, 'SCHEMA', @defaultSchema2, 'TABLE', N'DeviceTokens', 'COLUMN', N'IsDeleted';

CREATE TABLE [Notifications] (
    [NotificationId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Message] nvarchar(2000) NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [IsRead] bit NOT NULL DEFAULT CAST(0 AS bit),
    [SentAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UserId1] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationId]),
    CONSTRAINT [FK_Notifications_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Notifications_Users_UserId1] FOREIGN KEY ([UserId1]) REFERENCES [Users] ([Id])
);
DECLARE @defaultSchema3 AS sysname;
SET @defaultSchema3 = SCHEMA_NAME();
DECLARE @description3 AS sql_variant;
SET @description3 = N'Notification title is required, max 200 chars';
EXEC sp_addextendedproperty 'MS_Description', @description3, 'SCHEMA', @defaultSchema3, 'TABLE', N'Notifications', 'COLUMN', N'Title';
SET @description3 = N'Notification message is required, max 2000 chars';
EXEC sp_addextendedproperty 'MS_Description', @description3, 'SCHEMA', @defaultSchema3, 'TABLE', N'Notifications', 'COLUMN', N'Message';
SET @description3 = N'Soft delete flag';
EXEC sp_addextendedproperty 'MS_Description', @description3, 'SCHEMA', @defaultSchema3, 'TABLE', N'Notifications', 'COLUMN', N'IsDeleted';

CREATE TABLE [RefreshTokens] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [UserId] uniqueidentifier NOT NULL,
    [Token] nvarchar(max) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [RevokedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__RefreshT__3214EC07E9DA722D] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [TailorProfiles] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [UserId] uniqueidentifier NOT NULL,
    [ShopName] nvarchar(255) NOT NULL,
    [Address] nvarchar(500) NOT NULL,
    [Latitude] decimal(10,8) NULL,
    [Longitude] decimal(11,8) NULL,
    [ExperienceYears] int NULL,
    [PricingRange] nvarchar(100) NULL,
    [Bio] nvarchar(1000) NULL,
    [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK__TailorPr__3214EC07A3FCF42C] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TailorProfiles_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [UserActivityLogs] (
    [UserActivityLogId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Action] nvarchar(100) NOT NULL,
    [EntityType] nvarchar(50) NOT NULL,
    [EntityId] int NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [IpAddress] nvarchar(45) NOT NULL,
    [UserId1] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_UserActivityLogs] PRIMARY KEY ([UserActivityLogId]),
    CONSTRAINT [FK_UserActivityLogs_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserActivityLogs_Users_UserId1] FOREIGN KEY ([UserId1]) REFERENCES [Users] ([Id])
);
DECLARE @defaultSchema4 AS sysname;
SET @defaultSchema4 = SCHEMA_NAME();
DECLARE @description4 AS sql_variant;
SET @description4 = N'Action description is required, max 100 chars';
EXEC sp_addextendedproperty 'MS_Description', @description4, 'SCHEMA', @defaultSchema4, 'TABLE', N'UserActivityLogs', 'COLUMN', N'Action';
SET @description4 = N'Entity type max 50 chars';
EXEC sp_addextendedproperty 'MS_Description', @description4, 'SCHEMA', @defaultSchema4, 'TABLE', N'UserActivityLogs', 'COLUMN', N'EntityType';
SET @description4 = N'IP address max 45 chars, supports IPv6';
EXEC sp_addextendedproperty 'MS_Description', @description4, 'SCHEMA', @defaultSchema4, 'TABLE', N'UserActivityLogs', 'COLUMN', N'IpAddress';
SET @description4 = N'Soft delete flag';
EXEC sp_addextendedproperty 'MS_Description', @description4, 'SCHEMA', @defaultSchema4, 'TABLE', N'UserActivityLogs', 'COLUMN', N'IsDeleted';

CREATE TABLE [UserAddresses] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [UserId] uniqueidentifier NOT NULL,
    [Label] nvarchar(100) NOT NULL,
    [Street] nvarchar(255) NOT NULL,
    [City] nvarchar(100) NOT NULL,
    [Latitude] decimal(10,8) NULL,
    [Longitude] decimal(11,8) NULL,
    [IsDefault] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__UserAddr__3214EC07DDE0E48B] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserAddresses_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [Wallet] (
    [WalletId] int NOT NULL IDENTITY,
    [UserId] uniqueidentifier NOT NULL,
    [Balance] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Wallet] PRIMARY KEY ([WalletId]),
    CONSTRAINT [FK_Wallet_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [AuditLogs] (
    [Id] uniqueidentifier NOT NULL,
    [AdminId] uniqueidentifier NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [AffectedEntity] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AuditLogs_Admins_AdminId] FOREIGN KEY ([AdminId]) REFERENCES [Admins] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RFQs] (
    [Id] uniqueidentifier NOT NULL,
    [CorporateAccountId] uniqueidentifier NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Quantity] int NOT NULL,
    [Budget] decimal(18,2) NOT NULL,
    [Deadline] datetime2 NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_RFQs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RFQs_CorporateAccounts_CorporateAccountId] FOREIGN KEY ([CorporateAccountId]) REFERENCES [CorporateAccounts] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Orders] (
    [OrderId] uniqueidentifier NOT NULL,
    [Discription] nvarchar(max) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [DueDate] datetimeoffset NULL,
    [TotalPrice] float NOT NULL,
    [OrderType] nvarchar(max) NOT NULL,
    [Status] int NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [TailorId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([OrderId]),
    CONSTRAINT [FK_Orders_CustomerProfiles_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [CustomerProfiles] ([Id]),
    CONSTRAINT [FK_Orders_TailorProfiles_TailorId] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id])
);

CREATE TABLE [PortfolioImages] (
    [PortfolioImageId] uniqueidentifier NOT NULL,
    [TailorId] uniqueidentifier NOT NULL,
    [ImageUrl] nvarchar(500) NOT NULL,
    [IsBeforeAfter] bit NOT NULL,
    [UploadedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [TailorId1] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_PortfolioImages] PRIMARY KEY ([PortfolioImageId]),
    CONSTRAINT [FK_PortfolioImages_TailorProfiles] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PortfolioImages_TailorProfiles_TailorId1] FOREIGN KEY ([TailorId1]) REFERENCES [TailorProfiles] ([Id])
);
DECLARE @defaultSchema5 AS sysname;
SET @defaultSchema5 = SCHEMA_NAME();
DECLARE @description5 AS sql_variant;
SET @description5 = N'Indicates if image is before/after';
EXEC sp_addextendedproperty 'MS_Description', @description5, 'SCHEMA', @defaultSchema5, 'TABLE', N'PortfolioImages', 'COLUMN', N'IsBeforeAfter';
SET @description5 = N'Soft delete flag';
EXEC sp_addextendedproperty 'MS_Description', @description5, 'SCHEMA', @defaultSchema5, 'TABLE', N'PortfolioImages', 'COLUMN', N'IsDeleted';

CREATE TABLE [RevenueReports] (
    [TailorId] uniqueidentifier NOT NULL,
    [Month] date NOT NULL,
    [TotalRevenue] decimal(18,2) NOT NULL,
    [CompletedOrders] int NOT NULL,
    [GeneratedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [TailorId1] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_RevenueReports] PRIMARY KEY ([TailorId], [Month]),
    CONSTRAINT [FK_RevenueReports_TailorProfiles] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RevenueReports_TailorProfiles_TailorId1] FOREIGN KEY ([TailorId1]) REFERENCES [TailorProfiles] ([Id])
);
DECLARE @defaultSchema6 AS sysname;
SET @defaultSchema6 = SCHEMA_NAME();
DECLARE @description6 AS sql_variant;
SET @description6 = N'Soft delete flag';
EXEC sp_addextendedproperty 'MS_Description', @description6, 'SCHEMA', @defaultSchema6, 'TABLE', N'RevenueReports', 'COLUMN', N'IsDeleted';

CREATE TABLE [TailorBadges] (
    [TailorBadgeId] uniqueidentifier NOT NULL,
    [TailorId] uniqueidentifier NOT NULL,
    [BadgeName] nvarchar(150) NOT NULL,
    [EarnedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [Description] nvarchar(500) NOT NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_TailorBadges] PRIMARY KEY ([TailorBadgeId]),
    CONSTRAINT [FK_TailorBadges_TailorProfiles] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id])
);
DECLARE @defaultSchema7 AS sysname;
SET @defaultSchema7 = SCHEMA_NAME();
DECLARE @description7 AS sql_variant;
SET @description7 = N'Badge name is required, max 150 chars';
EXEC sp_addextendedproperty 'MS_Description', @description7, 'SCHEMA', @defaultSchema7, 'TABLE', N'TailorBadges', 'COLUMN', N'BadgeName';
SET @description7 = N'Description max 500 chars';
EXEC sp_addextendedproperty 'MS_Description', @description7, 'SCHEMA', @defaultSchema7, 'TABLE', N'TailorBadges', 'COLUMN', N'Description';
SET @description7 = N'Soft delete flag';
EXEC sp_addextendedproperty 'MS_Description', @description7, 'SCHEMA', @defaultSchema7, 'TABLE', N'TailorBadges', 'COLUMN', N'IsDeleted';

CREATE TABLE [TailorServices] (
    [TailorServiceId] uniqueidentifier NOT NULL,
    [TailorId] uniqueidentifier NOT NULL,
    [ServiceName] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [BasePrice] decimal(18,2) NOT NULL,
    [EstimatedDuration] int NOT NULL,
    [TailorId1] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_TailorServices] PRIMARY KEY ([TailorServiceId]),
    CONSTRAINT [FK_TailorServices_TailorProfiles] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TailorServices_TailorProfiles_TailorId1] FOREIGN KEY ([TailorId1]) REFERENCES [TailorProfiles] ([Id])
);
DECLARE @defaultSchema8 AS sysname;
SET @defaultSchema8 = SCHEMA_NAME();
DECLARE @description8 AS sql_variant;
SET @description8 = N'Service name is required, max 100 chars';
EXEC sp_addextendedproperty 'MS_Description', @description8, 'SCHEMA', @defaultSchema8, 'TABLE', N'TailorServices', 'COLUMN', N'ServiceName';
SET @description8 = N'Description max 500 chars';
EXEC sp_addextendedproperty 'MS_Description', @description8, 'SCHEMA', @defaultSchema8, 'TABLE', N'TailorServices', 'COLUMN', N'Description';
SET @description8 = N'Estimated duration is required in minutes';
EXEC sp_addextendedproperty 'MS_Description', @description8, 'SCHEMA', @defaultSchema8, 'TABLE', N'TailorServices', 'COLUMN', N'EstimatedDuration';
SET @description8 = N'Soft delete flag';
EXEC sp_addextendedproperty 'MS_Description', @description8, 'SCHEMA', @defaultSchema8, 'TABLE', N'TailorServices', 'COLUMN', N'IsDeleted';

CREATE TABLE [Contracts] (
    [Id] uniqueidentifier NOT NULL,
    [RFQId] uniqueidentifier NOT NULL,
    [TailorId] uniqueidentifier NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [ContractStatus] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Contracts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Contracts_RFQs_RFQId] FOREIGN KEY ([RFQId]) REFERENCES [RFQs] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Contracts_TailorProfiles_TailorId] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RFQBids] (
    [Id] uniqueidentifier NOT NULL,
    [RFQId] uniqueidentifier NOT NULL,
    [TailorId] uniqueidentifier NOT NULL,
    [BidAmount] decimal(18,2) NOT NULL,
    [EstimatedDelivery] datetime2 NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_RFQBids] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RFQBids_RFQs_RFQId] FOREIGN KEY ([RFQId]) REFERENCES [RFQs] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RFQBids_TailorProfiles_TailorId] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Disputes] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [OrderId] uniqueidentifier NOT NULL,
    [OpenedByUserId] uniqueidentifier NOT NULL,
    [Reason] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [ResolvedByAdminId] uniqueidentifier NULL,
    [ResolutionDetails] nvarchar(2000) NULL,
    [ResolvedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK_Disputes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Disputes_Orders] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]),
    CONSTRAINT [FK_Disputes_Users_OpenedByUserId] FOREIGN KEY ([OpenedByUserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Disputes_Users_ResolvedByAdminId] FOREIGN KEY ([ResolvedByAdminId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [OrderImages] (
    [OrderImageId] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NOT NULL,
    [ImgUrl] nvarchar(max) NOT NULL,
    [UploadedId] nvarchar(max) NOT NULL,
    [UploadedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_OrderImages] PRIMARY KEY ([OrderImageId]),
    CONSTRAINT [FK_OrderImages_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId])
);

CREATE TABLE [OrderItems] (
    [OrderItemId] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NOT NULL,
    [ItemName] nvarchar(max) NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [Total] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_OrderItems] PRIMARY KEY ([OrderItemId]),
    CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId])
);

CREATE TABLE [Payment] (
    [PaymentId] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [TailorId] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [PaymentType] int NOT NULL,
    [PaymentStatus] int NOT NULL,
    [TransactionType] int NOT NULL,
    [PaidAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Payment] PRIMARY KEY ([PaymentId]),
    CONSTRAINT [FK_Payment_CustomerProfiles_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [CustomerProfiles] ([Id]),
    CONSTRAINT [FK_Payment_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]),
    CONSTRAINT [FK_Payment_TailorProfiles_TailorId] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id])
);

CREATE TABLE [Quotes] (
    [QuoteId] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NOT NULL,
    [TailorId] uniqueidentifier NOT NULL,
    [ProposedPrice] decimal(18,2) NOT NULL,
    [EstimatedDays] int NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Quotes] PRIMARY KEY ([QuoteId]),
    CONSTRAINT [FK_Quotes_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]),
    CONSTRAINT [FK_Quotes_TailorProfiles_TailorId] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id])
);

CREATE TABLE [RefundRequests] (
    [Id] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NOT NULL,
    [RequestedBy] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [ProcessedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_RefundRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefundRequests_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]) ON DELETE CASCADE,
    CONSTRAINT [FK_RefundRequests_Users_RequestedBy] FOREIGN KEY ([RequestedBy]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Reviews] (
    [ReviewId] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NOT NULL,
    [TailorId] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [Rating] int NOT NULL,
    [Comment] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [OrderId1] uniqueidentifier NULL,
    [TailorId1] uniqueidentifier NULL,
    [CustomerId1] uniqueidentifier NULL,
    CONSTRAINT [PK_Reviews] PRIMARY KEY ([ReviewId]),
    CONSTRAINT [FK_Reviews_CustomerProfiles] FOREIGN KEY ([CustomerId]) REFERENCES [CustomerProfiles] ([Id]),
    CONSTRAINT [FK_Reviews_CustomerProfiles_CustomerId1] FOREIGN KEY ([CustomerId1]) REFERENCES [CustomerProfiles] ([Id]),
    CONSTRAINT [FK_Reviews_Orders] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]),
    CONSTRAINT [FK_Reviews_Orders_OrderId1] FOREIGN KEY ([OrderId1]) REFERENCES [Orders] ([OrderId]),
    CONSTRAINT [FK_Reviews_TailorProfiles] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id]),
    CONSTRAINT [FK_Reviews_TailorProfiles_TailorId1] FOREIGN KEY ([TailorId1]) REFERENCES [TailorProfiles] ([Id])
);
DECLARE @defaultSchema9 AS sysname;
SET @defaultSchema9 = SCHEMA_NAME();
DECLARE @description9 AS sql_variant;
SET @description9 = N'Rating is required';
EXEC sp_addextendedproperty 'MS_Description', @description9, 'SCHEMA', @defaultSchema9, 'TABLE', N'Reviews', 'COLUMN', N'Rating';
SET @description9 = N'Comment cannot exceed 1000 characters';
EXEC sp_addextendedproperty 'MS_Description', @description9, 'SCHEMA', @defaultSchema9, 'TABLE', N'Reviews', 'COLUMN', N'Comment';
SET @description9 = N'Soft delete flag';
EXEC sp_addextendedproperty 'MS_Description', @description9, 'SCHEMA', @defaultSchema9, 'TABLE', N'Reviews', 'COLUMN', N'IsDeleted';

CREATE TABLE [RatingDimensions] (
    [RatingDimensionId] uniqueidentifier NOT NULL,
    [ReviewId] uniqueidentifier NOT NULL,
    [DimensionName] nvarchar(100) NOT NULL,
    [Score] int NOT NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_RatingDimensions] PRIMARY KEY ([RatingDimensionId]),
    CONSTRAINT [FK_RatingDimensions_Reviews_ReviewId] FOREIGN KEY ([ReviewId]) REFERENCES [Reviews] ([ReviewId]) ON DELETE CASCADE
);
DECLARE @defaultSchema10 AS sysname;
SET @defaultSchema10 = SCHEMA_NAME();
DECLARE @description10 AS sql_variant;
SET @description10 = N'Dimension name is required, max 100 chars';
EXEC sp_addextendedproperty 'MS_Description', @description10, 'SCHEMA', @defaultSchema10, 'TABLE', N'RatingDimensions', 'COLUMN', N'DimensionName';
SET @description10 = N'Score is required';
EXEC sp_addextendedproperty 'MS_Description', @description10, 'SCHEMA', @defaultSchema10, 'TABLE', N'RatingDimensions', 'COLUMN', N'Score';
SET @description10 = N'Soft delete flag';
EXEC sp_addextendedproperty 'MS_Description', @description10, 'SCHEMA', @defaultSchema10, 'TABLE', N'RatingDimensions', 'COLUMN', N'IsDeleted';

CREATE INDEX [IX_Admins_UserId] ON [Admins] ([UserId]);

CREATE INDEX [IX_AuditLogs_AdminId] ON [AuditLogs] ([AdminId]);

CREATE INDEX [IX_BannedUsers_UserId] ON [BannedUsers] ([UserId]);

CREATE INDEX [IX_Contracts_RFQId] ON [Contracts] ([RFQId]);

CREATE INDEX [IX_Contracts_TailorId] ON [Contracts] ([TailorId]);

CREATE INDEX [IX_CorporateAccounts_UserId] ON [CorporateAccounts] ([UserId]);

CREATE UNIQUE INDEX [UQ__Corporat__1788CC4D39440DF8] ON [CorporateAccounts] ([UserId]);

CREATE INDEX [IX_CustomerProfiles_UserId] ON [CustomerProfiles] ([UserId]);

CREATE UNIQUE INDEX [UQ__Customer__1788CC4D90808B91] ON [CustomerProfiles] ([UserId]);

CREATE INDEX [IX_DeviceTokens_Platform] ON [DeviceTokens] ([Platform]);

CREATE INDEX [IX_DeviceTokens_Token] ON [DeviceTokens] ([Devicetoken]);

CREATE INDEX [IX_DeviceTokens_UserId] ON [DeviceTokens] ([UserId]);

CREATE INDEX [IX_DeviceTokens_UserId1] ON [DeviceTokens] ([UserId1]);

CREATE INDEX [IX_Disputes_OpenedByUserId] ON [Disputes] ([OpenedByUserId]);

CREATE INDEX [IX_Disputes_OrderId] ON [Disputes] ([OrderId]);

CREATE INDEX [IX_Disputes_ResolvedByAdminId] ON [Disputes] ([ResolvedByAdminId]);

CREATE INDEX [IX_ErrorLogs_CreatedAt] ON [ErrorLogs] ([CreatedAt]);

CREATE INDEX [IX_ErrorLogs_Severity] ON [ErrorLogs] ([Severity]);

CREATE INDEX [IX_ErrorLogs_Severity_CreatedAt] ON [ErrorLogs] ([Severity], [CreatedAt]);

CREATE INDEX [IX_Notifications_IsRead] ON [Notifications] ([IsRead]);

CREATE INDEX [IX_Notifications_SentAt] ON [Notifications] ([SentAt]);

CREATE INDEX [IX_Notifications_Type] ON [Notifications] ([Type]);

CREATE INDEX [IX_Notifications_User_Read_Date] ON [Notifications] ([UserId], [IsRead], [SentAt]);

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);

CREATE INDEX [IX_Notifications_UserId1] ON [Notifications] ([UserId1]);

CREATE INDEX [IX_OrderImages_OrderId] ON [OrderImages] ([OrderId]);

CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);

CREATE INDEX [IX_Orders_CustomerId] ON [Orders] ([CustomerId]);

CREATE INDEX [IX_Orders_TailorId] ON [Orders] ([TailorId]);

CREATE INDEX [IX_Payment_CustomerId] ON [Payment] ([CustomerId]);

CREATE INDEX [IX_Payment_OrderId] ON [Payment] ([OrderId]);

CREATE INDEX [IX_Payment_TailorId] ON [Payment] ([TailorId]);

CREATE INDEX [IX_PortfolioImages_IsBeforeAfter] ON [PortfolioImages] ([IsBeforeAfter]);

CREATE INDEX [IX_PortfolioImages_TailorId] ON [PortfolioImages] ([TailorId]);

CREATE INDEX [IX_PortfolioImages_TailorId_UploadedAt] ON [PortfolioImages] ([TailorId], [UploadedAt]);

CREATE INDEX [IX_PortfolioImages_TailorId1] ON [PortfolioImages] ([TailorId1]);

CREATE INDEX [IX_PortfolioImages_UploadedAt] ON [PortfolioImages] ([UploadedAt]);

CREATE INDEX [IX_Quotes_OrderId] ON [Quotes] ([OrderId]);

CREATE INDEX [IX_Quotes_TailorId] ON [Quotes] ([TailorId]);

CREATE INDEX [IX_RatingDimensions_ReviewId] ON [RatingDimensions] ([ReviewId]);

CREATE INDEX [IX_RefreshTokens_ExpiresAt] ON [RefreshTokens] ([ExpiresAt]);

CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);

CREATE INDEX [IX_RefundRequests_OrderId] ON [RefundRequests] ([OrderId]);

CREATE INDEX [IX_RefundRequests_RequestedBy] ON [RefundRequests] ([RequestedBy]);

CREATE INDEX [IX_RevenueReports_TailorId1] ON [RevenueReports] ([TailorId1]);

CREATE INDEX [IX_Reviews_CustomerId] ON [Reviews] ([CustomerId]);

CREATE INDEX [IX_Reviews_CustomerId1] ON [Reviews] ([CustomerId1]);

CREATE UNIQUE INDEX [IX_Reviews_OrderId] ON [Reviews] ([OrderId]);

CREATE INDEX [IX_Reviews_OrderId1] ON [Reviews] ([OrderId1]);

CREATE INDEX [IX_Reviews_TailorId] ON [Reviews] ([TailorId]);

CREATE INDEX [IX_Reviews_TailorId1] ON [Reviews] ([TailorId1]);

CREATE INDEX [IX_RFQBids_RFQId] ON [RFQBids] ([RFQId]);

CREATE INDEX [IX_RFQBids_TailorId] ON [RFQBids] ([TailorId]);

CREATE INDEX [IX_RFQs_CorporateAccountId] ON [RFQs] ([CorporateAccountId]);

CREATE INDEX [IX_SystemMessages_AudienceType] ON [SystemMessages] ([AudienceType]);

CREATE INDEX [IX_SystemMessages_CreatedAt] ON [SystemMessages] ([CreatedAt]);

CREATE INDEX [IX_TailorBadges_TailorId] ON [TailorBadges] ([TailorId]);

CREATE INDEX [IX_TailorProfiles_UserId] ON [TailorProfiles] ([UserId]);

CREATE UNIQUE INDEX [UQ__TailorPr__1788CC4D37A4BF4A] ON [TailorProfiles] ([UserId]);

CREATE INDEX [IX_TailorServices_ServiceName] ON [TailorServices] ([ServiceName]);

CREATE INDEX [IX_TailorServices_TailorId] ON [TailorServices] ([TailorId]);

CREATE INDEX [IX_TailorServices_TailorId1] ON [TailorServices] ([TailorId1]);

CREATE INDEX [IX_UserActivityLogs_Action] ON [UserActivityLogs] ([Action]);

CREATE INDEX [IX_UserActivityLogs_CreatedAt] ON [UserActivityLogs] ([CreatedAt]);

CREATE INDEX [IX_UserActivityLogs_EntityType] ON [UserActivityLogs] ([EntityType]);

CREATE INDEX [IX_UserActivityLogs_User_Action_Date] ON [UserActivityLogs] ([UserId], [Action], [CreatedAt]);

CREATE INDEX [IX_UserActivityLogs_UserId] ON [UserActivityLogs] ([UserId]);

CREATE INDEX [IX_UserActivityLogs_UserId1] ON [UserActivityLogs] ([UserId1]);

CREATE INDEX [IX_UserAddresses_UserId] ON [UserAddresses] ([UserId]);

CREATE INDEX [IX_Users_Email] ON [Users] ([Email]);

CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);

CREATE UNIQUE INDEX [UQ__Users__A9D10534975288F0] ON [Users] ([Email]);

CREATE UNIQUE INDEX [IX_Wallet_UserId] ON [Wallet] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018223954_FixFluentApiAndRelations', N'9.0.10');

ALTER TABLE [Users] ADD [EmailNotifications] bit NOT NULL DEFAULT CAST(1 AS bit);

ALTER TABLE [Users] ADD [PromotionalNotifications] bit NOT NULL DEFAULT CAST(1 AS bit);

ALTER TABLE [Users] ADD [SmsNotifications] bit NOT NULL DEFAULT CAST(1 AS bit);

ALTER TABLE [TailorProfiles] ADD [City] nvarchar(100) NULL;

ALTER TABLE [TailorProfiles] ADD [FullName] nvarchar(255) NULL;

ALTER TABLE [TailorProfiles] ADD [ProfilePictureUrl] nvarchar(500) NULL;

ALTER TABLE [CustomerProfiles] ADD [Bio] nvarchar(1000) NULL;

ALTER TABLE [CustomerProfiles] ADD [ProfilePictureUrl] nvarchar(500) NULL;

ALTER TABLE [CorporateAccounts] ADD [Bio] nvarchar(1000) NULL;

ALTER TABLE [CorporateAccounts] ADD [ProfilePictureUrl] nvarchar(500) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251029000026_AddUserNotificationPrefs', N'9.0.10');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251029001930_AddUserAndProfileExtras', N'9.0.10');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251029002424_FixMissingUserAndProfileColumns', N'9.0.10');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251029002754_ApplyMissingUserAndProfileColumns', N'9.0.10');

ALTER TABLE [TailorProfiles] ADD [ProfilePictureContentType] nvarchar(100) NULL;

ALTER TABLE [TailorProfiles] ADD [ProfilePictureData] varbinary(max) NULL;

ALTER TABLE [CustomerProfiles] ADD [ProfilePictureContentType] nvarchar(100) NULL;

ALTER TABLE [CustomerProfiles] ADD [ProfilePictureData] varbinary(max) NULL;

ALTER TABLE [CorporateAccounts] ADD [ProfilePictureContentType] nvarchar(100) NULL;

ALTER TABLE [CorporateAccounts] ADD [ProfilePictureData] varbinary(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251029202206_AddProfilePictureBinaryData', N'9.0.10');

ALTER TABLE [Users] ADD [LastLoginAt] datetime2 NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251101050651_AddLastLoginAtColumn', N'9.0.10');

ALTER TABLE [PortfolioImages] ADD [Category] nvarchar(50) NULL;

ALTER TABLE [PortfolioImages] ADD [ContentType] nvarchar(50) NULL;

ALTER TABLE [PortfolioImages] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [PortfolioImages] ADD [Description] nvarchar(500) NULL;

ALTER TABLE [PortfolioImages] ADD [DisplayOrder] int NOT NULL DEFAULT 0;

ALTER TABLE [PortfolioImages] ADD [EstimatedPrice] decimal(18,2) NULL;

ALTER TABLE [PortfolioImages] ADD [ImageData] varbinary(max) NULL;

ALTER TABLE [PortfolioImages] ADD [IsFeatured] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [PortfolioImages] ADD [Title] nvarchar(100) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251101065758_AddPortfolioManagementFeatures', N'9.0.10');

ALTER TABLE [TailorProfiles] ADD [AverageRating] decimal(3,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [TailorProfiles] ADD [BusinessHours] nvarchar(200) NULL;

ALTER TABLE [TailorProfiles] ADD [District] nvarchar(100) NULL;

ALTER TABLE [TailorProfiles] ADD [FacebookUrl] nvarchar(500) NULL;

ALTER TABLE [TailorProfiles] ADD [InstagramUrl] nvarchar(500) NULL;

ALTER TABLE [TailorProfiles] ADD [ShopDescription] nvarchar(500) NULL;

ALTER TABLE [TailorProfiles] ADD [Specialization] nvarchar(200) NULL;

ALTER TABLE [TailorProfiles] ADD [TwitterUrl] nvarchar(500) NULL;

ALTER TABLE [TailorProfiles] ADD [VerifiedAt] datetime2 NULL;

ALTER TABLE [TailorProfiles] ADD [WebsiteUrl] nvarchar(500) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251101073528_AddEditTailorProfileFields', N'9.0.10');

ALTER TABLE [UserActivityLogs] ADD [Details] nvarchar(1000) NULL;

ALTER TABLE [OrderImages] ADD [ContentType] nvarchar(max) NULL;

ALTER TABLE [OrderImages] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [OrderImages] ADD [ImageData] varbinary(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251101102147_AddDetailsToUserActivityLog', N'9.0.10');

ALTER TABLE [Users] ADD [EmailVerificationToken] nvarchar(64) NULL;

ALTER TABLE [Users] ADD [EmailVerificationTokenExpires] datetime2 NULL;

ALTER TABLE [Users] ADD [EmailVerified] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Users] ADD [EmailVerifiedAt] datetime2 NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251101230654_AddEmailVerification', N'9.0.10');

COMMIT;
GO

