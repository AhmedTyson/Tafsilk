# Tafsilk Platform - Clear Test Data Script

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TAFSILK PLATFORM - CLEAR TEST DATA" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if application is running
$appUrl = "https://localhost:7186"
Write-Host "Checking if application is running..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "$appUrl" -Method GET -UseBasicParsing -TimeoutSec 5 -SkipCertificateCheck -ErrorAction SilentlyContinue
Write-Host "✅ Application is running" -ForegroundColor Green
} catch {
  Write-Host "❌ Application is not running. Please start it first:" -ForegroundColor Red
    Write-Host "   cd TafsilkPlatform.Web" -ForegroundColor Yellow
    Write-Host "   dotnet run" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

Write-Host ""
Write-Host "⚠️  WARNING: This will delete all test data!" -ForegroundColor Yellow
$confirmation = Read-Host "Are you sure you want to continue? (yes/no)"

if ($confirmation -ne "yes") {
    Write-Host "Operation cancelled" -ForegroundColor Yellow
  exit 0
}

Write-Host ""
Write-Host "Clearing test data..." -ForegroundColor Yellow

try {
  $clearUrl = "$appUrl/api/DevData/clear-test-data"
    $response = Invoke-RestMethod -Uri $clearUrl -Method DELETE -UseBasicParsing -SkipCertificateCheck
    
    Write-Host "✅ Test data cleared successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "You can now run SeedTestData.ps1 to create fresh test data" -ForegroundColor Cyan
    Write-Host ""
} catch {
    Write-Host "❌ Failed to clear test data" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
Write-Host ""
    exit 1
}
