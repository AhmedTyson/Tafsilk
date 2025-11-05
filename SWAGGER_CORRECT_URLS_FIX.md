# 🔧 SWAGGER FIX - IMMEDIATE ACTION REQUIRED

## **🎯 YOUR ACTUAL SWAGGER URLs**

Based on your `launchSettings.json`, your application runs on **DIFFERENT PORTS** than mentioned before:

### **✅ CORRECT SWAGGER URLS:**

```
HTTPS: https://localhost:7186/swagger
HTTP:  http://localhost:5140/swagger
```

**NOT** `localhost:5001` or `localhost:5000` - those were wrong!

---

## **🚀 IMMEDIATE FIX - RUN THIS NOW**

### **Step 1: Start the Application**

```powershell
cd "C:\Users\ahmed\source\repos\AhmedTyson\Tafsilk\TafsilkPlatform.Web"
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --launch-profile https
```

### **Step 2: Wait for Startup Message**

You should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7186
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5140
info: TafsilkPlatform.Web.Program[0]
  === Tafsilk Platform Started Successfully ===
info: TafsilkPlatform.Web.Program[0]
   🔷 Swagger UI available at: https://localhost:7186/swagger
info: TafsilkPlatform.Web.Program[0]
      🔷 Swagger JSON available at: https://localhost:7186/swagger/v1/swagger.json
```

### **Step 3: Open Swagger in Browser**

**Primary URL:**
```
https://localhost:7186/swagger
```

**Alternative HTTP:**
```
http://localhost:5140/swagger
```

**Swagger JSON (to verify it's working):**
```
https://localhost:7186/swagger/v1/swagger.json
```

---

## **📋 ONE-COMMAND START SCRIPT**

Save this as `start-swagger.ps1`:

```powershell
# Diagnostic and Start Script
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  TAFSILK SWAGGER DIAGNOSTIC START" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Stop any existing instances
Write-Host "🛑 Stopping existing instances..." -ForegroundColor Yellow
Get-Process | Where-Object {$_.ProcessName -like "*TafsilkPlatform*"} | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Navigate to project
Set-Location "C:\Users\ahmed\source\repos\AhmedTyson\Tafsilk\TafsilkPlatform.Web"

# Set environment
$env:ASPNETCORE_ENVIRONMENT = "Development"

Write-Host "✅ Environment set to: Development" -ForegroundColor Green
Write-Host ""

# Show URLs
Write-Host "📍 Your application will be available at:" -ForegroundColor Cyan
Write-Host "   Main:    https://localhost:7186" -ForegroundColor White
Write-Host "   Alt:     http://localhost:5140" -ForegroundColor White
Write-Host ""
Write-Host "📍 Swagger UI will be available at:" -ForegroundColor Cyan
Write-Host "   HTTPS:   https://localhost:7186/swagger" -ForegroundColor Green -BackgroundColor Black
Write-Host "   HTTP:  http://localhost:5140/swagger" -ForegroundColor Green -BackgroundColor Black
Write-Host ""
Write-Host "📍 Swagger JSON:" -ForegroundColor Cyan
Write-Host "   JSON:  https://localhost:7186/swagger/v1/swagger.json" -ForegroundColor White
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Start application
Write-Host "🚀 Starting application..." -ForegroundColor Green
Write-Host ""

dotnet run --launch-profile https
```

**Run it:**
```powershell
.\start-swagger.ps1
```

---

## **🔍 VERIFY IT'S WORKING - CHECKLIST**

Once the application starts:

### **Test 1: Check JSON Endpoint**
Open in browser or PowerShell:
```powershell
Invoke-WebRequest -Uri "https://localhost:7186/swagger/v1/swagger.json" -SkipCertificateCheck | Select-Object -ExpandProperty Content
```

Should return JSON starting with:
```json
{
  "openapi": "3.0.1",
  "info": {
    "title": "Tafsilk Platform API",
    "description": "Tafsilk - منصة الخياطين والتفصيل - API Documentation",
    ...
}
```

✅ If you see JSON → Swagger backend is working!

---

### **Test 2: Check Swagger UI**
Open browser:
```
https://localhost:7186/swagger
```

You should see:
- ✅ Swagger UI interface with Tafsilk branding
- ✅ "Tafsilk Platform API v1" at top
- ✅ API endpoints listed (api/auth/register, api/auth/login, etc.)
- ✅ Green "Authorize" button at top right
- ✅ Schemas section at bottom

---

### **Test 3: Test an API Endpoint**

1. Open Swagger UI: `https://localhost:7186/swagger`
2. Find **POST /api/auth/login**
3. Click "Try it out"
4. Enter test data:
```json
{
  "email": "test@example.com",
  "password": "Test123!"
}
```
5. Click "Execute"
6. Should see response (even if 401 Unauthorized)

If you can execute the request → Swagger is fully working!

---

## **❌ IF STILL NOT WORKING**

### **Symptom: 404 Not Found**

**Check these:**

1. **Environment is Development:**
```powershell
$env:ASPNETCORE_ENVIRONMENT
# Should show: Development
```

2. **Application started successfully:**
```
Look for: "Now listening on: https://localhost:7186"
```

3. **No error messages in console**

---

### **Symptom: Certificate Error**

```
Your connection is not private
NET::ERR_CERT_AUTHORITY_INVALID
```

**Fix: Trust the dev certificate**
```powershell
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

Then restart application.

---

### **Symptom: Empty Swagger UI (No Endpoints)**

**This means Swagger is working but can't find API controllers.**

**Verify your API controller:**

```powershell
Get-Content "C:\Users\ahmed\source\repos\AhmedTyson\Tafsilk\TafsilkPlatform.Web\Controllers\ApiAuthController.cs" | Select-String "ApiController"
```

Should show:
```
[ApiController]
```

If not found, the controller might not be discovered.

**Fix: Check namespace and controller base class**

---

### **Symptom: Port Already in Use**

```
IOException: Failed to bind to address https://127.0.0.1:7186
```

**Find what's using the port:**
```powershell
netstat -ano | findstr :7186
```

**Kill the process:**
```powershell
# Use the PID from above command
taskkill /F /PID <PID_NUMBER>
```

---

## **🧪 COMPLETE DIAGNOSTIC SCRIPT**

Run this to diagnose ALL issues:

```powershell
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  SWAGGER DIAGNOSTIC TOOL" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# 1. Check if app is running
Write-Host "1. Checking for running instances..." -ForegroundColor Yellow
$processes = Get-Process | Where-Object {$_.ProcessName -like "*TafsilkPlatform*"}
if ($processes) {
    Write-Host "   ❌ Found running instances:" -ForegroundColor Red
    $processes | Format-Table ProcessName, Id, StartTime
    Write-Host "   Action: Stop them first!" -ForegroundColor Red
} else {
    Write-Host "   ✅ No running instances" -ForegroundColor Green
}
Write-Host ""

# 2. Check environment
Write-Host "2. Checking environment..." -ForegroundColor Yellow
$envCheck = $env:ASPNETCORE_ENVIRONMENT
if ($envCheck -eq "Development") {
    Write-Host "   ✅ Environment: $envCheck" -ForegroundColor Green
} else {
    Write-Host "   ❌ Environment: $envCheck (Should be Development)" -ForegroundColor Red
    Write-Host "   Action: Set with: `$env:ASPNETCORE_ENVIRONMENT = 'Development'" -ForegroundColor Red
}
Write-Host ""

# 3. Check if ports are available
Write-Host "3. Checking port availability..." -ForegroundColor Yellow
$port7186 = netstat -ano | findstr ":7186"
$port5140 = netstat -ano | findstr ":5140"

if ($port7186) {
    Write-Host " ❌ Port 7186 is in use:" -ForegroundColor Red
  Write-Host "      $port7186" -ForegroundColor Red
} else {
    Write-Host "   ✅ Port 7186 available" -ForegroundColor Green
}

if ($port5140) {
    Write-Host "   ❌ Port 5140 is in use:" -ForegroundColor Red
    Write-Host "      $port5140" -ForegroundColor Red
} else {
    Write-Host "   ✅ Port 5140 available" -ForegroundColor Green
}
Write-Host ""

# 4. Check HTTPS certificate
Write-Host "4. Checking HTTPS certificate..." -ForegroundColor Yellow
try {
    $cert = dotnet dev-certs https --check 2>&1
    if ($cert -like "*trusted*" -or $cert -like "*valid*") {
        Write-Host "   ✅ HTTPS certificate is valid" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️  Certificate status unclear" -ForegroundColor Yellow
        Write-Host "   Action: Run: dotnet dev-certs https --trust" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ❌ Could not check certificate" -ForegroundColor Red
}
Write-Host ""

# 5. Check if project builds
Write-Host "5. Checking if project builds..." -ForegroundColor Yellow
Set-Location "C:\Users\ahmed\source\repos\AhmedTyson\Tafsilk\TafsilkPlatform.Web"
$buildResult = dotnet build --no-restore 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Project builds successfully" -ForegroundColor Green
} else {
    Write-Host "   ❌ Build failed" -ForegroundColor Red
    Write-Host "   Last error:" -ForegroundColor Red
    $buildResult | Select-Object -Last 5 | ForEach-Object { Write-Host "      $_" -ForegroundColor Red }
}
Write-Host ""

# 6. Check for API controllers
Write-Host "6. Checking for API controllers..." -ForegroundColor Yellow
$apiControllers = Get-ChildItem "Controllers\*ApiController.cs" -ErrorAction SilentlyContinue
if ($apiControllers) {
    Write-Host "   ✅ Found API controllers:" -ForegroundColor Green
    $apiControllers | ForEach-Object { Write-Host "      - $($_.Name)" -ForegroundColor White }
} else {
  Write-Host "   ⚠️  No API controllers found" -ForegroundColor Yellow
}
Write-Host ""

# 7. Summary
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  DIAGNOSTIC SUMMARY" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

if (!$processes -and $envCheck -eq "Development" -and !$port7186 -and !$port5140 -and $LASTEXITCODE -eq 0) {
    Write-Host "✅ ALL CHECKS PASSED!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Ready to start! Run:" -ForegroundColor Green
    Write-Host "   dotnet run --launch-profile https" -ForegroundColor White
    Write-Host ""
    Write-Host "Then open:" -ForegroundColor Green
    Write-Host "   https://localhost:7186/swagger" -ForegroundColor Cyan
} else {
    Write-Host "⚠️  Some issues detected. Fix the items marked ❌ above." -ForegroundColor Yellow
}

Write-Host ""
```

Save as `diagnose-swagger.ps1` and run:
```powershell
.\diagnose-swagger.ps1
```

---

## **✅ EXPECTED WORKING STATE**

When everything is working:

### **Console Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7186
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5140
info: TafsilkPlatform.Web.Program[0]
    === Tafsilk Platform Started Successfully ===
info: TafsilkPlatform.Web.Program[0]
      Environment: Development
info: TafsilkPlatform.Web.Program[0]
      Authentication Schemes: Cookies, JWT, Google
info: TafsilkPlatform.Web.Program[0]
    🔷 Swagger UI available at: https://localhost:7186/swagger
info: TafsilkPlatform.Web.Program[0]
      🔷 Swagger JSON available at: https://localhost:7186/swagger/v1/swagger.json
```

### **Swagger UI (https://localhost:7186/swagger):**
- Shows "Tafsilk Platform API v1"
- Lists endpoints:
  - POST /api/auth/register
  - POST /api/auth/login
  - GET /api/auth/me
  - POST /api/auth/refresh
  - POST /api/auth/logout
- Green "Authorize" button visible
- Can click "Try it out" on any endpoint

### **Swagger JSON (https://localhost:7186/swagger/v1/swagger.json):**
- Returns valid JSON
- Contains OpenAPI 3.0.1 specification
- Lists all API endpoints

---

## **🎊 QUICK START COMMAND**

```powershell
cd "C:\Users\ahmed\source\repos\AhmedTyson\Tafsilk\TafsilkPlatform.Web"; $env:ASPNETCORE_ENVIRONMENT="Development"; dotnet run --launch-profile https
```

Then open: **https://localhost:7186/swagger**

---

## **📞 IF STILL FAILING**

### **Last Resort Checklist:**

1. ✅ Application stopped completely
2. ✅ Environment = Development
3. ✅ Ports 7186 and 5140 free
4. ✅ Build successful
5. ✅ HTTPS certificate trusted
6. ✅ Using correct URL: `https://localhost:7186/swagger`
7. ✅ API controllers exist in Controllers folder
8. ✅ No firewall blocking
9. ✅ Using correct browser (Chrome/Edge)
10. ✅ Not using incognito/private mode

---

## **🎯 SUMMARY**

**Problem:** You were using wrong URLs (`localhost:5001/5000`)  
**Actual URLs:** `localhost:7186` and `localhost:5140`  
**Solution:** Use correct URLs  
**Status:** Configuration is correct, just need right URL  

**Correct Swagger URL:**
```
https://localhost:7186/swagger
```

---

**Date:** 2025-01-20  
**Status:** Ready to test with correct URLs  
**Action:** Run app and open https://localhost:7186/swagger

---

**🎉 Swagger will work with the correct URL!**
