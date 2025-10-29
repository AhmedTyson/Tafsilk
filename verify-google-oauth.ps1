# Google OAuth Configuration Verification Script

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Google OAuth Configuration Check" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Check if appsettings.json exists
$appSettingsPath = "TafsilkPlatform.Web\appsettings.json"
if (Test-Path $appSettingsPath) {
    Write-Host "[✓] appsettings.json found" -ForegroundColor Green
    
    # Read and parse JSON
    $appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
    
    if ($appSettings.web.client_id) {
      Write-Host "[✓] Google Client ID configured" -ForegroundColor Green
        Write-Host "  Client ID: $($appSettings.web.client_id)" -ForegroundColor Gray
    } else {
 Write-Host "[✗] Google Client ID missing!" -ForegroundColor Red
    }
    
  if ($appSettings.web.client_secret) {
      Write-Host "[✓] Google Client Secret configured" -ForegroundColor Green
   Write-Host "    Secret: $($appSettings.web.client_secret.Substring(0, 10))..." -ForegroundColor Gray
    } else {
        Write-Host "[✗] Google Client Secret missing!" -ForegroundColor Red
    }
    
    if ($appSettings.web.redirect_uris) {
        Write-Host "[✓] Redirect URIs configured" -ForegroundColor Green
        foreach ($uri in $appSettings.web.redirect_uris) {
            Write-Host "    - $uri" -ForegroundColor Gray
        }
    } else {
        Write-Host "[✗] Redirect URIs missing!" -ForegroundColor Red
    }
} else {
    Write-Host "[✗] appsettings.json not found!" -ForegroundColor Red
}

Write-Host ""

# Check if appsettings.Production.json exists
$prodSettingsPath = "TafsilkPlatform.Web\appsettings.Production.json"
if (Test-Path $prodSettingsPath) {
    Write-Host "[✓] appsettings.Production.json found" -ForegroundColor Green
    
    $prodSettings = Get-Content $prodSettingsPath | ConvertFrom-Json
    
    if ($prodSettings.web.redirect_uris) {
  Write-Host "[✓] Production redirect URIs configured" -ForegroundColor Green
        foreach ($uri in $prodSettings.web.redirect_uris) {
          if ($uri -like "*your-production-domain*") {
                Write-Host "    [!] WARNING: Still using placeholder domain!" -ForegroundColor Yellow
            Write-Host "    Please update: $uri" -ForegroundColor Yellow
        } else {
     Write-Host "    - $uri" -ForegroundColor Gray
         }
        }
    }
} else {
    Write-Host "[!] appsettings.Production.json not found" -ForegroundColor Yellow
    Write-Host "    Creating it now..." -ForegroundColor Gray
}

Write-Host ""

# Check Program.cs for Google OAuth configuration
$programPath = "TafsilkPlatform.Web\Program.cs"
if (Test-Path $programPath) {
    $programContent = Get-Content $programPath -Raw
    
    if ($programContent -match "\.AddGoogle\(") {
        Write-Host "[✓] Google OAuth registered in Program.cs" -ForegroundColor Green
    } else {
        Write-Host "[✗] Google OAuth not registered in Program.cs!" -ForegroundColor Red
    }
    
    if ($programContent -match "googleOptions\.CallbackPath") {
        Write-Host "[✓] Callback path configured" -ForegroundColor Green
    } else {
Write-Host "[!] Callback path not explicitly set" -ForegroundColor Yellow
    }
    
    if ($programContent -match 'googleOptions\.Scope\.Add\("profile"\)') {
        Write-Host "[✓] Profile scope requested" -ForegroundColor Green
 }
    
  if ($programContent -match 'googleOptions\.Scope\.Add\("email"\)') {
  Write-Host "[✓] Email scope requested" -ForegroundColor Green
    }
}

Write-Host ""

# Check AccountController for OAuth handlers
$controllerPath = "TafsilkPlatform.Web\Controllers\AccountController.cs"
if (Test-Path $controllerPath) {
    $controllerContent = Get-Content $controllerPath -Raw
    
    if ($controllerContent -match "GoogleLogin") {
     Write-Host "[✓] GoogleLogin action found" -ForegroundColor Green
    } else {
   Write-Host "[✗] GoogleLogin action missing!" -ForegroundColor Red
  }
    
    if ($controllerContent -match "GoogleResponse") {
        Write-Host "[✓] GoogleResponse action found" -ForegroundColor Green
    } else {
   Write-Host "[✗] GoogleResponse action missing!" -ForegroundColor Red
    }
    
    if ($controllerContent -match "CompleteGoogleRegistration") {
        Write-Host "[✓] CompleteGoogleRegistration action found" -ForegroundColor Green
    } else {
        Write-Host "[✗] CompleteGoogleRegistration action missing!" -ForegroundColor Red
    }
}

Write-Host ""

# Check views
$loginViewPath = "TafsilkPlatform.Web\Views\Account\Login.cshtml"
if (Test-Path $loginViewPath) {
    $loginContent = Get-Content $loginViewPath -Raw
    
    if ($loginContent -match "GoogleLogin") {
        Write-Host "[✓] Google button connected in Login.cshtml" -ForegroundColor Green
    } else {
        Write-Host "[!] Google button not connected in Login.cshtml" -ForegroundColor Yellow
    }
}

$registerViewPath = "TafsilkPlatform.Web\Views\Account\Register.cshtml"
if (Test-Path $registerViewPath) {
    $registerContent = Get-Content $registerViewPath -Raw
    
    if ($registerContent -match "GoogleLogin") {
 Write-Host "[✓] Google button connected in Register.cshtml" -ForegroundColor Green
    } else {
     Write-Host "[!] Google button not connected in Register.cshtml" -ForegroundColor Yellow
    }
}

$completeViewPath = "TafsilkPlatform.Web\Views\Account\CompleteGoogleRegistration.cshtml"
if (Test-Path $completeViewPath) {
 Write-Host "[✓] CompleteGoogleRegistration.cshtml exists" -ForegroundColor Green
} else {
    Write-Host "[✗] CompleteGoogleRegistration.cshtml missing!" -ForegroundColor Red
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Summary" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Update appsettings.Production.json with your actual domain" -ForegroundColor White
Write-Host "2. Add production redirect URI to Google Cloud Console:" -ForegroundColor White
Write-Host "   https://your-domain.com/signin-google" -ForegroundColor Gray
Write-Host "3. Deploy and test!" -ForegroundColor White
Write-Host ""
Write-Host "For detailed instructions, see: GOOGLE_OAUTH_PRODUCTION_CHECKLIST.md" -ForegroundColor Cyan
Write-Host ""
