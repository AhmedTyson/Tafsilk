using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TafsilkPlatform.Web.Data.Migrations
{
 public partial class ConvertCascadeToNoAction : Migration
 {
 protected override void Up(MigrationBuilder migrationBuilder)
 {
 // Drop any FK with ON DELETE CASCADE and recreate with NO ACTION
 migrationBuilder.Sql(@"
SET NOCOUNT ON;
DECLARE @fkname sysname, @parent_schema sysname, @parent_table sysname, @sql nvarchar(max);
DECLARE fk CURSOR FOR
SELECT fk.name, schema_name(o.schema_id) AS parent_schema, o.name AS parent_table
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.delete_referential_action =1; --1 = CASCADE

OPEN fk;
FETCH NEXT FROM fk INTO @fkname, @parent_schema, @parent_table;
WHILE @@FETCH_STATUS =0
BEGIN
 -- Build column lists
 DECLARE @parent_cols nvarchar(max) = '';
 DECLARE @ref_table nvarchar(max) = '';
 DECLARE @ref_cols nvarchar(max) = '';

 SELECT @parent_cols = STRING_AGG(QUOTENAME(pc.name), ',') WITHIN GROUP (ORDER BY fkc.constraint_column_id)
 FROM sys.foreign_key_columns fkc
 JOIN sys.columns pc ON fkc.parent_object_id = pc.object_id AND fkc.parent_column_id = pc.column_id
 JOIN sys.foreign_keys fk2 ON fkc.constraint_object_id = fk2.object_id
 WHERE fk2.name = @fkname;

 SELECT @ref_cols = STRING_AGG(QUOTENAME(rc.name), ',') WITHIN GROUP (ORDER BY fkc.constraint_column_id)
 FROM sys.foreign_key_columns fkc
 JOIN sys.columns rc ON fkc.referenced_object_id = rc.object_id AND fkc.referenced_column_id = rc.column_id
 JOIN sys.foreign_keys fk2 ON fkc.constraint_object_id = fk2.object_id
 WHERE fk2.name = @fkname;

 SELECT TOP1 @ref_table = QUOTENAME(schema_name(o2.schema_id)) + '.' + QUOTENAME(o2.name)
 FROM sys.foreign_keys fk3
 JOIN sys.objects o2 ON fk3.referenced_object_id = o2.object_id
 WHERE fk3.name = @fkname;

 SET @sql = N'ALTER TABLE ' + QUOTENAME(@parent_schema) + N'.' + QUOTENAME(@parent_table) + N' DROP CONSTRAINT ' + QUOTENAME(@fkname) + N';';
 EXEC sp_executesql @sql;

 SET @sql = N'ALTER TABLE ' + QUOTENAME(@parent_schema) + N'.' + QUOTENAME(@parent_table) +
 N' ADD CONSTRAINT ' + QUOTENAME(@fkname) +
 N' FOREIGN KEY (' + ISNULL(@parent_cols, '') + N') REFERENCES ' + ISNULL(@ref_table, 'dbo.Orders') + N'(' + ISNULL(@ref_cols, '') + N') ON DELETE NO ACTION;';
 EXEC sp_executesql @sql;

 FETCH NEXT FROM fk INTO @fkname, @parent_schema, @parent_table;
END
CLOSE fk;
DEALLOCATE fk;
");
 }

 protected override void Down(MigrationBuilder migrationBuilder)
 {
 // No automatic down safe operation
 }
 }
}
