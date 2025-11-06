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
    [Severity] nvarchar(50) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ErrorLogs] PRIMARY KEY ([ErrorLogId])
);

CREATE TABLE [Roles] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(255) NULL,
    [Permissions] nvarchar(2000) NULL,
    [Priority] int NOT NULL DEFAULT 0,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__Roles__3214EC07CB85E41E] PRIMARY KEY ([Id])
);

CREATE TABLE [TailorBadges] (
    [TailorBadgeId] uniqueidentifier NOT NULL,
    [TailorId] uniqueidentifier NOT NULL,
    [BadgeName] nvarchar(max) NOT NULL,
    [EarnedAt] datetime2 NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_TailorBadges] PRIMARY KEY ([TailorBadgeId])
);

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [Email] nvarchar(255) NOT NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NULL,
    [LastLoginAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [EmailVerified] bit NOT NULL,
    [EmailVerificationToken] nvarchar(64) NULL,
    [EmailVerificationTokenExpires] datetime2 NULL,
    [EmailVerifiedAt] datetime2 NULL,
    [PasswordResetToken] nvarchar(64) NULL,
    [PasswordResetTokenExpires] datetime2 NULL,
    [EmailNotifications] bit NOT NULL DEFAULT CAST(1 AS bit),
    [SmsNotifications] bit NOT NULL DEFAULT CAST(1 AS bit),
    [PromotionalNotifications] bit NOT NULL DEFAULT CAST(1 AS bit),
    [BannedAt] datetime2 NULL,
    [BanReason] nvarchar(500) NULL,
    [BanExpiresAt] datetime2 NULL,
    CONSTRAINT [PK__Users__3214EC07AA820590] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Users_Roles] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id])
);

CREATE TABLE [ActivityLogs] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Action] nvarchar(100) NOT NULL,
    [EntityType] nvarchar(50) NOT NULL,
    [EntityId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [IpAddress] nvarchar(45) NULL,
    [Details] nvarchar(1000) NULL,
    [IsAdminAction] bit NOT NULL DEFAULT CAST(0 AS bit),
    [IsDeleted] bit NOT NULL,
    [Discriminator] nvarchar(21) NOT NULL,
    [UserActivityLogId] uniqueidentifier NULL,
    CONSTRAINT [PK_ActivityLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ActivityLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [CorporateAccounts] (
    [Id] uniqueidentifier NOT NULL DEFAULT ((newid())),
    [UserId] uniqueidentifier NOT NULL,
    [CompanyName] nvarchar(255) NOT NULL,
    [ContactPerson] nvarchar(255) NOT NULL,
    [Industry] nvarchar(100) NULL,
    [TaxNumber] nvarchar(100) NULL,
    [Bio] nvarchar(1000) NULL,
    [ProfilePictureUrl] nvarchar(500) NULL,
    [ProfilePictureData] varbinary(max) NULL,
    [ProfilePictureContentType] nvarchar(100) NULL,
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
    [Bio] nvarchar(1000) NULL,
    [ProfilePictureUrl] nvarchar(500) NULL,
    [ProfilePictureData] varbinary(max) NULL,
    [ProfilePictureContentType] nvarchar(100) NULL,
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
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_DeviceTokens] PRIMARY KEY ([DeviceTokenId]),
    CONSTRAINT [FK_DeviceTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [Notifications] (
    [NotificationId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NULL,
    [AudienceType] nvarchar(50) NULL,
    [Title] nvarchar(200) NOT NULL,
    [Message] nvarchar(2000) NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [IsRead] bit NOT NULL DEFAULT CAST(0 AS bit),
    [SentAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [ExpiresAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationId]),
    CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);

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
    [FullName] nvarchar(255) NULL,
    [ShopName] nvarchar(255) NOT NULL,
    [ShopDescription] nvarchar(500) NULL,
    [Specialization] nvarchar(200) NULL,
    [Address] nvarchar(500) NOT NULL,
    [City] nvarchar(100) NULL,
    [District] nvarchar(100) NULL,
    [Latitude] decimal(10,8) NULL,
    [Longitude] decimal(11,8) NULL,
    [ExperienceYears] int NULL,
    [PricingRange] nvarchar(100) NULL,
    [Bio] nvarchar(1000) NULL,
    [BusinessHours] nvarchar(200) NULL,
    [FacebookUrl] nvarchar(500) NULL,
    [InstagramUrl] nvarchar(500) NULL,
    [TwitterUrl] nvarchar(500) NULL,
    [WebsiteUrl] nvarchar(500) NULL,
    [ProfilePictureUrl] nvarchar(500) NULL,
    [ProfilePictureData] varbinary(max) NULL,
    [ProfilePictureContentType] nvarchar(100) NULL,
    [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit),
    [VerifiedAt] datetime2 NULL,
    [AverageRating] decimal(3,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK__TailorPr__3214EC07A3FCF42C] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TailorProfiles_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);

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
    CONSTRAINT [FK_RFQs_CorporateAccounts_CorporateAccountId] FOREIGN KEY ([CorporateAccountId]) REFERENCES [CorporateAccounts] ([Id])
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
    [Title] nvarchar(100) NULL,
    [Category] nvarchar(50) NULL,
    [Description] nvarchar(500) NULL,
    [ImageUrl] nvarchar(500) NULL,
    [ImageData] varbinary(max) NULL,
    [ContentType] nvarchar(50) NULL,
    [EstimatedPrice] decimal(18,2) NULL,
    [IsBeforeAfter] bit NOT NULL,
    [IsFeatured] bit NOT NULL,
    [DisplayOrder] int NOT NULL,
    [UploadedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [CreatedAt] datetime2 NOT NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_PortfolioImages] PRIMARY KEY ([PortfolioImageId]),
    CONSTRAINT [FK_PortfolioImages_TailorProfiles_TailorId] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id])
);

CREATE TABLE [TailorServices] (
    [TailorServiceId] uniqueidentifier NOT NULL,
    [TailorId] uniqueidentifier NOT NULL,
    [ServiceName] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [BasePrice] decimal(18,2) NOT NULL,
    [EstimatedDuration] int NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_TailorServices] PRIMARY KEY ([TailorServiceId]),
    CONSTRAINT [FK_TailorServices_TailorProfiles_TailorId] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id])
);

CREATE TABLE [Contracts] (
    [Id] uniqueidentifier NOT NULL,
    [RFQId] uniqueidentifier NOT NULL,
    [TailorId] uniqueidentifier NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [ContractStatus] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Contracts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Contracts_RFQs_RFQId] FOREIGN KEY ([RFQId]) REFERENCES [RFQs] ([Id]),
    CONSTRAINT [FK_Contracts_TailorProfiles_TailorId] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id])
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
    CONSTRAINT [FK_RFQBids_RFQs_RFQId] FOREIGN KEY ([RFQId]) REFERENCES [RFQs] ([Id]),
    CONSTRAINT [FK_RFQBids_TailorProfiles_TailorId] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id])
);

CREATE TABLE [Disputes] (
    [Id] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NOT NULL,
    [OpenedByUserId] uniqueidentifier NOT NULL,
    [Reason] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [ResolvedByAdminId] uniqueidentifier NULL,
    [ResolutionDetails] nvarchar(max) NULL,
    [ResolvedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Disputes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Disputes_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]),
    CONSTRAINT [FK_Disputes_Users_OpenedByUserId] FOREIGN KEY ([OpenedByUserId]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_Disputes_Users_ResolvedByAdminId] FOREIGN KEY ([ResolvedByAdminId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [OrderImages] (
    [OrderImageId] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NOT NULL,
    [ImageData] varbinary(max) NULL,
    [ContentType] nvarchar(max) NULL,
    [ImgUrl] nvarchar(max) NOT NULL,
    [UploadedId] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
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
    CONSTRAINT [FK_RefundRequests_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]),
    CONSTRAINT [FK_RefundRequests_Users_RequestedBy] FOREIGN KEY ([RequestedBy]) REFERENCES [Users] ([Id])
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
    [CustomerProfileId] uniqueidentifier NULL,
    [TailorProfileId] uniqueidentifier NULL,
    CONSTRAINT [PK_Reviews] PRIMARY KEY ([ReviewId]),
    CONSTRAINT [FK_Reviews_CustomerProfiles_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [CustomerProfiles] ([Id]),
    CONSTRAINT [FK_Reviews_CustomerProfiles_CustomerProfileId] FOREIGN KEY ([CustomerProfileId]) REFERENCES [CustomerProfiles] ([Id]),
    CONSTRAINT [FK_Reviews_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]),
    CONSTRAINT [FK_Reviews_TailorProfiles_TailorId] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id]),
    CONSTRAINT [FK_Reviews_TailorProfiles_TailorProfileId] FOREIGN KEY ([TailorProfileId]) REFERENCES [TailorProfiles] ([Id])
);

CREATE TABLE [RatingDimensions] (
    [RatingDimensionId] uniqueidentifier NOT NULL,
    [ReviewId] uniqueidentifier NOT NULL,
    [DimensionName] nvarchar(max) NOT NULL,
    [Score] int NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_RatingDimensions] PRIMARY KEY ([RatingDimensionId]),
    CONSTRAINT [FK_RatingDimensions_Reviews_ReviewId] FOREIGN KEY ([ReviewId]) REFERENCES [Reviews] ([ReviewId])
);

CREATE INDEX [IX_ActivityLogs_IsAdminAction] ON [ActivityLogs] ([IsAdminAction]);

CREATE INDEX [IX_ActivityLogs_UserId] ON [ActivityLogs] ([UserId]);

CREATE INDEX [IX_Contracts_RFQId] ON [Contracts] ([RFQId]);

CREATE INDEX [IX_Contracts_TailorId] ON [Contracts] ([TailorId]);

CREATE INDEX [IX_CorporateAccounts_UserId] ON [CorporateAccounts] ([UserId]);

CREATE UNIQUE INDEX [UQ__Corporat__1788CC4D39440DF8] ON [CorporateAccounts] ([UserId]);

CREATE INDEX [IX_CustomerProfiles_UserId] ON [CustomerProfiles] ([UserId]);

CREATE UNIQUE INDEX [UQ__Customer__1788CC4D90808B91] ON [CustomerProfiles] ([UserId]);

CREATE INDEX [IX_DeviceTokens_UserId] ON [DeviceTokens] ([UserId]);

CREATE INDEX [IX_Disputes_OpenedByUserId] ON [Disputes] ([OpenedByUserId]);

CREATE INDEX [IX_Disputes_OrderId] ON [Disputes] ([OrderId]);

CREATE INDEX [IX_Disputes_ResolvedByAdminId] ON [Disputes] ([ResolvedByAdminId]);

CREATE INDEX [IX_Notifications_AudienceType] ON [Notifications] ([AudienceType]);

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);

CREATE INDEX [IX_OrderImages_OrderId] ON [OrderImages] ([OrderId]);

CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);

CREATE INDEX [IX_Orders_CustomerId] ON [Orders] ([CustomerId]);

CREATE INDEX [IX_Orders_TailorId] ON [Orders] ([TailorId]);

CREATE INDEX [IX_Payment_CustomerId] ON [Payment] ([CustomerId]);

CREATE INDEX [IX_Payment_OrderId] ON [Payment] ([OrderId]);

CREATE INDEX [IX_Payment_TailorId] ON [Payment] ([TailorId]);

CREATE INDEX [IX_PortfolioImages_TailorId] ON [PortfolioImages] ([TailorId]);

CREATE INDEX [IX_Quotes_OrderId] ON [Quotes] ([OrderId]);

CREATE INDEX [IX_Quotes_TailorId] ON [Quotes] ([TailorId]);

CREATE INDEX [IX_RatingDimensions_ReviewId] ON [RatingDimensions] ([ReviewId]);

CREATE INDEX [IX_RefreshTokens_ExpiresAt] ON [RefreshTokens] ([ExpiresAt]);

CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);

CREATE INDEX [IX_RefundRequests_OrderId] ON [RefundRequests] ([OrderId]);

CREATE INDEX [IX_RefundRequests_RequestedBy] ON [RefundRequests] ([RequestedBy]);

CREATE INDEX [IX_Reviews_CustomerId] ON [Reviews] ([CustomerId]);

CREATE INDEX [IX_Reviews_CustomerProfileId] ON [Reviews] ([CustomerProfileId]);

CREATE INDEX [IX_Reviews_OrderId] ON [Reviews] ([OrderId]);

CREATE INDEX [IX_Reviews_TailorId] ON [Reviews] ([TailorId]);

CREATE INDEX [IX_Reviews_TailorProfileId] ON [Reviews] ([TailorProfileId]);

CREATE INDEX [IX_RFQBids_RFQId] ON [RFQBids] ([RFQId]);

CREATE INDEX [IX_RFQBids_TailorId] ON [RFQBids] ([TailorId]);

CREATE INDEX [IX_RFQs_CorporateAccountId] ON [RFQs] ([CorporateAccountId]);

CREATE INDEX [IX_TailorProfiles_UserId] ON [TailorProfiles] ([UserId]);

CREATE UNIQUE INDEX [UQ__TailorPr__1788CC4D37A4BF4A] ON [TailorProfiles] ([UserId]);

CREATE INDEX [IX_TailorServices_TailorId] ON [TailorServices] ([TailorId]);

CREATE INDEX [IX_UserAddresses_UserId] ON [UserAddresses] ([UserId]);

CREATE INDEX [IX_Users_Email] ON [Users] ([Email]);

CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);

CREATE UNIQUE INDEX [UQ__Users__A9D10534975288F0] ON [Users] ([Email]);

CREATE UNIQUE INDEX [IX_Wallet_UserId] ON [Wallet] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251103155326_AddPasswordResetFieldsToUsers', N'9.0.10');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251103160056_dbnew', N'9.0.10');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251103163237_Accountcontroller_fix', N'9.0.10');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251104034807_TafsilkPlatformDb_Dev_Tailor_FIX', N'9.0.10');

EXEC sp_rename N'[Orders].[Discription]', N'Description', 'COLUMN';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251105005924_FixOrderDescriptionTypo', N'9.0.10');

DROP TABLE [ActivityLogs];

DROP TABLE [Contracts];

DROP TABLE [DeviceTokens];

DROP TABLE [Disputes];

DROP TABLE [ErrorLogs];

DROP TABLE [Quotes];

DROP TABLE [RefundRequests];

DROP TABLE [RFQBids];

DROP TABLE [TailorBadges];

DROP TABLE [Wallet];

DROP TABLE [RFQs];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251105015406_asyncfix', N'9.0.10');

DROP TABLE [CorporateAccounts];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251105023951_RemoveCorporateFeature', N'9.0.10');

CREATE TABLE [TailorVerifications] (
    [Id] uniqueidentifier NOT NULL,
    [TailorProfileId] uniqueidentifier NOT NULL,
    [NationalIdNumber] nvarchar(50) NOT NULL,
    [FullLegalName] nvarchar(200) NOT NULL,
    [Nationality] nvarchar(100) NULL,
    [DateOfBirth] datetime2 NULL,
    [CommercialRegistrationNumber] nvarchar(100) NULL,
    [ProfessionalLicenseNumber] nvarchar(100) NULL,
    [IdDocumentFrontData] varbinary(max) NOT NULL,
    [IdDocumentFrontContentType] nvarchar(100) NULL,
    [IdDocumentBackData] varbinary(max) NULL,
    [IdDocumentBackContentType] nvarchar(100) NULL,
    [CommercialRegistrationData] varbinary(max) NULL,
    [CommercialRegistrationContentType] nvarchar(100) NULL,
    [ProfessionalLicenseData] varbinary(max) NULL,
    [ProfessionalLicenseContentType] nvarchar(100) NULL,
    [Status] int NOT NULL DEFAULT 0,
    [SubmittedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [ReviewedAt] datetime2 NULL,
    [ReviewedByAdminId] uniqueidentifier NULL,
    [ReviewNotes] nvarchar(1000) NULL,
    [RejectionReason] nvarchar(1000) NULL,
    [AdditionalNotes] nvarchar(500) NULL,
    CONSTRAINT [PK_TailorVerifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TailorVerifications_TailorProfiles_TailorProfileId] FOREIGN KEY ([TailorProfileId]) REFERENCES [TailorProfiles] ([Id]),
    CONSTRAINT [FK_TailorVerifications_Users_ReviewedByAdminId] FOREIGN KEY ([ReviewedByAdminId]) REFERENCES [Users] ([Id])
);

CREATE INDEX [IX_TailorVerifications_ReviewedByAdminId] ON [TailorVerifications] ([ReviewedByAdminId]);

CREATE INDEX [IX_TailorVerifications_Status] ON [TailorVerifications] ([Status]);

CREATE UNIQUE INDEX [IX_TailorVerifications_TailorProfileId] ON [TailorVerifications] ([TailorProfileId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251106105417_AddTailorVerificationTable', N'9.0.10');

CREATE TABLE [IdempotencyKeys] (
    [Key] nvarchar(128) NOT NULL,
    [Status] int NOT NULL DEFAULT 0,
    [ResponseJson] nvarchar(max) NULL,
    [StatusCode] int NULL,
    [ContentType] nvarchar(100) NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [LastAccessedAtUtc] datetime2 NULL,
    [ExpiresAtUtc] datetime2 NOT NULL,
    [UserId] uniqueidentifier NULL,
    [Endpoint] nvarchar(500) NULL,
    [Method] nvarchar(10) NULL,
    [ErrorMessage] nvarchar(max) NULL,
    CONSTRAINT [PK_IdempotencyKeys] PRIMARY KEY ([Key])
);

CREATE INDEX [IX_IdempotencyKeys_ExpiresAtUtc] ON [IdempotencyKeys] ([ExpiresAtUtc]);

CREATE INDEX [IX_IdempotencyKeys_Status] ON [IdempotencyKeys] ([Status]);

CREATE INDEX [IX_IdempotencyKeys_UserId] ON [IdempotencyKeys] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251106184011_AddIdempotencyAndEnhancements', N'9.0.10');

ALTER TABLE [Orders] ADD [DeliveryAddress] nvarchar(max) NULL;

ALTER TABLE [Orders] ADD [DepositAmount] float NULL;

ALTER TABLE [Orders] ADD [DepositPaid] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Orders] ADD [DepositPaidAt] datetimeoffset NULL;

ALTER TABLE [Orders] ADD [FulfillmentMethod] nvarchar(20) NULL;

ALTER TABLE [Orders] ADD [MeasurementsJson] nvarchar(max) NULL;

ALTER TABLE [Orders] ADD [QuoteProvidedAt] datetimeoffset NULL;

ALTER TABLE [Orders] ADD [RequiresDeposit] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Orders] ADD [TailorQuote] float NULL;

ALTER TABLE [Orders] ADD [TailorQuoteNotes] nvarchar(max) NULL;

CREATE TABLE [Complaints] (
    [Id] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [TailorId] uniqueidentifier NOT NULL,
    [Subject] nvarchar(100) NOT NULL,
    [Description] nvarchar(2000) NOT NULL,
    [ComplaintType] nvarchar(50) NOT NULL DEFAULT N'Other',
    [DesiredResolution] nvarchar(50) NULL,
    [Status] nvarchar(50) NOT NULL DEFAULT N'Open',
    [Priority] nvarchar(20) NOT NULL DEFAULT N'Medium',
    [AdminResponse] nvarchar(2000) NULL,
    [ResolvedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NULL,
    [ResolvedAt] datetime2 NULL,
    CONSTRAINT [PK_Complaints] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Complaints_CustomerProfiles_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [CustomerProfiles] ([Id]),
    CONSTRAINT [FK_Complaints_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]),
    CONSTRAINT [FK_Complaints_TailorProfiles_TailorId] FOREIGN KEY ([TailorId]) REFERENCES [TailorProfiles] ([Id])
);

CREATE TABLE [CustomerLoyalty] (
    [Id] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [Points] int NOT NULL,
    [LifetimePoints] int NOT NULL,
    [Tier] nvarchar(50) NOT NULL DEFAULT N'Bronze',
    [TotalOrders] int NOT NULL,
    [ReferralsCount] int NOT NULL,
    [ReferralCode] nvarchar(20) NULL,
    [ReferredBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_CustomerLoyalty] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerLoyalty_CustomerProfiles_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [CustomerProfiles] ([Id])
);

CREATE TABLE [CustomerMeasurements] (
    [Id] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [GarmentType] nvarchar(50) NULL,
    [Chest] decimal(5,2) NULL,
    [Waist] decimal(5,2) NULL,
    [Hips] decimal(5,2) NULL,
    [ShoulderWidth] decimal(5,2) NULL,
    [SleeveLength] decimal(5,2) NULL,
    [InseamLength] decimal(5,2) NULL,
    [OutseamLength] decimal(5,2) NULL,
    [NeckCircumference] decimal(5,2) NULL,
    [ArmLength] decimal(5,2) NULL,
    [ThighCircumference] decimal(5,2) NULL,
    [ThobeLength] decimal(5,2) NULL,
    [AbayaLength] decimal(5,2) NULL,
    [CustomMeasurementsJson] nvarchar(2000) NULL,
    [Notes] nvarchar(500) NULL,
    [IsDefault] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_CustomerMeasurements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerMeasurements_CustomerProfiles_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [CustomerProfiles] ([Id])
);

CREATE TABLE [ComplaintAttachments] (
    [Id] uniqueidentifier NOT NULL,
    [ComplaintId] uniqueidentifier NOT NULL,
    [FileData] varbinary(max) NULL,
    [ContentType] nvarchar(100) NULL,
    [FileName] nvarchar(255) NULL,
    [UploadedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK_ComplaintAttachments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ComplaintAttachments_Complaints_ComplaintId] FOREIGN KEY ([ComplaintId]) REFERENCES [Complaints] ([Id])
);

CREATE TABLE [LoyaltyTransactions] (
    [Id] uniqueidentifier NOT NULL,
    [CustomerLoyaltyId] uniqueidentifier NOT NULL,
    [Points] int NOT NULL,
    [Type] nvarchar(20) NOT NULL,
    [Description] nvarchar(200) NULL,
    [RelatedOrderId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK_LoyaltyTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LoyaltyTransactions_CustomerLoyalty_CustomerLoyaltyId] FOREIGN KEY ([CustomerLoyaltyId]) REFERENCES [CustomerLoyalty] ([Id])
);

CREATE INDEX [IX_ComplaintAttachments_ComplaintId] ON [ComplaintAttachments] ([ComplaintId]);

CREATE INDEX [IX_Complaints_CustomerId] ON [Complaints] ([CustomerId]);

CREATE INDEX [IX_Complaints_OrderId] ON [Complaints] ([OrderId]);

CREATE INDEX [IX_Complaints_Status] ON [Complaints] ([Status]);

CREATE INDEX [IX_Complaints_TailorId] ON [Complaints] ([TailorId]);

CREATE UNIQUE INDEX [IX_CustomerLoyalty_CustomerId] ON [CustomerLoyalty] ([CustomerId]);

CREATE INDEX [IX_CustomerLoyalty_ReferralCode] ON [CustomerLoyalty] ([ReferralCode]);

CREATE INDEX [IX_CustomerMeasurements_CustomerId] ON [CustomerMeasurements] ([CustomerId]);

CREATE INDEX [IX_LoyaltyTransactions_CreatedAt] ON [LoyaltyTransactions] ([CreatedAt]);

CREATE INDEX [IX_LoyaltyTransactions_CustomerLoyaltyId] ON [LoyaltyTransactions] ([CustomerLoyaltyId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251109003252_AddLoyaltyComplaintsAndMeasurements', N'9.0.10');

COMMIT;
GO

