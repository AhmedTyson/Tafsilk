-- This script drops existing FOREIGN KEY constraints that reference dbo.Orders
-- and recreates equivalent FOREIGN KEY constraints with ON DELETE NO ACTION.
-- Review before running. Running this will modify the database schema.

SET NOCOUNT ON;

DECLARE @fkName sysname, @parentSchema sysname, @parentTable sysname;
DECLARE @cols NVARCHAR(MAX), @refCols NVARCHAR(MAX), @sql NVARCHAR(MAX);

DECLARE fk_cursor CURSOR FOR
SELECT fk.name AS FK_Name,
 SCH.name AS ParentSchema,
 PT.name AS ParentTable
FROM sys.foreign_keys fk
JOIN sys.objects PT ON fk.parent_object_id = PT.object_id
JOIN sys.schemas SCH ON PT.schema_id = SCH.schema_id
WHERE fk.referenced_object_id = OBJECT_ID(N'dbo.Orders');

OPEN fk_cursor;
FETCH NEXT FROM fk_cursor INTO @fkName, @parentSchema, @parentTable;

WHILE @@FETCH_STATUS =0
BEGIN
 -- Build column lists for the FK
 SELECT @cols = STRING_AGG(QUOTENAME(pc.name), ',') WITHIN GROUP (ORDER BY fkc.constraint_column_id)
 FROM sys.foreign_key_columns fkc
 JOIN sys.columns pc ON fkc.parent_object_id = pc.object_id AND fkc.parent_column_id = pc.column_id
 JOIN sys.foreign_keys fk2 ON fkc.constraint_object_id = fk2.object_id
 WHERE fk2.name = @fkName;

 SELECT @refCols = STRING_AGG(QUOTENAME(rc.name), ',') WITHIN GROUP (ORDER BY fkc.constraint_column_id)
 FROM sys.foreign_key_columns fkc
 JOIN sys.columns rc ON fkc.referenced_object_id = rc.object_id AND fkc.referenced_column_id = rc.column_id
 JOIN sys.foreign_keys fk2 ON fkc.constraint_object_id = fk2.object_id
 WHERE fk2.name = @fkName;

 -- Drop the existing FK
 SET @sql = N'ALTER TABLE ' + QUOTENAME(@parentSchema) + N'.' + QUOTENAME(@parentTable) + N' DROP CONSTRAINT ' + QUOTENAME(@fkName) + N';';
 PRINT('Dropping: ' + @fkName + ' on ' + @parentSchema + '.' + @parentTable);
 EXEC sp_executesql @sql;

 -- Recreate FK with ON DELETE NO ACTION
 SET @sql = N'ALTER TABLE ' + QUOTENAME(@parentSchema) + N'.' + QUOTENAME(@parentTable) +
 N' ADD CONSTRAINT ' + QUOTENAME(@fkName) +
 N' FOREIGN KEY (' + ISNULL(@cols, '') + N') REFERENCES dbo.Orders(' + ISNULL(@refCols, '') + N') ON DELETE NO ACTION;';
 PRINT('Recreating: ' + @fkName + ' as NO ACTION');
 EXEC sp_executesql @sql;

 FETCH NEXT FROM fk_cursor INTO @fkName, @parentSchema, @parentTable;
END;

CLOSE fk_cursor;
DEALLOCATE fk_cursor;

PRINT 'Done.';
