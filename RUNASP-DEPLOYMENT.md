# 🚀 RunASP.NET Deployment Guide - Ultra Fast Publishing

## ✅ Your Deployment Details

| Setting | Value |
|---------|-------|
| **Site URL** | http://tafsilk.runasp.net |
| **Control Panel** | https://panel.runasp.net |
| **Publish Server** | site41423.siteasp.net |
| **Site Name** | site41423 |
| **Username** | site41423 |
| **Framework** | .NET 9.0 |

---

## ⚡ FASTEST METHOD - One Command

### **Windows (Double-click):**
```
publish-quick.bat
```

### **PowerShell (Recommended):**
```powershell
.\publish-runasp.ps1
```

### **Command Line (Direct):**
```bash
dotnet publish TafsilkPlatform.Web\TafsilkPlatform.Web.csproj `
  -c Release `
  /p:PublishProfile=RunASP `
  /p:Password=oC@3D4w_+K7i
```

**Deployment Time:** ~30-60 seconds

---

## 🎯 PUBLISHING OPTIONS

### **Option 1: Visual Studio (GUI)**

1. Right-click `TafsilkPlatform.Web` project
2. Select **Publish**
3. Click **Import Profile**
4. Browse to publish profile file
5. Click **Publish**

**Time:** 2 minutes

---

### **Option 2: PowerShell Script (Optimized)**

```powershell
# Full deployment with verification
.\publish-runasp.ps1

# Skip build (if already built)
.\publish-runasp.ps1 -SkipBuild

# Verbose output for troubleshooting
.\publish-runasp.ps1 -Verbose
```

**Features:**
- ✅ Automated build
- ✅ Optimized publishing
- ✅ Site verification
- ✅ Opens browser automatically
- ✅ Progress indicators

**Time:** 45 seconds

---

### **Option 3: One-Click Batch File**

```cmd
publish-quick.bat
```

**Features:**
- ✅ Single double-click
- ✅ No parameters needed
- ✅ Automatic browser launch
- ✅ Error handling

**Time:** 30 seconds

---

## 🔧 RUNASP CONFIGURATION

### **After First Deployment:**

1. **Login to Control Panel:**
   - Visit: https://panel.runasp.net
   - Username: `site41423`
   - Password: `oC@3D4w_+K7i`

2. **Configure Connection String:**
   ```
   Go to: Databases → SQL Server
   Copy connection string
   Add to: Configuration → Connection Strings
   Name: DefaultConnection
   ```

3. **Set Environment Variables:**
   ```
   Configuration → Application Settings
   
   Add:
   - ASPNETCORE_ENVIRONMENT = Production
   - Jwt__Key = YOUR_JWT_SECRET_KEY
- Google__client_id = YOUR_GOOGLE_ID
   - Google__client_secret = YOUR_GOOGLE_SECRET
   - Facebook__app_id = YOUR_FACEBOOK_ID
   - Facebook__app_secret = YOUR_FACEBOOK_SECRET
   ```

4. **Configure OAuth Redirect URLs:**
   - Google Console: `http://tafsilk.runasp.net/signin-google`
   - Facebook Developer: `http://tafsilk.runasp.net/signin-facebook`

---

## 📊 OPTIMIZATION TIPS FOR FASTER DEPLOYMENT

### **1. Pre-build for Speed:**
```powershell
# Build once
dotnet build TafsilkPlatform.Web\TafsilkPlatform.Web.csproj -c Release

# Then publish with --no-build
dotnet publish TafsilkPlatform.Web\TafsilkPlatform.Web.csproj `
  -c Release `
  /p:PublishProfile=RunASP `
  /p:Password=oC@3D4w_+K7i `
  --no-build
```

**Saves:** 20-30 seconds per publish

---

### **2. Skip Unchanged Files:**
```powershell
# Only uploads changed files
dotnet publish TafsilkPlatform.Web\TafsilkPlatform.Web.csproj `
  -c Release `
  /p:PublishProfile=RunASP `
  /p:Password=oC@3D4w_+K7i `
  /p:SkipExtraFilesOnServer=true
```

**Saves:** 15-20 seconds on subsequent deploys

---

### **3. Incremental Builds:**
```powershell
# Don't clean before build
dotnet publish TafsilkPlatform.Web\TafsilkPlatform.Web.csproj `
  -c Release `
  /p:PublishProfile=RunASP `
  /p:Password=oC@3D4w_+K7i `
  --no-restore
```

**Saves:** 10-15 seconds

---

## 🔍 TROUBLESHOOTING

### **Issue: "Could not connect to remote server"**

**Solution:**
```powershell
# Test Web Deploy connectivity
Test-NetConnection -ComputerName site41423.siteasp.net -Port 8172
```

**If fails:**
- Check firewall settings
- Verify internet connection
- Try from different network

---

### **Issue: "Authentication failed"**

**Solution:**
- Verify password: `oC@3D4w_+K7i`
- Check username: `site41423`
- Re-import publish profile

---

### **Issue: "Site shows 500 error"**

**Solution:**
1. Check RunASP control panel logs
2. Verify connection string is configured
3. Ensure environment variables are set
4. Check .NET 9 runtime is enabled

---

### **Issue: "Deployment is slow"**

**Solutions:**
- Use `publish-runasp.ps1 -SkipBuild` after first build
- Enable `SkipExtraFilesOnServer=true`
- Check your internet speed
- Try deploying during off-peak hours

---

## 📋 POST-DEPLOYMENT CHECKLIST

After deployment, verify:

- [ ] Site loads: http://tafsilk.runasp.net
- [ ] Database connection working
- [ ] Login page accessible
- [ ] OAuth redirects configured
- [ ] Static files loading (CSS/JS)
- [ ] HTTPS redirect working (if configured)

---

## 🚦 DEPLOYMENT WORKFLOW

### **Development Cycle:**

```powershell
# 1. Make changes to code
# 2. Test locally
dotnet run --project TafsilkPlatform.Web

# 3. Build Release
dotnet build TafsilkPlatform.Web\TafsilkPlatform.Web.csproj -c Release

# 4. Publish to RunASP
.\publish-runasp.ps1 -SkipBuild

# 5. Verify deployment
# Opens automatically in browser
```

**Total Time:** ~1 minute from code change to live

---

## 📊 PERFORMANCE METRICS

### **Expected Deployment Times:**

| Method | First Deploy | Subsequent Deploys |
|--------|-------------|-------------------|
| Visual Studio | 2-3 min | 1-2 min |
| PowerShell Script | 60-90 sec | 30-45 sec |
| Quick Batch | 45-60 sec | 30-40 sec |
| Command Line | 30-60 sec | 20-30 sec |

**Optimized Times (with flags):**
- With `--no-build`: ~20-30 seconds
- With `--no-restore`: ~15-25 seconds
- With both: ~15-20 seconds

---

## 🔐 SECURITY NOTES

### **Password Management:**

**⚠️ Important:** The publish profile contains your password in plain text.

**Best Practices:**
1. **Don't commit to Git:**
 ```
   # Already in .gitignore
   *.pubxml.user
   ```

2. **Use Environment Variables:**
   ```powershell
   $env:RUNASP_PASSWORD = "oC@3D4w_+K7i"
   dotnet publish ... /p:Password=$env:RUNASP_PASSWORD
   ```

3. **Rotate Password Regularly:**
   - Change in RunASP control panel
   - Update publish profile

---

## 🌐 CUSTOM DOMAIN SETUP

### **If you have a custom domain:**

1. **Add Domain in RunASP:**
   - Control Panel → Domains → Add Domain
   - Enter your domain name

2. **Update DNS Records:**
   ```
   Type: CNAME
   Host: www
   Value: site41423.siteasp.net
   
   Type: A
   Host: @
   Value: [Get from RunASP panel]
   ```

3. **Update Publish Profile:**
   ```xml
   <SiteUrlToLaunchAfterPublish>https://yourdomain.com</SiteUrlToLaunchAfterPublish>
   ```

---

## 🎯 QUICK REFERENCE

### **Essential Commands:**

```powershell
# Quick publish
.\publish-runasp.ps1

# Fastest publish (pre-built)
.\publish-runasp.ps1 -SkipBuild

# Detailed output
.\publish-runasp.ps1 -Verbose

# Direct command
dotnet publish -c Release /p:PublishProfile=RunASP /p:Password=oC@3D4w_+K7i
```

### **Essential URLs:**

- **Your Site:** http://tafsilk.runasp.net
- **Control Panel:** https://panel.runasp.net
- **Support:** https://support.runasp.net

---

## ✅ YOU'RE READY!

**To deploy right now:**

```powershell
# Option 1: Full automated deployment
.\publish-runasp.ps1

# Option 2: One-click deployment
# Double-click: publish-quick.bat

# Option 3: Quick command
dotnet publish -c Release /p:PublishProfile=RunASP /p:Password=oC@3D4w_+K7i
```

**Your site will be live at:** http://tafsilk.runasp.net

---

**Status:** ✅ Ready to publish  
**Estimated Time:** 30-60 seconds  
**Files Created:** 3 (publish profile + 2 scripts)
