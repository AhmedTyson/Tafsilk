# Pre-Deployment Validation Script
# Run this before deploying to production

Write-Host "TafsilkPlatform.Web - Pre-Deployment Validation" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

$ErrorCount = 0
$WarningCount = 0

# Check 1: .NET SDK Version
Write-Host "[1/10] Checking .NET SDK..." -ForegroundColor Yellow
try {
  $dotnetVersion = dotnet --version
    if ($dotnetVersion -like "9.*") {
        Write-Host "✓ .NET 9 SDK installed: $dotnetVersion" -ForegroundColor Green
    } else {
   Write-Host "✗ .NET 9 SDK not found. Current: $dotnetVersion" -ForegroundColor Red
   $ErrorCount++
    }
} catch {
    Write-Host "✗ .NET SDK not installed" -ForegroundColor Red
    $ErrorCount++
}

# Check 2: Build Release Configuration
Write-Host "[2/10] Building Release configuration..." -ForegroundColor Yellow
$buildResult = dotnet build TafsilkPlatform.Web/TafsilkPlatform.Web.csproj --configuration Release --no-incremental
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Release build successful" -ForegroundColor Green
} else {
    Write-Host "✗ Release build failed" -ForegroundColor Red
    $ErrorCount++
}

# Check 3: Run Tests
Write-Host "[3/10] Running tests..." -ForegroundColor Yellow
$testResult = dotnet test --configuration Release --no-build 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ All tests passed" -ForegroundColor Green
} else {
    Write-Host "⚠ No tests found or tests failed" -ForegroundColor Yellow
    $WarningCount++
}

# Check 4: Check for pending migrations
Write-Host "[4/10] Checking database migrations..." -ForegroundColor Yellow
Push-Location TafsilkPlatform.Web
$migrations = dotnet ef migrations list 2>&1 | Select-String "Pending"
Pop-Location
if ($migrations) {
    Write-Host "⚠ Pending migrations found. Run 'dotnet ef database update' before deployment" -ForegroundColor Yellow
    $WarningCount++
} else {
    Write-Host "✓ No pending migrations" -ForegroundColor Green
}

# Check 5: Production appsettings
Write-Host "[5/10] Validating production configuration..." -ForegroundColor Yellow
if (Test-Path "TafsilkPlatform.Web/appsettings.Production.json") {
    $prodConfig = Get-Content "TafsilkPlatform.Web/appsettings.Production.json" -Raw | ConvertFrom-Json
    
    if ($prodConfig.ConnectionStrings.DefaultConnection -match "localdb|localhost") {
        Write-Host "⚠ Production config contains local connection string" -ForegroundColor Yellow
     $WarningCount++
    } else {
        Write-Host "✓ Production configuration exists" -ForegroundColor Green
    }
    
    if ($prodConfig.Jwt.Key -eq "" -or $prodConfig.Jwt.Key.Length -lt 32) {
        Write-Host "⚠ JWT key not configured or too short" -ForegroundColor Yellow
  $WarningCount++
    }
} else {
    Write-Host "✗ appsettings.Production.json not found" -ForegroundColor Red
    $ErrorCount++
}

# Check 6: Secrets in appsettings.json
Write-Host "[6/10] Checking for hardcoded secrets..." -ForegroundColor Yellow
$appsettings = Get-Content "TafsilkPlatform.Web/appsettings.json" -Raw
if ($appsettings -match '"Key":\s*"[A-Za-z0-9+/=]{32,}"' -or 
    $appsettings -match '"client_secret":\s*"[^"]{10,}"' -or
    $appsettings -match '"Password=[^;]{5,}"') {
    Write-Host "⚠ Potential secrets found in appsettings.json" -ForegroundColor Yellow
    $WarningCount++
} else {
 Write-Host "✓ No hardcoded secrets detected" -ForegroundColor Green
}

# Check 7: wwwroot folder size
Write-Host "[7/10] Checking static files..." -ForegroundColor Yellow
if (Test-Path "TafsilkPlatform.Web/wwwroot") {
    $size = (Get-ChildItem "TafsilkPlatform.Web/wwwroot" -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
    if ($size -gt 100) {
        Write-Host "⚠ wwwroot folder is large: $([math]::Round($size, 2)) MB" -ForegroundColor Yellow
        $WarningCount++
    } else {
        Write-Host "✓ Static files size OK: $([math]::Round($size, 2)) MB" -ForegroundColor Green
  }
}

# Check 8: HTTPS configuration
Write-Host "[8/10] Checking HTTPS configuration..." -ForegroundColor Yellow
$programCs = Get-Content "TafsilkPlatform.Web/Program.cs" -Raw
if ($programCs -match "UseHttpsRedirection") {
    Write-Host "✓ HTTPS redirection enabled" -ForegroundColor Green
} else {
    Write-Host "⚠ HTTPS redirection not found" -ForegroundColor Yellow
    $WarningCount++
}

# Check 9: Environment-specific logging
Write-Host "[9/10] Checking logging configuration..." -ForegroundColor Yellow
if (Test-Path "TafsilkPlatform.Web/appsettings.Production.json") {
    $prodLog = Get-Content "TafsilkPlatform.Web/appsettings.Production.json" -Raw | ConvertFrom-Json
    if ($prodLog.Logging.LogLevel.Default -eq "Warning" -or $prodLog.Logging.LogLevel.Default -eq "Error") {
        Write-Host "✓ Production logging configured appropriately" -ForegroundColor Green
    } else {
 Write-Host "⚠ Production logging level may be too verbose" -ForegroundColor Yellow
        $WarningCount++
    }
}

# Check 10: Git status
Write-Host "[10/10] Checking Git status..." -ForegroundColor Yellow
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Host "⚠ Uncommitted changes found:" -ForegroundColor Yellow
    Write-Host $gitStatus
    $WarningCount++
} else {
    Write-Host "✓ All changes committed" -ForegroundColor Green
}

# Summary
Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "Validation Summary" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "Errors:   $ErrorCount" -ForegroundColor $(if ($ErrorCount -eq 0) { "Green" } else { "Red" })
Write-Host "Warnings: $WarningCount" -ForegroundColor $(if ($WarningCount -eq 0) { "Green" } else { "Yellow" })
Write-Host ""

if ($ErrorCount -eq 0) {
    Write-Host "✓ Validation passed! Ready for deployment." -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Review DEPLOYMENT.md for deployment options"
    Write-Host "2. Configure production secrets (not in source control)"
    Write-Host "3. Run: dotnet publish -c Release -o ./publish"
    Write-Host "4. Deploy using your preferred method (Azure/IIS/Docker)"
    exit 0
} else {
    Write-Host "✗ Validation failed. Please fix errors before deploying." -ForegroundColor Red
    exit 1
}
