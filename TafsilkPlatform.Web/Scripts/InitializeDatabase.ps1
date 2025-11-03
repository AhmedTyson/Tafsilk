# Database Initialization Script
Write-Host "╔════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   Tafsilk Platform - Database Initialization      ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Step 1: Build the project
Write-Host "Step 1: Building project..." -ForegroundColor Yellow
dotnet build --nologo
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Build successful" -ForegroundColor Green
Write-Host ""

# Step 2: Drop existing database
Write-Host "Step 2: Dropping existing database (if exists)..." -ForegroundColor Yellow
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "DROP DATABASE IF EXISTS TafsilkPlatformDb_Dev" -b
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Database dropped" -ForegroundColor Green
}
Write-Host ""

# Step 3: Start application to initialize database
Write-Host "Step 3: Starting application to initialize database..." -ForegroundColor Yellow
Write-Host "This will take about 10 seconds..." -ForegroundColor Gray

$job = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    dotnet run --no-build 2>&1
}

# Wait for application to start and initialize database
Start-Sleep -Seconds 10

# Stop the application
Stop-Job -Job $job
Remove-Job -Job $job -Force

Write-Host "✓ Application stopped" -ForegroundColor Green
Write-Host ""

# Step 4: Verify database was created
Write-Host "Step 4: Verifying database creation..." -ForegroundColor Yellow

$dbCheck = sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT name FROM sys.databases WHERE name = 'TafsilkPlatformDb_Dev'" -h-1 -W
if ($dbCheck -match "TafsilkPlatformDb_Dev") {
    Write-Host "✓ Database exists" -ForegroundColor Green
 
    # Check tables
    $tableCount = sqlcmd -S "(localdb)\MSSQLLocalDB" -d TafsilkPlatformDb_Dev -Q "SELECT COUNT(*) as cnt FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'" -h-1 -W
    Write-Host "  Tables created: $tableCount" -ForegroundColor Cyan
  
    # List some key tables
    Write-Host ""
  Write-Host "Key tables:" -ForegroundColor Cyan
    $tables = sqlcmd -S "(localdb)\MSSQLLocalDB" -d TafsilkPlatformDb_Dev -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME IN ('Users', 'Roles', 'TailorProfiles', 'CustomerProfiles', 'Orders') ORDER BY TABLE_NAME" -h-1 -W
    foreach ($table in $tables) {
    if ($table.Trim()) {
            Write-Host "  ✓ $table" -ForegroundColor Green
      }
    }
} else {
    Write-Host "✗ Database was not created!" -ForegroundColor Red
 Write-Host "Please check the application logs for errors." -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║    Database Initialization Complete! ✓       ║" -ForegroundColor Green
Write-Host "╚��═══════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review PERFORMANCE_OPTIMIZATIONS.md for optimization details" -ForegroundColor Gray
Write-Host "  2. Run 'dotnet run' to start the application" -ForegroundColor Gray
Write-Host "  3. Navigate to https://localhost:5140 or http://localhost:5140" -ForegroundColor Gray
Write-Host ""
