-- Add PrimaryImageUrl column to Products table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Products]') AND name = 'PrimaryImageUrl')
BEGIN
    ALTER TABLE [Products] ADD [PrimaryImageUrl] nvarchar(500) NULL;
    PRINT 'Column PrimaryImageUrl added to table Products.';
END
ELSE
BEGIN
    PRINT 'Column PrimaryImageUrl already exists in table Products.';
END
GO
