-- ========================================
-- Migration: Add Password Reset Fields to Users Table
-- Date: 2024
-- Description: Adds PasswordResetToken and PasswordResetTokenExpires
--     fields to support password reset functionality
-- ========================================

USE TafsilkPlatformDb;
GO

-- Check if columns already exist before adding
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
             WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'PasswordResetToken')
BEGIN
    PRINT 'Adding PasswordResetToken column...';
    ALTER TABLE Users
    ADD PasswordResetToken NVARCHAR(64) NULL;
    PRINT '✓ PasswordResetToken column added successfully';
END
ELSE
BEGIN
    PRINT '✓ PasswordResetToken column already exists';
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'PasswordResetTokenExpires')
BEGIN
 PRINT 'Adding PasswordResetTokenExpires column...';
    ALTER TABLE Users
    ADD PasswordResetTokenExpires DATETIME2 NULL;
    PRINT '✓ PasswordResetTokenExpires column added successfully';
END
ELSE
BEGIN
    PRINT '✓ PasswordResetTokenExpires column already exists';
END
GO

-- Create index for performance (only if not exists)
IF NOT EXISTS (SELECT * FROM sys.indexes 
  WHERE name = 'IX_Users_PasswordResetToken' AND object_id = OBJECT_ID('Users'))
BEGIN
    PRINT 'Creating index on PasswordResetToken...';
    CREATE INDEX IX_Users_PasswordResetToken 
    ON Users(PasswordResetToken) 
    WHERE PasswordResetToken IS NOT NULL;
    PRINT '✓ Index created successfully';
END
ELSE
BEGIN
    PRINT '✓ Index already exists';
END
GO

-- Verify changes
PRINT '';
PRINT '=== Migration Verification ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users' 
    AND COLUMN_NAME IN ('PasswordResetToken', 'PasswordResetTokenExpires')
ORDER BY COLUMN_NAME;
GO

PRINT '';
PRINT '✓ Migration completed successfully!';
PRINT '  - PasswordResetToken (NVARCHAR(64), NULL)';
PRINT '  - PasswordResetTokenExpires (DATETIME2, NULL)';
PRINT '  - Index IX_Users_PasswordResetToken created';
GO
