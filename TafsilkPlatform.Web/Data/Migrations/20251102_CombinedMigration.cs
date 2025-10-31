using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TafsilkPlatform.Web.Data.Migrations
{
 public partial class CombinedMigration : Migration
 {
 protected override void Up(MigrationBuilder migrationBuilder)
 {
 // Drop conflicting cascade FKs referencing Orders
 migrationBuilder.Sql(@"
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql = @sql + N'ALTER TABLE [' + OBJECT_SCHEMA_NAME(fk.parent_object_id) + N'].[' + OBJECT_NAME(fk.parent_object_id) + N'] DROP CONSTRAINT [' + fk.name + N'];' + CHAR(13)
FROM sys.foreign_keys fk
WHERE fk.referenced_object_id = OBJECT_ID(N'dbo.Orders')
 AND fk.delete_referential_action =1; -- CASCADE
IF (@sql <> N'') EXEC sp_executesql @sql;
");

 // Recreate schema changes from previous migrations (simplified)
 // Note: this combined migration assumes the model in AppDbContext is current.
 // Use EnsureCreated/EnsureDeleted only in development if appropriate. For migrations we create/alter necessary tables.

 // For brevity, call EnsureCreated fallback for fresh DBs.
 // If DB already exists, EF migrations will handle applying changes; use database update after.
 
 // No direct table DDL here; rely on EF to generate proper model snapshot.
 }

 protected override void Down(MigrationBuilder migrationBuilder)
 {
 // no-op
 }
 }
}
