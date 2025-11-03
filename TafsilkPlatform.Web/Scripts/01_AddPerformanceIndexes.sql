-- Performance Optimization Indexes for Tafsilk Platform
-- Run this script after applying migrations

USE [TafsilkPlatformDb_Dev];
GO

-- Index 1: Email Verification Token Lookup
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_EmailVerificationToken' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Users_EmailVerificationToken] 
    ON [Users]([EmailVerificationToken])
    WHERE [EmailVerificationToken] IS NOT NULL;
    PRINT 'Created index: IX_Users_EmailVerificationToken';
END
GO

-- Index 2: Active Users Filter
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_IsActive_IsDeleted' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Users_IsActive_IsDeleted] 
    ON [Users]([IsActive], [IsDeleted])
    INCLUDE ([Email], [RoleId]);
    PRINT 'Created index: IX_Users_IsActive_IsDeleted';
END
GO

-- Index 3: Tailor Verification Status
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TailorProfiles_UserId_IsVerified' AND object_id = OBJECT_ID('TailorProfiles'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_TailorProfiles_UserId_IsVerified] 
    ON [TailorProfiles]([UserId], [IsVerified]);
    PRINT 'Created index: IX_TailorProfiles_UserId_IsVerified';
END
GO

-- Index 4: Corporate Approval Status
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CorporateAccounts_UserId_IsApproved' AND object_id = OBJECT_ID('CorporateAccounts'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CorporateAccounts_UserId_IsApproved] 
    ON [CorporateAccounts]([UserId], [IsApproved]);
    PRINT 'Created index: IX_CorporateAccounts_UserId_IsApproved';
END
GO

-- Index 5: Customer Orders by Status (FIXED: Use TotalPrice instead of TotalAmount)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_CustomerId_Status' AND object_id = OBJECT_ID('Orders'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Orders_CustomerId_Status] 
    ON [Orders]([CustomerId], [Status])
    INCLUDE ([CreatedAt], [TotalPrice]);
    PRINT 'Created index: IX_Orders_CustomerId_Status';
END
GO

-- Index 6: Tailor Orders by Status (FIXED: Use TotalPrice instead of TotalAmount)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_TailorId_Status' AND object_id = OBJECT_ID('Orders'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Orders_TailorId_Status] 
  ON [Orders]([TailorId], [Status])
    INCLUDE ([CreatedAt], [TotalPrice]);
    PRINT 'Created index: IX_Orders_TailorId_Status';
END
GO

-- Index 7: Unread Notifications
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notifications_UserId_IsRead' AND object_id = OBJECT_ID('Notifications'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Notifications_UserId_IsRead] 
    ON [Notifications]([UserId], [IsRead])
    WHERE [IsDeleted] = 0;
    PRINT 'Created index: IX_Notifications_UserId_IsRead';
END
GO

-- Index 8: Tailor Reviews and Ratings
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reviews_TailorId_CreatedAt' AND object_id = OBJECT_ID('Reviews'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Reviews_TailorId_CreatedAt] 
ON [Reviews]([TailorId], [CreatedAt] DESC)
    INCLUDE ([Rating])
    WHERE [IsDeleted] = 0;
    PRINT 'Created index: IX_Reviews_TailorId_CreatedAt';
END
GO

-- Index 9: Refresh Token Cleanup (FIXED: Use RevokedAt IS NULL instead of IsRevoked = 0)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RefreshTokens_UserId_ExpiresAt' AND object_id = OBJECT_ID('RefreshTokens'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_UserId_ExpiresAt] 
    ON [RefreshTokens]([UserId], [ExpiresAt] DESC)
    WHERE [RevokedAt] IS NULL;
    PRINT 'Created index: IX_RefreshTokens_UserId_ExpiresAt';
END
GO

-- Index 10: User Activity History
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLogs_UserId_CreatedAt' AND object_id = OBJECT_ID('ActivityLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ActivityLogs_UserId_CreatedAt] 
    ON [ActivityLogs]([UserId], [CreatedAt] DESC);
    PRINT 'Created index: IX_ActivityLogs_UserId_CreatedAt';
END
GO

PRINT 'Performance indexes creation completed!';
