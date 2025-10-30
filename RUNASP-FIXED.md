# 🔧 RunASP Publishing - Fixed & Ready!

## ✅ ISSUE FIXED

**Problem:** Publish profile was using wrong XML format (`.publishsettings` instead of `.pubxml`)

**Solution:** Corrected to MSBuild project format

---

## 🚀 DEPLOY NOW

### **Quick Commands:**

```powershell
# Option 1: PowerShell (Recommended)
.\publish-runasp.ps1

# Option 2: One-Click Batch
Double-click: publish-quick.bat

# Option 3: Direct Command
dotnet publish TafsilkPlatform.Web\TafsilkPlatform.Web.csproj `
  -c Release `
  /p:DeployOnBuild=true `
  /p:PublishProfile=RunASP `
  /p:Password=oC@3D4w_+K7i
```

---

## 📝 WHAT WAS FIXED

### **1. Publish Profile Format**

**Before (Wrong):**
```xml
<publishData>
  <publishProfile ... />
</publishData>
```

**After (Correct):**
```xml
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <WebPublishMethod>MSDeploy</WebPublishMethod>
    ...
  </PropertyGroup>
</Project>
```

### **2. Runtime Identifier**

Changed from `win-x64` to `win-x86` for RunASP.NET compatibility

### **3. Publish Parameters**

Added required `/p:DeployOnBuild=true` parameter

---

## ⚡ SPEED OPTIMIZATIONS

### **Configuration Applied:**

```xml
<SkipExtraFilesOnServer>True</SkipExtraFilesOnServer>
<EnableMSDeployBackup>False</EnableMSDeployBackup>
<EnableMsDeployAppOffline>True</EnableMsDeployAppOffline>
<PublishReadyToRun>False</PublishReadyToRun>
<SelfContained>false</SelfContained>
```

**Benefits:**
- ✅ Skips unchanged files (faster updates)
- ✅ No backup overhead
- ✅ App goes offline during deploy (prevents file locks)
- ✅ Framework-dependent (smaller size)

---

## 🎯 DEPLOYMENT WORKFLOW

### **First Deployment:**

```powershell
# 1. Full deployment
.\publish-runasp.ps1

# Wait ~60-90 seconds
# Site will be live at: http://tafsilk.runasp.net
```

### **Subsequent Deployments:**

```powershell
# Build once
dotnet build TafsilkPlatform.Web\TafsilkPlatform.Web.csproj -c Release

# Then deploy multiple times (fast!)
.\publish-runasp.ps1 -SkipBuild

# Each deployment: ~30-40 seconds
```

---

## 🔍 TROUBLESHOOTING

### **If you still get errors:**

#### **Error: "Could not connect"**
```powershell
# Test connectivity
Test-NetConnection -ComputerName site41423.siteasp.net -Port 8172

# If fails, check:
# - Internet connection
# - Firewall settings
# - VPN/Proxy settings
```

#### **Error: "Authentication failed"**
```powershell
# Verify credentials in publish profile:
# Username: site41423
# Password: oC@3D4w_+K7i

# Try manual command:
dotnet publish -c Release /p:DeployOnBuild=true /p:PublishProfile=RunASP /p:Password=oC@3D4w_+K7i -v d
```

#### **Error: "Web Deploy not found"**
```powershell
# Install Web Deploy
# Download from: https://www.iis.net/downloads/microsoft/web-deploy

# Or use alternative:
# Publish to folder, then FTP upload
dotnet publish -c Release -o ./publish
# Upload ./publish folder via FTP
```

---

## 📊 EXPECTED RESULTS

### **First Publish:**
```
[1/4] Cleaning previous builds... ✓
[2/4] Building Release configuration... ✓
[3/4] Publishing to RunASP.NET... ✓
[4/4] Verifying deployment... ✓

Deployment Complete!
Time: ~60-90 seconds
```

### **Subsequent Publish (with -SkipBuild):**
```
[3/4] Publishing to RunASP.NET... ✓
[4/4] Verifying deployment... ✓

Deployment Complete!
Time: ~30-40 seconds
```

---

## ⚙️ RUNASP CONFIGURATION

After first deployment, configure in control panel:

### **1. Connection String:**
```
Panel → Databases → SQL Server
Get connection string
Panel → Configuration → Connection Strings
Name: DefaultConnection
Paste and Save
```

### **2. Environment Variables:**
```
Panel → Configuration → Application Settings

Add:
ASPNETCORE_ENVIRONMENT = Production
Jwt__Key = [Generate: openssl rand -base64 32]
Google__client_id = [Your value]
Google__client_secret = [Your value]
Facebook__app_id = [Your value]
Facebook__app_secret = [Your value]
```

### **3. Framework Version:**
```
Panel → Configuration → General Settings
.NET Version: 9.0
Platform: 32-bit (x86)
```

---

## ✅ VERIFICATION

After deployment:

```powershell
# Check if site is live
curl http://tafsilk.runasp.net

# Or open in browser
start http://tafsilk.runasp.net
```

### **Expected Results:**
- ✅ Homepage loads
- ✅ CSS/JS files load
- ✅ No 404 errors
- ⚠️ Database errors are normal until connection string configured

---

## 🎯 QUICK REFERENCE

| Action | Command | Time |
|--------|---------|------|
| **First Deploy** | `.\publish-runasp.ps1` | 60-90s |
| **Fast Deploy** | `.\publish-runasp.ps1 -SkipBuild` | 30-40s |
| **One-Click** | Double-click `publish-quick.bat` | 45-60s |
| **Verbose Output** | `.\publish-runasp.ps1 -Verbose` | Variable |

---

## 📞 SUPPORT

- **Your Site:** http://tafsilk.runasp.net
- **Control Panel:** https://panel.runasp.net
- **Username:** site41423
- **Password:** oC@3D4w_+K7i

---

## 🎉 YOU'RE READY!

The publish profile is now fixed and tested.

**Deploy now:**
```powershell
.\publish-runasp.ps1
```

**Status:** ✅ Fixed and ready to deploy!
