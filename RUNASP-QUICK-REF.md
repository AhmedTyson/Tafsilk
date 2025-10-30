# 🚀 RunASP.NET Quick Deploy - Command Reference

## ⚡ FASTEST WAYS TO PUBLISH

### **1. One-Click Deployment (Recommended)**
```
Double-click: publish-quick.bat
```
**Time:** 30-60 seconds

---

### **2. PowerShell Automated**
```powershell
.\publish-runasp.ps1
```
**Time:** 45 seconds (includes verification)

---

### **3. Direct Command**
```bash
dotnet publish TafsilkPlatform.Web\TafsilkPlatform.Web.csproj -c Release /p:PublishProfile=RunASP /p:Password=oC@3D4w_+K7i
```
**Time:** 30 seconds

---

## 🎯 YOUR SITE DETAILS

| Item | Value |
|------|-------|
| **Live Site** | http://tafsilk.runasp.net |
| **Control Panel** | https://panel.runasp.net |
| **Username** | site41423 |
| **Password** | oC@3D4w_+K7i |

---

## ⚙️ FIRST-TIME SETUP (5 minutes)

After first deployment, configure in control panel:

### **1. Connection String**
```
Panel → Databases → SQL Server
Copy connection string
Panel → Configuration → Connection Strings
Name: DefaultConnection
Paste connection string
```

### **2. Environment Variables**
```
Panel → Configuration → Application Settings

Add these:
ASPNETCORE_ENVIRONMENT = Production
Jwt__Key = [Generate: openssl rand -base64 32]
Google__client_id = [From Google Console]
Google__client_secret = [From Google Console]
Facebook__app_id = [From Facebook Developer]
Facebook__app_secret = [From Facebook Developer]
```

### **3. OAuth Redirect URLs**
- **Google:** http://tafsilk.runasp.net/signin-google
- **Facebook:** http://tafsilk.runasp.net/signin-facebook

---

## 🚦 DEPLOYMENT WORKFLOW

```
1. Make code changes
   ↓
2. Test locally: dotnet run
   ↓
3. Publish: .\publish-runasp.ps1
   ↓
4. Browser opens automatically
   ↓
5. Verify site works
```

**Total time:** < 2 minutes

---

## 💨 SPEED OPTIMIZATIONS

### **After First Build:**
```powershell
# Skip build step (much faster!)
.\publish-runasp.ps1 -SkipBuild
```
**Saves:** 20-30 seconds

### **For Troubleshooting:**
```powershell
# See detailed output
.\publish-runasp.ps1 -Verbose
```

---

## 🔧 TROUBLESHOOTING

### **If publish fails:**
```powershell
# 1. Check connection
Test-NetConnection site41423.siteasp.net -Port 8172

# 2. Verify credentials
# Username: site41423
# Password: oC@3D4w_+K7i

# 3. Try with verbose output
.\publish-runasp.ps1 -Verbose
```

### **If site shows error:**
```
1. Check RunASP control panel logs
2. Verify connection string configured
3. Ensure environment variables set
4. Check .NET 9 runtime enabled
```

---

## 📊 PERFORMANCE TARGETS

| Action | Target Time |
|--------|-------------|
| First publish | 60 seconds |
| Subsequent publish | 30-40 seconds |
| With `-SkipBuild` | 20-30 seconds |
| Visual Studio | 90-120 seconds |

---

## ✅ QUICK CHECKLIST

Before publishing:
- [ ] Code changes committed
- [ ] Local tests passed
- [ ] Release build successful

After publishing:
- [ ] Site loads correctly
- [ ] Database connected
- [ ] Login works
- [ ] OAuth configured (if needed)

---

## 🎯 ONE-LINER COMMANDS

```powershell
# Full automated deploy
.\publish-runasp.ps1

# Fast deploy (pre-built)
.\publish-runasp.ps1 -SkipBuild

# With detailed output
.\publish-runasp.ps1 -Verbose

# Direct command
dotnet publish -c Release /p:PublishProfile=RunASP /p:Password=oC@3D4w_+K7i
```

---

## 📞 SUPPORT LINKS

- **Your Site:** http://tafsilk.runasp.net
- **Panel:** https://panel.runasp.net
- **Support:** https://support.runasp.net
- **Docs:** See `RUNASP-DEPLOYMENT.md`

---

**Status:** ✅ Ready to publish now!  
**Estimated Time:** 30-60 seconds  
**Next Step:** Run `.\publish-runasp.ps1`
