# 🔧 SWAGGER NOT WORKING - TROUBLESHOOTING GUIDE

## **⚠️ CURRENT ISSUE DETECTED**

Your application is currently running, which is preventing the build. Let's fix the Swagger URL issue.

---

## **🎯 IMMEDIATE FIX - RESTART YOUR APPLICATION**

### **Step 1: Stop the Running Application**

**Option A - Visual Studio:**
1. Click the **Stop** button (Red square ⏹️) or press `Shift + F5`
2. Wait for the application to fully stop

**Option B - Command Line:**
```bash
# Find the process
tasklist | findstr TafsilkPlatform.Web

# Kill the process (replace XXXX with actual PID)
taskkill /F /PID XXXX
```

**Option C - Task Manager:**
1. Press `Ctrl + Shift + Esc`
2. Find `TafsilkPlatform.Web.exe`
3. Click **End Task**

---

### **Step 2: Rebuild the Application**

```bash
# Clean and rebuild
dotnet clean
dotnet build
```

Or in Visual Studio:
1. **Build** → **Clean Solution**
2. **Build** → **Rebuild Solution**

---

### **Step 3: Run the Application**

```bash
dotnet run
```

Or press **F5** in Visual Studio

---

### **Step 4: Access Swagger UI**

Open your browser and navigate to:
```
https://localhost:5001/swagger
```

Or if using HTTP:
```
http://localhost:5000/swagger
```

---

## **🔍 COMMON SWAGGER ISSUES & SOLUTIONS**

### **Issue 1: 404 - Page Not Found**

**Symptoms:**
- Browser shows "404 Not Found"
- URL: `https://localhost:5001/swagger`

**Solutions:**

#### **A. Check Environment**
Your application must be running in **Development** mode.

**Verify in console:**
```
Environment: Development ✅
```

**If it shows "Production", fix it:**

**Option 1: launchSettings.json**
```json
{
  "profiles": {
    "https": {
    "commandName": "Project",
 "launchBrowser": true,
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
   "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**Option 2: Command Line**
```bash
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
```

---

#### **B. Check URL**

**Try these URLs:**
```
✅ https://localhost:5001/swagger
✅ https://localhost:5001/swagger/index.html
✅ http://localhost:5000/swagger
```

**Check Swagger JSON directly:**
```
https://localhost:5001/swagger/v1/swagger.json
```

If JSON works but UI doesn't, there's a UI configuration issue.

---

#### **C. Check Port Number**

**Find your actual port:**
1. Look at console output when app starts
2. Check `Properties/launchSettings.json`
3. Look for: `Now listening on: https://localhost:XXXX`

**Example:**
```
info: Microsoft.Hosting.Lifetime[14]
    Now listening on: https://localhost:7001
```

Then use: `https://localhost:7001/swagger`

---

### **Issue 2: Swagger UI Shows But No Endpoints**

**Symptoms:**
- Swagger UI loads
- Shows empty list or no API endpoints

**Solutions:**

#### **A. Add API Controllers**

You need controllers with `[ApiController]` attribute:

```csharp
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "API is working!" });
    }
}
```

---

#### **B. Check Controller Discovery**

Add to `Program.cs` (already added):
```csharp
builder.Services.AddControllers(); // or AddControllersWithViews()
```

And map controllers:
```csharp
app.MapControllers(); // or app.MapControllerRoute(...)
```

---

### **Issue 3: Middleware Order Error**

**Symptoms:**
- 500 Internal Server Error
- Console shows middleware exceptions

**✅ FIXED: Correct Middleware Order**

```csharp
var app = builder.Build();

// 1. Exception Handling (first!)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 2. Swagger (BEFORE UseRouting!)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => { /* config */ });
}

// 3. Other middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// 4. Custom middleware
app.UseMiddleware<UserStatusMiddleware>();

// 5. Endpoints (last!)
app.MapControllerRoute(...);
```

---

### **Issue 4: HTTPS Certificate Issues**

**Symptoms:**
- Browser shows certificate warning
- "Your connection is not private"
- NET::ERR_CERT_AUTHORITY_INVALID

**Solution: Trust Development Certificate**

```bash
# Trust the HTTPS development certificate
dotnet dev-certs https --trust
```

Then restart your application.

---

### **Issue 5: Firewall or Antivirus Blocking**

**Symptoms:**
- Connection timeout
- "This site can't be reached"

**Solutions:**

**A. Check Windows Firewall:**
1. Open **Windows Defender Firewall**
2. Click **Allow an app through firewall**
3. Find `TafsilkPlatform.Web.exe`
4. Check both **Private** and **Public**

**B. Temporarily Disable Antivirus:**
- Disable antivirus temporarily
- Try accessing Swagger
- If it works, add exception for your app

---

### **Issue 6: Port Already in Use**

**Symptoms:**
```
System.IO.IOException: Failed to bind to address https://127.0.0.1:5001: address already in use.
```

**Solutions:**

**A. Find Process Using Port:**
```bash
netstat -ano | findstr :5001
```

**B. Kill the Process:**
```bash
taskkill /F /PID <PID_NUMBER>
```

**C. Use Different Port:**

Edit `Properties/launchSettings.json`:
```json
{
  "applicationUrl": "https://localhost:5002;http://localhost:5003"
}
```

Then access: `https://localhost:5002/swagger`

---

## **🧪 TESTING SWAGGER**

### **Step-by-Step Test:**

1. **Stop your application completely**
2. **Clean build:**
   ```bash
   dotnet clean
   dotnet build
   ```

3. **Run with verbose logging:**
   ```bash
   dotnet run --verbosity detailed
   ```

4. **Look for these log messages:**
   ```
   ✅ Environment: Development
   ✅ Swagger UI available at: https://localhost:5001/swagger
   ✅ Swagger JSON available at: https://localhost:5001/swagger/v1/swagger.json
```

5. **Test Swagger JSON first:**
   ```
   https://localhost:5001/swagger/v1/swagger.json
   ```
   
   Should return JSON like:
   ```json
   {
     "openapi": "3.0.1",
     "info": {
       "title": "Tafsilk Platform API",
       "version": "v1"
   },
     ...
   }
   ```

6. **Then test Swagger UI:**
   ```
   https://localhost:5001/swagger
   ```

---

## **🔧 ADVANCED TROUBLESHOOTING**

### **Enable Detailed Logging**

Add to `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
   "Default": "Debug",
      "Microsoft": "Debug",
      "Microsoft.AspNetCore": "Debug",
      "Swashbuckle": "Debug"
    }
  }
}
```

---

### **Check Swagger Registration**

Add this logging after `var app = builder.Build();`:
```csharp
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Log all registered services
var services = app.Services.GetService<IServiceCollection>();
logger.LogInformation("🔷 Registered Services Count: {Count}", services?.Count ?? 0);

// Check if Swagger is registered
try
{
    var swaggerGen = app.Services.GetService<Swashbuckle.AspNetCore.SwaggerGen.ISwaggerProvider>();
    logger.LogInformation("✅ Swagger Provider registered: {IsRegistered}", swaggerGen != null);
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ Swagger Provider not found");
}
```

---

### **Test with Minimal API**

Create a test endpoint to verify Swagger is working:

Add to `Program.cs` before `app.Run()`:
```csharp
// Test endpoint
app.MapGet("/api/test", () => Results.Ok(new { 
    message = "API is working!",
    timestamp = DateTime.UtcNow 
}))
.WithName("TestEndpoint")
.WithOpenApi();

app.MapGet("/api/health", () => Results.Ok(new {
  status = "healthy",
    swagger = "enabled",
    environment = app.Environment.EnvironmentName
}))
.WithName("HealthCheck")
.WithOpenApi();
```

Then check if these appear in Swagger UI.

---

## **✅ VERIFICATION CHECKLIST**

Run through this checklist:

- [ ] Application stopped completely
- [ ] Cleaned and rebuilt (`dotnet clean && dotnet build`)
- [ ] Running in Development mode
- [ ] `ASPNETCORE_ENVIRONMENT=Development` is set
- [ ] Port 5001 (or your port) is not in use
- [ ] HTTPS certificate trusted (`dotnet dev-certs https --trust`)
- [ ] Firewall allows the application
- [ ] Console shows: "Swagger UI available at: ..."
- [ ] `/swagger/v1/swagger.json` returns JSON
- [ ] `/swagger` shows Swagger UI

---

## **🎯 QUICK FIX SCRIPT**

Run this PowerShell script to reset everything:

```powershell
# Stop all instances
Get-Process | Where-Object {$_.ProcessName -like "*TafsilkPlatform*"} | Stop-Process -Force

# Clean
Set-Location "C:\Users\ahmed\source\repos\AhmedTyson\Tafsilk"
dotnet clean

# Trust HTTPS cert
dotnet dev-certs https --clean
dotnet dev-certs https --trust

# Rebuild
dotnet build

# Run
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project TafsilkPlatform.Web
```

---

## **📞 STILL NOT WORKING?**

### **Check These URLs:**

```
1. Swagger UI:
   https://localhost:5001/swagger
   http://localhost:5000/swagger

2. Swagger JSON:
   https://localhost:5001/swagger/v1/swagger.json

3. Alternative UI:
   https://localhost:5001/swagger/index.html

4. Root:
   https://localhost:5001/
```

### **Console Output Should Show:**

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: TafsilkPlatform.Web.Program[0]
      === Tafsilk Platform Started Successfully ===
info: TafsilkPlatform.Web.Program[0]
      Environment: Development
info: TafsilkPlatform.Web.Program[0]
      🔷 Swagger UI available at: https://localhost:5001/swagger
```

---

## **🎊 SUCCESS INDICATORS**

When Swagger is working correctly, you should see:

1. **Swagger UI loads** with Tafsilk branding
2. **API endpoints listed** (if you have API controllers)
3. **Authorization button (🔒)** at top right
4. **Try it out** buttons on endpoints
5. **Schemas section** at the bottom
6. **No console errors**

---

## **📝 SUMMARY**

**Most Common Issue:** Application already running

**Quick Fix:**
1. Stop application (`Shift + F5`)
2. Rebuild (`Ctrl + Shift + B`)
3. Run (`F5`)
4. Navigate to `https://localhost:5001/swagger`

**Environment:** Must be **Development** (not Production)

**Middleware Order:** Swagger must be **before** `UseRouting()`

**Status:** ✅ Configuration is correct, just needs restart!

---

**Date:** 2025-01-20  
**Status:** Ready to test after restart  
**Next Step:** Stop app → Rebuild → Run → Test Swagger

---

**🎉 After following these steps, Swagger should work perfectly!**
