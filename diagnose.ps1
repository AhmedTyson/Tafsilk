# Tafsilk Platform - Diagnostic Script
# Run this script to diagnose common runtime issues

Write-Host "🔍 Tafsilk Platform - Runtime Diagnostics" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "TafsilkPlatform.Web"
$issuesFound = 0

# Check 1: Project exists
Write-Host "✓ Checking project structure..." -ForegroundColor Yellow
if (Test-Path $projectPath) {
    Write-Host "  ✅ Project directory found" -ForegroundColor Green
} else {
    Write-Host "  ❌ Project directory not found!" -ForegroundColor Red
$issuesFound++
}

# Check 2: OAuth Configuration
Write-Host ""
Write-Host "✓ Checking OAuth configuration..." -ForegroundColor Yellow
Push-Location $projectPath
$secrets = dotnet user-secrets list 2>$null

if ($secrets -match "Google:client_id") {
    Write-Host "  ✅ Google OAuth configured" -ForegroundColor Green
} else {
  Write-Host "  ⚠️  Google OAuth NOT configured" -ForegroundColor Yellow
    Write-Host "     Run: dotnet user-secrets set 'Google:client_id' 'YOUR_ID'" -ForegroundColor Gray
    $issuesFound++
}

if ($secrets -match "Facebook:app_id") {
    Write-Host "  ✅ Facebook OAuth configured" -ForegroundColor Green
} else {
    Write-Host "  ⚠️  Facebook OAuth NOT configured" -ForegroundColor Yellow
    Write-Host "     Run: dotnet user-secrets set 'Facebook:app_id' 'YOUR_ID'" -ForegroundColor Gray
  $issuesFound++
}
Pop-Location

# Check 3: Build Status
Write-Host ""
Write-Host "✓ Checking build status..." -ForegroundColor Yellow
Push-Location $projectPath
$buildOutput = dotnet build --no-restore 2>&1 | Out-String

if ($buildOutput -match "Build succeeded") {
    if ($buildOutput -match "0 Warning\(s\)") {
        Write-Host "  ✅ Build successful (0 errors, 0 warnings)" -ForegroundColor Green
 } else {
        $warnings = [regex]::Match($buildOutput, "(\d+) Warning\(s\)").Groups[1].Value
        Write-Host "  ⚠️  Build successful but has $warnings warning(s)" -ForegroundColor Yellow
  $issuesFound++
    }
} else {
    Write-Host "  ❌ Build FAILED!" -ForegroundColor Red
    $issuesFound++
}
Pop-Location

# Check 4: Database Connection
Write-Host ""
Write-Host "✓ Checking database configuration..." -ForegroundColor Yellow
$appSettings = Get-Content "$projectPath/appsettings.json" | ConvertFrom-Json
$connectionString = $appSettings.ConnectionStrings.DefaultConnection

if ($connectionString) {
    Write-Host "  ✅ Connection string configured" -ForegroundColor Green
    if ($connectionString -match "LocalDB") {
        Write-Host "     Using LocalDB (Development)" -ForegroundColor Gray
    }
} else {
    Write-Host "  ❌ Connection string NOT found!" -ForegroundColor Red
    $issuesFound++
}

# Check 5: ViewModels
Write-Host ""
Write-Host "✓ Checking ViewModels..." -ForegroundColor Yellow
$viewModelsPath = "$projectPath/ViewModels"
$requiredViewModels = @(
    "RegisterRequest.cs",
    "RegistrationRole.cs",
 "LoginRequest.cs",
    "ChangePasswordViewModel.cs",
    "UserSettingsViewModel.cs",
  "AccountSettingsViewModel.cs",
    "CompleteGoogleRegistrationViewModel.cs",
    "RoleChangeRequestViewModel.cs",
    "TokenResponse.cs",
    "UpdateUserSettingsRequest.cs"
)

$missingViewModels = @()
foreach ($vm in $requiredViewModels) {
    $vmPath = Join-Path $viewModelsPath $vm
    if (Test-Path $vmPath) {
        # Write-Host "  ✅ $vm" -ForegroundColor Green
    } else {
        $missingViewModels += $vm
        $issuesFound++
    }
}

if ($missingViewModels.Count -eq 0) {
  Write-Host "  ✅ All required ViewModels present ($($requiredViewModels.Count) files)" -ForegroundColor Green
} else {
    Write-Host "  ❌ Missing ViewModels:" -ForegroundColor Red
    foreach ($vm in $missingViewModels) {
        Write-Host "     - $vm" -ForegroundColor Red
    }
}

# Check 6: Controllers
Write-Host ""
Write-Host "✓ Checking Controllers..." -ForegroundColor Yellow
$controllersPath = "$projectPath/Controllers"
$requiredControllers = @(
    "AccountController.cs",
    "UserSettingsController.cs"
)

$missingControllers = @()
foreach ($ctrl in $requiredControllers) {
    $ctrlPath = Join-Path $controllersPath $ctrl
    if (Test-Path $ctrlPath) {
        # Write-Host "  ✅ $ctrl" -ForegroundColor Green
    } else {
    $missingControllers += $ctrl
  $issuesFound++
    }
}

if ($missingControllers.Count -eq 0) {
    Write-Host "  ✅ All required Controllers present ($($requiredControllers.Count) files)" -ForegroundColor Green
} else {
    Write-Host "  ❌ Missing Controllers:" -ForegroundColor Red
    foreach ($ctrl in $missingControllers) {
        Write-Host "     - $ctrl" -ForegroundColor Red
    }
}

# Check 7: Git Status
Write-Host ""
Write-Host "✓ Checking Git status..." -ForegroundColor Yellow
$gitStatus = git status --porcelain 2>$null

if ($LASTEXITCODE -eq 0) {
    if ([string]::IsNullOrWhiteSpace($gitStatus)) {
        Write-Host "  ✅ Working directory clean (no uncommitted changes)" -ForegroundColor Green
    } else {
        $changedFiles = ($gitStatus -split "`n").Count
        Write-Host "  ⚠️  $changedFiles uncommitted change(s)" -ForegroundColor Yellow
    }
    
    $currentBranch = git branch --show-current
    Write-Host "     Current branch: $currentBranch" -ForegroundColor Gray
} else {
    Write-Host "  ⚠️  Not a Git repository or Git not installed" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "📊 Diagnostic Summary" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

if ($issuesFound -eq 0) {
    Write-Host ""
    Write-Host "🎉 NO ISSUES FOUND!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Your application appears to be properly configured." -ForegroundColor Green
    Write-Host "You can run the application with:" -ForegroundColor White
Write-Host "  cd $projectPath" -ForegroundColor Cyan
    Write-Host "  dotnet run" -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "⚠️  Found $issuesFound potential issue(s)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Please review the issues above and:" -ForegroundColor Yellow
    Write-Host "  1. Configure missing OAuth credentials" -ForegroundColor White
    Write-Host "  2. Fix any build errors or warnings" -ForegroundColor White
    Write-Host "  3. Ensure all required files exist" -ForegroundColor White
    Write-Host ""
    Write-Host "📖 See RUNTIME_ISSUES_FIX_GUIDE.md for detailed solutions" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "For more help, see:" -ForegroundColor White
Write-Host "  - RUNTIME_ISSUES_FIX_GUIDE.md" -ForegroundColor Cyan
Write-Host "  - README.md" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
