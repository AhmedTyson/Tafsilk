# Tafsilk Platform Startup Script
Write-Host "Starting Tafsilk Platform..." -ForegroundColor Green
Write-Host ""

# Change to project directory
Set-Location $PSScriptRoot

# Build the project
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed! Please fix errors before running." -ForegroundColor Red
    exit 1
}

Write-Host "Build succeeded!" -ForegroundColor Green
Write-Host ""

# Start the application
Write-Host "Starting application on https://localhost:7186..." -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop the application" -ForegroundColor Cyan
Write-Host ""

dotnet run --launch-profile https

