# Tafsilk Platform - Test Data Seeding Script
# Run this after starting the application

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TAFSILK PLATFORM - TEST DATA SEEDER" -ForegroundColor Cyan
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
Write-Host "Seeding test data..." -ForegroundColor Yellow

try {
    $seedUrl = "$appUrl/api/DevData/seed-test-data"
    $response = Invoke-RestMethod -Uri $seedUrl -Method POST -UseBasicParsing -SkipCertificateCheck
    
    Write-Host "✅ Test data seeded successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "TEST CREDENTIALS" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Password for ALL accounts: Test@123" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Customers:" -ForegroundColor Green
    $response.testCredentials.customers | ForEach-Object {
   Write-Host "  - $_" -ForegroundColor White
  }
    Write-Host ""
    Write-Host "Tailors:" -ForegroundColor Green
    $response.testCredentials.tailors | ForEach-Object {
        Write-Host "  - $_" -ForegroundColor White
    }
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "READY FOR TESTING!" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "You can now:" -ForegroundColor Yellow
    Write-Host "1. Navigate to $appUrl" -ForegroundColor White
    Write-Host "2. Login with any test account" -ForegroundColor White
    Write-Host "3. Test the customer journey workflow" -ForegroundColor White
    Write-Host ""
    Write-Host "For more testing scenarios, see TESTING_GUIDE.md" -ForegroundColor Cyan
    Write-Host ""
} catch {
    Write-Host "❌ Failed to seed test data" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "This might happen if:" -ForegroundColor Yellow
    Write-Host "1. Test data already exists (run Clear-TestData.ps1 first)" -ForegroundColor White
    Write-Host "2. Database migrations not applied" -ForegroundColor White
    Write-Host "3. Connection string is incorrect" -ForegroundColor White
    Write-Host ""
 exit 1
}
