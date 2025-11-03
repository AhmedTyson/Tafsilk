# Database Status Check
Write-Host "`nTAFSILK PLATFORM - DATABASE STATUS" -ForegroundColor Cyan
Write-Host "====================================`n" -ForegroundColor Cyan

# Check database
Write-Host "Checking database..." -ForegroundColor Yellow
$db = sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT name FROM sys.databases WHERE name = 'TafsilkPlatformDb_Dev'" -h-1 -W
if ($db -match "TafsilkPlatformDb_Dev") {
    Write-Host "✅ Database exists: TafsilkPlatformDb_Dev" -ForegroundColor Green
    
    # Check tables
    $tables = sqlcmd -S "(localdb)\MSSQLLocalDB" -d TafsilkPlatformDb_Dev -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME != '__EFMigrationsHistory'" -h-1 -W
Write-Host "✅ Tables created: $tables" -ForegroundColor Green
    
    # Check indexes
    $indexes = sqlcmd -S "(localdb)\MSSQLLocalDB" -d TafsilkPlatformDb_Dev -Q "SELECT COUNT(*) FROM sys.indexes WHERE name LIKE 'IX_%'" -h-1 -W
    Write-Host "✅ Performance indexes: $indexes" -ForegroundColor Green
    
    # Check admin
    $admin = sqlcmd -S "(localdb)\MSSQLLocalDB" -d TafsilkPlatformDb_Dev -Q "SELECT Email FROM Users WHERE Email = 'admin@tafsilk.local'" -h-1 -W
    if ($admin -match "admin") {
        Write-Host "✅ Admin user seeded: admin@tafsilk.local" -ForegroundColor Green
    }
    
    Write-Host "`n✅ DATABASE READY!" -ForegroundColor Green
} else {
  Write-Host "❌ Database not found" -ForegroundColor Red
}

Write-Host "`nRun 'dotnet run' to start the application" -ForegroundColor Cyan
