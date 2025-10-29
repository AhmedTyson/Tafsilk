# OAuth Test and Fix Script

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  OAuth State Error - Diagnostic Tool" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# 1. Check if app is running
Write-Host "[1/6] Checking if application is running..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "https://localhost:7186" -UseBasicParsing -TimeoutSec 5 -SkipCertificateCheck
    Write-Host "    ✅ Application is running" -ForegroundColor Green
} catch {
    Write-Host "    ❌ Application is NOT running" -ForegroundColor Red
    Write-Host "       Start it with: dotnet run" -ForegroundColor Gray
    Write-Host ""
}

# 2. Check HTTPS certificate
Write-Host "[2/6] Checking HTTPS certificate..." -ForegroundColor Yellow
try {
    $certCheck = dotnet dev-certs https --check 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ HTTPS certificate is valid and trusted" -ForegroundColor Green
    } else {
   Write-Host "    ❌ HTTPS certificate issue detected" -ForegroundColor Red
  Write-Host "       Run these commands:" -ForegroundColor Gray
        Write-Host "   dotnet dev-certs https --clean" -ForegroundColor Gray
    Write-Host "       dotnet dev-certs https --trust" -ForegroundColor Gray
    }
} catch {
    Write-Host "  ⚠️  Could not check certificate" -ForegroundColor Yellow
}
Write-Host ""

# 3. Check port
Write-Host "[3/6] Checking if port 7186 is accessible..." -ForegroundColor Yellow
$portCheck = Test-NetConnection -ComputerName localhost -Port 7186 -WarningAction SilentlyContinue -InformationLevel Quiet
if ($portCheck) {
    Write-Host "    ✅ Port 7186 is accessible" -ForegroundColor Green
} else {
    Write-Host "    ❌ Port 7186 is not accessible or in use" -ForegroundColor Red
    Write-Host "       Check with: netstat -ano | findstr :7186" -ForegroundColor Gray
}
Write-Host ""

# 4. Check Program.cs configuration
Write-Host "[4/6] Checking Program.cs configuration..." -ForegroundColor Yellow
$programPath = "TafsilkPlatform.Web\Program.cs"
if (Test-Path $programPath) {
    $programContent = Get-Content $programPath -Raw
    
    $checks = @{
        "AddDataProtection" = $programContent -match "AddDataProtection"
        "AddSession" = $programContent -match "AddSession"
        "AddDistributedMemoryCache" = $programContent -match "AddDistributedMemoryCache"
        "CorrelationCookie.SameSite" = $programContent -match "CorrelationCookie\.SameSite"
        "UseSession before UseAuthentication" = ($programContent -match "UseSession.*UseAuthentication")
    }
    
    foreach ($check in $checks.GetEnumerator()) {
        if ($check.Value) {
   Write-Host "    ✅ $($check.Key)" -ForegroundColor Green
  } else {
       Write-Host "❌ $($check.Key) - MISSING!" -ForegroundColor Red
        }
    }
} else {
    Write-Host "    ❌ Program.cs not found!" -ForegroundColor Red
}
Write-Host ""

# 5. Check Facebook configuration
Write-Host "[5/6] Checking Facebook OAuth configuration..." -ForegroundColor Yellow
$appSettingsDev = "TafsilkPlatform.Web\appsettings.Development.json"
if (Test-Path $appSettingsDev) {
    try {
        $config = Get-Content $appSettingsDev | ConvertFrom-Json
        if ($config.Facebook.app_id) {
            Write-Host "    ✅ Facebook App ID configured" -ForegroundColor Green
            Write-Host "       App ID: $($config.Facebook.app_id)" -ForegroundColor Gray
        } else {
            Write-Host "    ❌ Facebook App ID not found" -ForegroundColor Red
        }
        
        if ($config.Facebook.app_secret) {
            Write-Host "    ✅ Facebook App Secret configured" -ForegroundColor Green
        } else {
    Write-Host "    ❌ Facebook App Secret not found" -ForegroundColor Red
        }
    } catch {
 Write-Host "    ⚠️  Could not parse configuration" -ForegroundColor Yellow
    }
} else {
    Write-Host "    ❌ appsettings.Development.json not found!" -ForegroundColor Red
}
Write-Host ""

# 6. Instructions
Write-Host "[6/6] Testing Instructions:" -ForegroundColor Yellow
Write-Host ""
Write-Host "Facebook App Settings Check:" -ForegroundColor Cyan
Write-Host "  1. Go to: https://developers.facebook.com/" -ForegroundColor White
Write-Host "  2. Select your app (ID: 1328542205419363)" -ForegroundColor White
Write-Host "  3. Facebook Login → Settings" -ForegroundColor White
Write-Host "  4. Verify redirect URI:" -ForegroundColor White
Write-Host "     https://localhost:7186/signin-facebook" -ForegroundColor Gray
Write-Host ""
Write-Host "Browser Testing Steps:" -ForegroundColor Cyan
Write-Host "  1. Close ALL browser windows" -ForegroundColor White
Write-Host "  2. Clear cookies: Ctrl+Shift+Delete" -ForegroundColor White
Write-Host "  3. Open in Incognito/Private window" -ForegroundColor White
Write-Host "  4. Go to: https://localhost:7186/Account/Login" -ForegroundColor White
Write-Host "  5. Open DevTools: F12" -ForegroundColor White
Write-Host "  6. Go to: Application → Cookies → https://localhost:7186" -ForegroundColor White
Write-Host "  7. Click 'Facebook' button" -ForegroundColor White
Write-Host "  8. Watch for cookie: .Tafsilk.Correlation.Facebook" -ForegroundColor White
Write-Host ""
Write-Host "Expected Cookies:" -ForegroundColor Cyan
Write-Host "  ✅ .Tafsilk.Correlation.Facebook - Should appear after clicking button" -ForegroundColor Green
Write-Host "  ✅ Cookie should have SameSite=Lax" -ForegroundColor Green
Write-Host "  ✅ Cookie should persist during Facebook redirect" -ForegroundColor Green
Write-Host ""
Write-Host "If Cookie is Missing:" -ForegroundColor Yellow
Write-Host "  → Browser is blocking cookies" -ForegroundColor Red
Write-Host "  → Check: chrome://settings/cookies" -ForegroundColor Gray
Write-Host "  → Temporarily allow all cookies" -ForegroundColor Gray
Write-Host ""
Write-Host "If Cookie Disappears:" -ForegroundColor Yellow
Write-Host "  → SameSite policy is too strict" -ForegroundColor Red
Write-Host "  → Try SameSite=None in Program.cs" -ForegroundColor Gray
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "For detailed troubleshooting, see:" -ForegroundColor Cyan
Write-Host "  OAUTH_STATE_ERROR_FIX.md" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
