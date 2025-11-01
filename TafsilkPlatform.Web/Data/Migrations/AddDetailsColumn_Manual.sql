-- Migration: Add Details column to UserActivityLogs table
-- This fixes the string truncation error in EntityType column

-- Add the new Details column to store long descriptions
ALTER TABLE [UserActivityLogs] 
ADD [Details] NVARCHAR(1000) NULL;

-- Optional: Migrate existing data from EntityType to Details if needed
-- (Only if you have existing logs with descriptions in EntityType)
-- UPDATE [UserActivityLogs] 
-- SET [Details] = [EntityType]
-- WHERE LEN([EntityType]) > 50;

-- The EntityType column will now store the actual entity type (e.g., 'TailorProfile', 'User')
-- while Details will store the detailed description of the action
