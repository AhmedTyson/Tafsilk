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
    CONSTRAINT [FK_TailorBadges_TailorProfiles] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id]) ON DELETE NO ACTION
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
    CONSTRAINT [FK_Reviews_CustomerProfiles] FOREIGN KEY ([CustomerId]) REFERENCES [CustomerProfiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Reviews_CustomerProfiles_CustomerId1] FOREIGN KEY ([CustomerId1]) REFERENCES [CustomerProfiles] ([Id]),
    CONSTRAINT [FK_Reviews_Orders] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Reviews_Orders_OrderId1] FOREIGN KEY ([OrderId1]) REFERENCES [Orders] ([OrderId]),
    CONSTRAINT [FK_Reviews_TailorProfiles] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id]) ON DELETE NO ACTION,
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

ALTER TABLE [DeviceTokens] DROP CONSTRAINT [FK_DeviceTokens_Users];

ALTER TABLE [DeviceTokens] DROP CONSTRAINT [FK_DeviceTokens_Users_UserId1];

ALTER TABLE [Disputes] DROP CONSTRAINT [FK_Disputes_Orders];

ALTER TABLE [Notifications] DROP CONSTRAINT [FK_Notifications_Users];

ALTER TABLE [Notifications] DROP CONSTRAINT [FK_Notifications_Users_UserId1];

ALTER TABLE [OrderImages] DROP CONSTRAINT [FK_OrderImages_Orders_OrderId];

ALTER TABLE [OrderItems] DROP CONSTRAINT [FK_OrderItems_Orders_OrderId];

ALTER TABLE [OrderItems] DROP CONSTRAINT [FK_OrderItems_Orders_OrderId1];

ALTER TABLE [PortfolioImages] DROP CONSTRAINT [FK_PortfolioImages_TailorProfiles];

ALTER TABLE [RevenueReports] DROP CONSTRAINT [FK_RevenueReports_TailorProfiles];

ALTER TABLE [Reviews] DROP CONSTRAINT [FK_Reviews_CustomerProfiles];

ALTER TABLE [Reviews] DROP CONSTRAINT [FK_Reviews_Orders];

ALTER TABLE [Reviews] DROP CONSTRAINT [FK_Reviews_TailorProfiles];

ALTER TABLE [TailorBadges] DROP CONSTRAINT [FK_TailorBadges_TailorProfiles];

ALTER TABLE [TailorServices] DROP CONSTRAINT [FK_TailorServices_TailorProfiles];

ALTER TABLE [TailorServices] DROP CONSTRAINT [FK_TailorServices_TailorProfiles_TailorId1];

ALTER TABLE [UserActivityLogs] DROP CONSTRAINT [FK_UserActivityLogs_Users];

ALTER TABLE [UserActivityLogs] DROP CONSTRAINT [FK_UserActivityLogs_Users_UserId1];

ALTER TABLE [Wallet] DROP CONSTRAINT [FK_Wallet_Users_UserId];

DROP INDEX [IX_UserActivityLogs_Action] ON [UserActivityLogs];

DROP INDEX [IX_UserActivityLogs_CreatedAt] ON [UserActivityLogs];

DROP INDEX [IX_UserActivityLogs_EntityType] ON [UserActivityLogs];

DROP INDEX [IX_UserActivityLogs_User_Action_Date] ON [UserActivityLogs];

DROP INDEX [IX_UserActivityLogs_UserId1] ON [UserActivityLogs];

DROP INDEX [IX_TailorServices_ServiceName] ON [TailorServices];

DROP INDEX [IX_TailorServices_TailorId1] ON [TailorServices];

DROP INDEX [IX_TailorBadges_TailorId] ON [TailorBadges];

DROP INDEX [IX_SystemMessages_AudienceType] ON [SystemMessages];

DROP INDEX [IX_SystemMessages_CreatedAt] ON [SystemMessages];

DROP INDEX [IX_PortfolioImages_IsBeforeAfter] ON [PortfolioImages];

DROP INDEX [IX_PortfolioImages_TailorId_UploadedAt] ON [PortfolioImages];

DROP INDEX [IX_PortfolioImages_UploadedAt] ON [PortfolioImages];

DROP INDEX [IX_OrderItems_OrderId1] ON [OrderItems];

DROP INDEX [IX_Notifications_IsRead] ON [Notifications];

DROP INDEX [IX_Notifications_SentAt] ON [Notifications];

DROP INDEX [IX_Notifications_Type] ON [Notifications];

DROP INDEX [IX_Notifications_User_Read_Date] ON [Notifications];

DROP INDEX [IX_Notifications_UserId1] ON [Notifications];

DROP INDEX [IX_ErrorLogs_CreatedAt] ON [ErrorLogs];

DROP INDEX [IX_ErrorLogs_Severity] ON [ErrorLogs];

DROP INDEX [IX_ErrorLogs_Severity_CreatedAt] ON [ErrorLogs];

DROP INDEX [IX_DeviceTokens_Platform] ON [DeviceTokens];

DROP INDEX [IX_DeviceTokens_Token] ON [DeviceTokens];

DROP INDEX [IX_DeviceTokens_UserId1] ON [DeviceTokens];

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserActivityLogs]') AND [c].[name] = N'UserId1');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [UserActivityLogs] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [UserActivityLogs] DROP COLUMN [UserId1];

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TailorServices]') AND [c].[name] = N'TailorId1');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [TailorServices] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [TailorServices] DROP COLUMN [TailorId1];

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrderItems]') AND [c].[name] = N'OrderId1');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [OrderItems] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [OrderItems] DROP COLUMN [OrderId1];

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Notifications]') AND [c].[name] = N'UserId1');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Notifications] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [Notifications] DROP COLUMN [UserId1];

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeviceTokens]') AND [c].[name] = N'UserId1');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [DeviceTokens] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [DeviceTokens] DROP COLUMN [UserId1];

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserActivityLogs]') AND [c].[name] = N'IsDeleted');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [UserActivityLogs] DROP CONSTRAINT [' + @var16 + '];');
DECLARE @defaultSchema17 AS sysname;
SET @defaultSchema17 = SCHEMA_NAME();
DECLARE @description17 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema17, 'TABLE', N'UserActivityLogs', 'COLUMN', N'IsDeleted';

DECLARE @defaultSchema18 AS sysname;
SET @defaultSchema18 = SCHEMA_NAME();
DECLARE @description18 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema18, 'TABLE', N'UserActivityLogs', 'COLUMN', N'IpAddress';

DECLARE @defaultSchema19 AS sysname;
SET @defaultSchema19 = SCHEMA_NAME();
DECLARE @description19 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema19, 'TABLE', N'UserActivityLogs', 'COLUMN', N'EntityType';

DECLARE @defaultSchema20 AS sysname;
SET @defaultSchema20 = SCHEMA_NAME();
DECLARE @description20 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema20, 'TABLE', N'UserActivityLogs', 'COLUMN', N'Action';

DECLARE @defaultSchema21 AS sysname;
SET @defaultSchema21 = SCHEMA_NAME();
DECLARE @description21 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema21, 'TABLE', N'TailorServices', 'COLUMN', N'ServiceName';

DECLARE @var22 sysname;
SELECT @var22 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TailorServices]') AND [c].[name] = N'IsDeleted');
IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [TailorServices] DROP CONSTRAINT [' + @var22 + '];');
DECLARE @defaultSchema23 AS sysname;
SET @defaultSchema23 = SCHEMA_NAME();
DECLARE @description23 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema23, 'TABLE', N'TailorServices', 'COLUMN', N'IsDeleted';

DECLARE @defaultSchema24 AS sysname;
SET @defaultSchema24 = SCHEMA_NAME();
DECLARE @description24 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema24, 'TABLE', N'TailorServices', 'COLUMN', N'EstimatedDuration';

DECLARE @defaultSchema25 AS sysname;
SET @defaultSchema25 = SCHEMA_NAME();
DECLARE @description25 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema25, 'TABLE', N'TailorServices', 'COLUMN', N'Description';

DECLARE @var26 sysname;
SELECT @var26 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TailorBadges]') AND [c].[name] = N'IsDeleted');
IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [TailorBadges] DROP CONSTRAINT [' + @var26 + '];');
DECLARE @defaultSchema27 AS sysname;
SET @defaultSchema27 = SCHEMA_NAME();
DECLARE @description27 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema27, 'TABLE', N'TailorBadges', 'COLUMN', N'IsDeleted';

DECLARE @var28 sysname;
SELECT @var28 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TailorBadges]') AND [c].[name] = N'EarnedAt');
IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [TailorBadges] DROP CONSTRAINT [' + @var28 + '];');

DECLARE @var29 sysname;
SELECT @var29 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TailorBadges]') AND [c].[name] = N'Description');
IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [TailorBadges] DROP CONSTRAINT [' + @var29 + '];');
ALTER TABLE [TailorBadges] ALTER COLUMN [Description] nvarchar(max) NOT NULL;
DECLARE @defaultSchema30 AS sysname;
SET @defaultSchema30 = SCHEMA_NAME();
DECLARE @description30 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema30, 'TABLE', N'TailorBadges', 'COLUMN', N'Description';

DECLARE @var31 sysname;
SELECT @var31 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TailorBadges]') AND [c].[name] = N'BadgeName');
IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [TailorBadges] DROP CONSTRAINT [' + @var31 + '];');
ALTER TABLE [TailorBadges] ALTER COLUMN [BadgeName] nvarchar(max) NOT NULL;
DECLARE @defaultSchema32 AS sysname;
SET @defaultSchema32 = SCHEMA_NAME();
DECLARE @description32 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema32, 'TABLE', N'TailorBadges', 'COLUMN', N'BadgeName';

DECLARE @var33 sysname;
SELECT @var33 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SystemMessages]') AND [c].[name] = N'Title');
IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [SystemMessages] DROP CONSTRAINT [' + @var33 + '];');
ALTER TABLE [SystemMessages] ALTER COLUMN [Title] nvarchar(max) NOT NULL;
DECLARE @defaultSchema34 AS sysname;
SET @defaultSchema34 = SCHEMA_NAME();
DECLARE @description34 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema34, 'TABLE', N'SystemMessages', 'COLUMN', N'Title';

DECLARE @var35 sysname;
SELECT @var35 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SystemMessages]') AND [c].[name] = N'IsDeleted');
IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [SystemMessages] DROP CONSTRAINT [' + @var35 + '];');
DECLARE @defaultSchema36 AS sysname;
SET @defaultSchema36 = SCHEMA_NAME();
DECLARE @description36 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema36, 'TABLE', N'SystemMessages', 'COLUMN', N'IsDeleted';

DECLARE @var37 sysname;
SELECT @var37 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SystemMessages]') AND [c].[name] = N'CreatedAt');
IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [SystemMessages] DROP CONSTRAINT [' + @var37 + '];');

DECLARE @var38 sysname;
SELECT @var38 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SystemMessages]') AND [c].[name] = N'Content');
IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [SystemMessages] DROP CONSTRAINT [' + @var38 + '];');
ALTER TABLE [SystemMessages] ALTER COLUMN [Content] nvarchar(max) NOT NULL;
DECLARE @defaultSchema39 AS sysname;
SET @defaultSchema39 = SCHEMA_NAME();
DECLARE @description39 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema39, 'TABLE', N'SystemMessages', 'COLUMN', N'Content';

DECLARE @var40 sysname;
SELECT @var40 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SystemMessages]') AND [c].[name] = N'AudienceType');
IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [SystemMessages] DROP CONSTRAINT [' + @var40 + '];');
ALTER TABLE [SystemMessages] ALTER COLUMN [AudienceType] nvarchar(max) NOT NULL;

DECLARE @defaultSchema41 AS sysname;
SET @defaultSchema41 = SCHEMA_NAME();
DECLARE @description41 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema41, 'TABLE', N'Reviews', 'COLUMN', N'Rating';

DECLARE @var42 sysname;
SELECT @var42 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Reviews]') AND [c].[name] = N'IsDeleted');
IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [Reviews] DROP CONSTRAINT [' + @var42 + '];');
DECLARE @defaultSchema43 AS sysname;
SET @defaultSchema43 = SCHEMA_NAME();
DECLARE @description43 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema43, 'TABLE', N'Reviews', 'COLUMN', N'IsDeleted';

DECLARE @defaultSchema44 AS sysname;
SET @defaultSchema44 = SCHEMA_NAME();
DECLARE @description44 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema44, 'TABLE', N'Reviews', 'COLUMN', N'Comment';

DECLARE @var45 sysname;
SELECT @var45 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RevenueReports]') AND [c].[name] = N'IsDeleted');
IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [RevenueReports] DROP CONSTRAINT [' + @var45 + '];');
DECLARE @defaultSchema46 AS sysname;
SET @defaultSchema46 = SCHEMA_NAME();
DECLARE @description46 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema46, 'TABLE', N'RevenueReports', 'COLUMN', N'IsDeleted';

DECLARE @defaultSchema47 AS sysname;
SET @defaultSchema47 = SCHEMA_NAME();
DECLARE @description47 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema47, 'TABLE', N'RatingDimensions', 'COLUMN', N'Score';

DECLARE @var48 sysname;
SELECT @var48 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RatingDimensions]') AND [c].[name] = N'IsDeleted');
IF @var48 IS NOT NULL EXEC(N'ALTER TABLE [RatingDimensions] DROP CONSTRAINT [' + @var48 + '];');
DECLARE @defaultSchema49 AS sysname;
SET @defaultSchema49 = SCHEMA_NAME();
DECLARE @description49 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema49, 'TABLE', N'RatingDimensions', 'COLUMN', N'IsDeleted';

DECLARE @var50 sysname;
SELECT @var50 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RatingDimensions]') AND [c].[name] = N'DimensionName');
IF @var50 IS NOT NULL EXEC(N'ALTER TABLE [RatingDimensions] DROP CONSTRAINT [' + @var50 + '];');
ALTER TABLE [RatingDimensions] ALTER COLUMN [DimensionName] nvarchar(max) NOT NULL;
DECLARE @defaultSchema51 AS sysname;
SET @defaultSchema51 = SCHEMA_NAME();
DECLARE @description51 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema51, 'TABLE', N'RatingDimensions', 'COLUMN', N'DimensionName';

DECLARE @var52 sysname;
SELECT @var52 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PortfolioImages]') AND [c].[name] = N'IsDeleted');
IF @var52 IS NOT NULL EXEC(N'ALTER TABLE [PortfolioImages] DROP CONSTRAINT [' + @var52 + '];');
DECLARE @defaultSchema53 AS sysname;
SET @defaultSchema53 = SCHEMA_NAME();
DECLARE @description53 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema53, 'TABLE', N'PortfolioImages', 'COLUMN', N'IsDeleted';

DECLARE @defaultSchema54 AS sysname;
SET @defaultSchema54 = SCHEMA_NAME();
DECLARE @description54 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema54, 'TABLE', N'PortfolioImages', 'COLUMN', N'IsBeforeAfter';

DECLARE @defaultSchema55 AS sysname;
SET @defaultSchema55 = SCHEMA_NAME();
DECLARE @description55 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema55, 'TABLE', N'Notifications', 'COLUMN', N'Title';

DECLARE @defaultSchema56 AS sysname;
SET @defaultSchema56 = SCHEMA_NAME();
DECLARE @description56 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema56, 'TABLE', N'Notifications', 'COLUMN', N'Message';

DECLARE @defaultSchema57 AS sysname;
SET @defaultSchema57 = SCHEMA_NAME();
DECLARE @description57 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema57, 'TABLE', N'Notifications', 'COLUMN', N'IsDeleted';

DECLARE @var58 sysname;
SELECT @var58 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ErrorLogs]') AND [c].[name] = N'Severity');
IF @var58 IS NOT NULL EXEC(N'ALTER TABLE [ErrorLogs] DROP CONSTRAINT [' + @var58 + '];');
ALTER TABLE [ErrorLogs] ALTER COLUMN [Severity] nvarchar(max) NOT NULL;
DECLARE @defaultSchema59 AS sysname;
SET @defaultSchema59 = SCHEMA_NAME();
DECLARE @description59 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema59, 'TABLE', N'ErrorLogs', 'COLUMN', N'Severity';

DECLARE @var60 sysname;
SELECT @var60 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ErrorLogs]') AND [c].[name] = N'Message');
IF @var60 IS NOT NULL EXEC(N'ALTER TABLE [ErrorLogs] DROP CONSTRAINT [' + @var60 + '];');
ALTER TABLE [ErrorLogs] ALTER COLUMN [Message] nvarchar(max) NOT NULL;
DECLARE @defaultSchema61 AS sysname;
SET @defaultSchema61 = SCHEMA_NAME();
DECLARE @description61 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema61, 'TABLE', N'ErrorLogs', 'COLUMN', N'Message';

DECLARE @var62 sysname;
SELECT @var62 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ErrorLogs]') AND [c].[name] = N'IsDeleted');
IF @var62 IS NOT NULL EXEC(N'ALTER TABLE [ErrorLogs] DROP CONSTRAINT [' + @var62 + '];');
DECLARE @defaultSchema63 AS sysname;
SET @defaultSchema63 = SCHEMA_NAME();
DECLARE @description63 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema63, 'TABLE', N'ErrorLogs', 'COLUMN', N'IsDeleted';

DECLARE @var64 sysname;
SELECT @var64 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ErrorLogs]') AND [c].[name] = N'CreatedAt');
IF @var64 IS NOT NULL EXEC(N'ALTER TABLE [ErrorLogs] DROP CONSTRAINT [' + @var64 + '];');

DECLARE @var65 sysname;
SELECT @var65 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Disputes]') AND [c].[name] = N'Status');
IF @var65 IS NOT NULL EXEC(N'ALTER TABLE [Disputes] DROP CONSTRAINT [' + @var65 + '];');
ALTER TABLE [Disputes] ALTER COLUMN [Status] nvarchar(max) NOT NULL;

DECLARE @var66 sysname;
SELECT @var66 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Disputes]') AND [c].[name] = N'ResolutionDetails');
IF @var66 IS NOT NULL EXEC(N'ALTER TABLE [Disputes] DROP CONSTRAINT [' + @var66 + '];');
ALTER TABLE [Disputes] ALTER COLUMN [ResolutionDetails] nvarchar(max) NULL;

DECLARE @var67 sysname;
SELECT @var67 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Disputes]') AND [c].[name] = N'Reason');
IF @var67 IS NOT NULL EXEC(N'ALTER TABLE [Disputes] DROP CONSTRAINT [' + @var67 + '];');
ALTER TABLE [Disputes] ALTER COLUMN [Reason] nvarchar(max) NOT NULL;

DECLARE @var68 sysname;
SELECT @var68 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Disputes]') AND [c].[name] = N'Description');
IF @var68 IS NOT NULL EXEC(N'ALTER TABLE [Disputes] DROP CONSTRAINT [' + @var68 + '];');
ALTER TABLE [Disputes] ALTER COLUMN [Description] nvarchar(max) NOT NULL;

DECLARE @var69 sysname;
SELECT @var69 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Disputes]') AND [c].[name] = N'CreatedAt');
IF @var69 IS NOT NULL EXEC(N'ALTER TABLE [Disputes] DROP CONSTRAINT [' + @var69 + '];');

DECLARE @var70 sysname;
SELECT @var70 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Disputes]') AND [c].[name] = N'Id');
IF @var70 IS NOT NULL EXEC(N'ALTER TABLE [Disputes] DROP CONSTRAINT [' + @var70 + '];');

DECLARE @var71 sysname;
SELECT @var71 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeviceTokens]') AND [c].[name] = N'IsDeleted');
IF @var71 IS NOT NULL EXEC(N'ALTER TABLE [DeviceTokens] DROP CONSTRAINT [' + @var71 + '];');
DECLARE @defaultSchema72 AS sysname;
SET @defaultSchema72 = SCHEMA_NAME();
DECLARE @description72 AS sql_variant;
EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', @defaultSchema72, 'TABLE', N'DeviceTokens', 'COLUMN', N'IsDeleted';

ALTER TABLE [DeviceTokens] ADD CONSTRAINT [FK_DeviceTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Disputes] ADD CONSTRAINT [FK_Disputes_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]) ON DELETE CASCADE;

ALTER TABLE [Notifications] ADD CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;

ALTER TABLE [OrderImages] ADD CONSTRAINT [FK_OrderImages_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]) ON DELETE CASCADE;

ALTER TABLE [OrderItems] ADD CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]) ON DELETE CASCADE;

ALTER TABLE [PortfolioImages] ADD CONSTRAINT [FK_PortfolioImages_TailorProfiles_TailorId] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id]) ON DELETE CASCADE;

ALTER TABLE [RevenueReports] ADD CONSTRAINT [FK_RevenueReports_TailorProfiles_TailorId] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Reviews] ADD CONSTRAINT [FK_Reviews_CustomerProfiles_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [CustomerProfiles] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Reviews] ADD CONSTRAINT [FK_Reviews_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]) ON DELETE NO ACTION;

ALTER TABLE [Reviews] ADD CONSTRAINT [FK_Reviews_TailorProfiles_TailorId] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [TailorServices] ADD CONSTRAINT [FK_TailorServices_TailorProfiles_TailorId] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id]) ON DELETE CASCADE;

ALTER TABLE [UserActivityLogs] ADD CONSTRAINT [FK_UserActivityLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Wallet] ADD CONSTRAINT [FK_Wallet_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251031205115_FixTailorServiceFk', N'9.0.10');

ALTER TABLE [PortfolioImages] DROP CONSTRAINT [FK_PortfolioImages_TailorProfiles_TailorId1];

DROP INDEX [IX_PortfolioImages_TailorId1] ON [PortfolioImages];

DECLARE @var73 sysname;
SELECT @var73 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PortfolioImages]') AND [c].[name] = N'TailorId1');
IF @var73 IS NOT NULL EXEC(N'ALTER TABLE [PortfolioImages] DROP CONSTRAINT [' + @var73 + '];');
ALTER TABLE [PortfolioImages] DROP COLUMN [TailorId1];

DECLARE @var74 sysname;
SELECT @var74 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Reviews]') AND [c].[name] = N'IsDeleted');
IF @var74 IS NOT NULL EXEC(N'ALTER TABLE [Reviews] DROP CONSTRAINT [' + @var74 + '];');
ALTER TABLE [Reviews] ADD DEFAULT CAST(0 AS bit) FOR [IsDeleted];

DECLARE @var75 sysname;
SELECT @var75 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RevenueReports]') AND [c].[name] = N'IsDeleted');
IF @var75 IS NOT NULL EXEC(N'ALTER TABLE [RevenueReports] DROP CONSTRAINT [' + @var75 + '];');
ALTER TABLE [RevenueReports] ADD DEFAULT CAST(0 AS bit) FOR [IsDeleted];

DECLARE @var76 sysname;
SELECT @var76 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PortfolioImages]') AND [c].[name] = N'IsDeleted');
IF @var76 IS NOT NULL EXEC(N'ALTER TABLE [PortfolioImages] DROP CONSTRAINT [' + @var76 + '];');
ALTER TABLE [PortfolioImages] ADD DEFAULT CAST(0 AS bit) FOR [IsDeleted];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251031210106_FixCascadeAndShadowFks', N'9.0.10');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251031211110_ApplyNoActionToOrderChildren', N'9.0.10');

COMMIT;
GO

