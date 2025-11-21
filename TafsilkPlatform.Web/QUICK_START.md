# ⚡ Quick Start - 30 Seconds Setup

## Copy & Paste These 3 Commands

```bash
# 1. Set JWT Key (REQUIRED - only once)
dotnet user-secrets set "Jwt:Key" "TafsilkPlatform_SuperSecretKey_2024_Minimum32Chars!"

# 2. Run the app
dotnet run

# 3. Open browser
# Go to: https://localhost:7186
```

**Done!** 🎉

---

## 🔑 Login

- **Admin**: `admin@tafsilk.local` / `Admin@123!`
- **Tester**: `tester@tafsilk.local` / `Tester@123!`

---

## 📍 Important URLs

- **App**: https://localhost:7186
- **Swagger API**: https://localhost:7186/swagger  
- **Health Check**: https://localhost:7186/health

---

## ❓ Problems?

**"JWT Key not configured"**  
→ Run command #1 above

**"Cannot connect to database"**  
→ Make sure SQL Server is running

**"Port in use"**  
→ Change port in `Properties/launchSettings.json`

---

That's it! Everything else is automatic. 🚀

