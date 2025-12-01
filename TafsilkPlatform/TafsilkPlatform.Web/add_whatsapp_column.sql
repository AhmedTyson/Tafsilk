-- Add WhatsAppNumber column to TailorProfiles table
-- For SQL Server database

ALTER TABLE TailorProfiles 
ADD WhatsAppNumber NVARCHAR(20) NULL;

-- Verify the column was added
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TailorProfiles' 
  AND COLUMN_NAME = 'WhatsAppNumber';
