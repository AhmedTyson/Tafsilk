# 🎯 Simple Setup Guide - No Thinking Required!

## ⚡ Super Quick Start (Copy & Paste)

### Windows (PowerShell)
```powershell
# 1. Set JWT Key
dotnet user-secrets set "Jwt:Key" "TafsilkPlatform_SuperSecretKey_2024_Minimum32Chars!"

# 2. Run
dotnet run

# 3. Open browser to: https://localhost:7186
```

### Linux/Mac
```bash
# 1. Set JWT Key
dotnet user-secrets set "Jwt:Key" "TafsilkPlatform_SuperSecretKey_2024_Minimum32Chars!"

# 2. Run
dotnet run

# 3. Open browser to: https://localhost:7186
```

**Done!** Database creates itself automatically.

---

## 🔑 Login After First Run

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@tafsilk.local` | `Admin@123!` |
| Tester | `tester@tafsilk.local` | `Tester@123!` |

---

## 🛠️ Common Commands

```bash
# Run the app
dotnet run

# Build the app
dotnet build

# Update database
dotnet ef database update

# View logs
cat Logs/log-*.txt  # Linux/Mac
type Logs\log-*.txt  # Windows
```

---

## ⚙️ Optional Configuration

### Change Admin Email/Password
```bash
dotnet user-secrets set "Admin:Email" "your-email@example.com"
dotnet user-secrets set "Admin:Password" "YourSecurePassword123!"
```

### Change Database
Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YourConnectionString"
  }
}
```

---

## 🐛 Problems?

### "JWT Key not configured"
**Fix**: Run `dotnet user-secrets set "Jwt:Key" "YourKeyHere"`

### "Cannot connect to database"
**Fix**: 
1. Make sure SQL Server is running
2. Check connection string in `appsettings.Development.json`

### "Port in use"
**Fix**: Change port in `Properties/launchSettings.json`

---

## 📍 Important Files

- **Configuration**: `appsettings.json`, `appsettings.Development.json`
- **Database**: Auto-created on first run
- **Logs**: `Logs/` folder (auto-created)
- **Secrets**: Stored in User Secrets (not in files)

---

## ✅ What You Get

- ✅ User registration & login
- ✅ Admin dashboard
- ✅ E-commerce (products, cart, checkout)
- ✅ Order management
- ✅ API documentation at `/swagger`
- ✅ Health checks at `/health`

---

**That's it!** Everything else is automatic. 🚀

