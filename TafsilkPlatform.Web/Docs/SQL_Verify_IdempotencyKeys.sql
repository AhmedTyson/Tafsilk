-- ========================================
-- IDEMPOTENCY TABLE VERIFICATION SCRIPT
-- ========================================
-- Run this script in SQL Server Management Studio or Azure Data Studio
-- to verify the IdempotencyKeys table was created correctly

USE TafsilkPlatform;
GO

-- ========================================
-- 1. CHECK TABLE EXISTS
-- ========================================
PRINT '=== 1. Checking if IdempotencyKeys table exists ==='
IF OBJECT_ID('IdempotencyKeys', 'U') IS NOT NULL
    PRINT '✅ IdempotencyKeys table EXISTS'
ELSE
 PRINT '❌ IdempotencyKeys table DOES NOT EXIST'
GO

-- ========================================
-- 2. VIEW TABLE STRUCTURE
-- ========================================
PRINT ''
PRINT '=== 2. Table Structure ==='
SELECT 
    COLUMN_NAME as [Column Name],
    DATA_TYPE as [Data Type],
    CHARACTER_MAXIMUM_LENGTH as [Max Length],
    IS_NULLABLE as [Nullable],
    COLUMN_DEFAULT as [Default Value]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'IdempotencyKeys'
ORDER BY ORDINAL_POSITION;
GO

-- ========================================
-- 3. VIEW INDEXES
-- ========================================
PRINT ''
PRINT '=== 3. Indexes on IdempotencyKeys ==='
SELECT 
    i.name as [Index Name],
    i.type_desc as [Index Type],
    i.is_unique as [Is Unique],
    COL_NAME(ic.object_id, ic.column_id) as [Column Name]
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.object_id = OBJECT_ID('IdempotencyKeys')
ORDER BY i.name, ic.key_ordinal;
GO

-- ========================================
-- 4. CHECK PRIMARY KEY
-- ========================================
PRINT ''
PRINT '=== 4. Primary Key Information ==='
SELECT 
    tc.CONSTRAINT_NAME as [Constraint Name],
    tc.CONSTRAINT_TYPE as [Type],
    kcu.COLUMN_NAME as [Column Name]
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu 
    ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
WHERE tc.TABLE_NAME = 'IdempotencyKeys' 
    AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY';
GO

-- ========================================
-- 5. INSERT TEST DATA
-- ========================================
PRINT ''
PRINT '=== 5. Inserting test idempotency key ==='
BEGIN TRY
    INSERT INTO IdempotencyKeys (
      [Key], 
        [Status], 
        ResponseJson, 
        StatusCode, 
  ContentType, 
        CreatedAtUtc, 
        ExpiresAtUtc,
        UserId,
        Endpoint,
        Method
    ) VALUES (
        'test-verification-key-001',
    1, -- Completed
        '{"success": true, "orderId": "abc123", "message": "Test order created"}',
        200,
        'application/json',
        GETUTCDATE(),
        DATEADD(hour, 24, GETUTCDATE()),
        NEWID(), -- Random test user ID
        '/api/orders',
        'POST'
    );
    PRINT '✅ Test data inserted successfully'
END TRY
BEGIN CATCH
  PRINT '❌ Error inserting test data: ' + ERROR_MESSAGE()
END CATCH
GO

-- ========================================
-- 6. QUERY TEST DATA
-- ========================================
PRINT ''
PRINT '=== 6. Retrieving test data ==='
SELECT 
    [Key],
    [Status],
    StatusCode,
    ContentType,
    CreatedAtUtc,
    ExpiresAtUtc,
    Endpoint,
    Method,
    LEN(ResponseJson) as [Response Length]
FROM IdempotencyKeys
WHERE [Key] = 'test-verification-key-001';
GO

-- ========================================
-- 7. TEST QUERY PERFORMANCE
-- ========================================
PRINT ''
PRINT '=== 7. Testing index usage ==='
-- This query should use IX_IdempotencyKeys_Status index
SELECT [Key], StatusCode, CreatedAtUtc
FROM IdempotencyKeys
WHERE [Status] = 1; -- Completed

-- This query should use IX_IdempotencyKeys_ExpiresAtUtc index
SELECT [Key], ExpiresAtUtc
FROM IdempotencyKeys
WHERE ExpiresAtUtc < GETUTCDATE();

-- This query should use IX_IdempotencyKeys_UserId index
SELECT [Key], CreatedAtUtc
FROM IdempotencyKeys
WHERE UserId IS NOT NULL;
GO

-- ========================================
-- 8. CLEANUP TEST DATA
-- ========================================
PRINT ''
PRINT '=== 8. Cleaning up test data ==='
DELETE FROM IdempotencyKeys WHERE [Key] = 'test-verification-key-001';
PRINT '✅ Test data cleaned up'
GO

-- ========================================
-- 9. VERIFY EMPTY TABLE
-- ========================================
PRINT ''
PRINT '=== 9. Final verification ==='
DECLARE @count INT
SELECT @count = COUNT(*) FROM IdempotencyKeys
PRINT 'Current row count: ' + CAST(@count AS VARCHAR(10))
IF @count = 0
    PRINT '✅ Table is empty (ready for production)'
ELSE
    PRINT '⚠️ Table contains ' + CAST(@count AS VARCHAR(10)) + ' rows'
GO

-- ========================================
-- 10. MIGRATION HISTORY
-- ========================================
PRINT ''
PRINT '=== 10. Migration History ==='
SELECT 
    MigrationId,
    ProductVersion,
CASE 
      WHEN MigrationId LIKE '%AddIdempotency%' THEN '✅ CURRENT'
        ELSE ''
    END as [Status]
FROM __EFMigrationsHistory
ORDER BY MigrationId DESC;
GO

PRINT ''
PRINT '========================================='
PRINT '✅ VERIFICATION COMPLETE'
PRINT '========================================='
PRINT 'If all checks passed, your IdempotencyKeys table is ready for use!'
GO
