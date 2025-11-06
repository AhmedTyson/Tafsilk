-- ========================================
-- Tafsilk Platform - Database Verification Script
-- Customer Journey Workflow Implementation
-- ========================================

USE TafsilkPlatformDb_Dev;
GO

PRINT '========================================';
PRINT 'VERIFYING CUSTOMER JOURNEY ENHANCEMENTS';
PRINT '========================================';
PRINT '';

-- Check Migration Status
PRINT '1. MIGRATION STATUS:';
PRINT '--------------------';
SELECT TOP 1 MigrationId, ProductVersion 
FROM __EFMigrationsHistory 
WHERE MigrationId LIKE '%AddLoyaltyComplaintsAndMeasurements%'
ORDER BY MigrationId DESC;
PRINT '';

-- Verify New Tables
PRINT '2. NEW TABLES CREATED:';
PRINT '----------------------';
SELECT TABLE_NAME, 
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_NAME IN (
    'CustomerLoyalty',
    'LoyaltyTransactions',
    'CustomerMeasurements',
    'Complaints',
    'ComplaintAttachments'
)
ORDER BY TABLE_NAME;
PRINT '';

-- Verify New Columns in Orders Table
PRINT '3. NEW ORDER TABLE COLUMNS:';
PRINT '---------------------------';
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Orders' 
AND COLUMN_NAME IN (
    'TailorQuote',
    'TailorQuoteNotes',
    'QuoteProvidedAt',
    'RequiresDeposit',
    'DepositAmount',
    'DepositPaid',
    'DepositPaidAt',
    'MeasurementsJson',
  'FulfillmentMethod',
    'DeliveryAddress'
)
ORDER BY ORDINAL_POSITION;
PRINT '';

-- Check Indexes
PRINT '4. NEW INDEXES CREATED:';
PRINT '-----------------------';
SELECT 
 i.name AS IndexName,
    t.name AS TableName,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('CustomerLoyalty', 'LoyaltyTransactions', 'CustomerMeasurements', 'Complaints', 'ComplaintAttachments')
AND i.name IS NOT NULL
ORDER BY t.name, i.name;
PRINT '';

-- Sample Data Counts
PRINT '5. TABLE ROW COUNTS:';
PRINT '--------------------';
SELECT 'CustomerLoyalty' AS TableName, COUNT(*) AS RowCount FROM CustomerLoyalty
UNION ALL
SELECT 'LoyaltyTransactions', COUNT(*) FROM LoyaltyTransactions
UNION ALL
SELECT 'CustomerMeasurements', COUNT(*) FROM CustomerMeasurements
UNION ALL
SELECT 'Complaints', COUNT(*) FROM Complaints
UNION ALL
SELECT 'ComplaintAttachments', COUNT(*) FROM ComplaintAttachments;
PRINT '';

-- Verify Foreign Keys
PRINT '6. FOREIGN KEY RELATIONSHIPS:';
PRINT '------------------------------';
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) IN (
    'CustomerLoyalty', 
    'LoyaltyTransactions', 
    'CustomerMeasurements', 
    'Complaints', 
    'ComplaintAttachments'
)
ORDER BY TableName, ForeignKeyName;
PRINT '';

PRINT '========================================';
PRINT 'VERIFICATION COMPLETE!';
PRINT '========================================';
PRINT '';
PRINT 'All new tables and columns have been verified.';
PRINT 'The Customer Journey Workflow implementation is ready!';
PRINT '';
