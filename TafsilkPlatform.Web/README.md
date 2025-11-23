# Tafsilk Platform - Easy Setup Guide

## 🚀 Quick Start (3 Steps)

### Step 1: Set JWT Key (Required)
```bash
dotnet user-secrets set "Jwt:Key" "YourSuperSecretKeyAtLeast32CharactersLong!"
```

### Step 2: Run the Application
```bash
dotnet run
```

### Step 3: Open Browser
- **Application**: https://localhost:7186
- **Swagger API**: https://localhost:7186/swagger
- **Health Check**: https://localhost:7186/health

**That's it!** The database will be created automatically.

---

## 📋 Default Login Credentials

After first run, you can login with:

- **Admin**: `admin@tafsilk.local` / `Admin@123!`
- **Tester**: `tester@tafsilk.local` / `Tester@123!`

⚠️ **Change these passwords immediately in production!**

---

## 🔧 Configuration (Optional)

### Change Admin Credentials
```bash
dotnet user-secrets set "Admin:Email" "your-admin@email.com"
dotnet user-secrets set "Admin:Password" "YourSecurePassword123!"
```

### Change Database Connection
Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YourConnectionStringHere"
  }
}
```

### Enable Email (Optional)
```bash
dotnet user-secrets set "Email:SmtpUsername" "your-email@gmail.com"
dotnet user-secrets set "Email:SmtpPassword" "your-app-password"
```

---

## 📁 Project Structure

```
TafsilkPlatform.Web/
├── Controllers/      # API and MVC controllers
├── Services/         # Business logic
├── Repositories/    # Data access
├── Models/          # Database entities
├── ViewModels/      # Data transfer objects
├── Views/           # Razor pages
├── Middleware/      # Request processing
├── Security/        # Authentication & authorization
└── Data/            # Database context & migrations
```

---

## 🛠️ Common Tasks

### Add a New Feature
1. Create model in `Models/`
2. Add DbSet to `AppDbContext`
3. Create repository in `Repositories/`
4. Create service in `Services/`
5. Create controller in `Controllers/`
6. Run migration: `dotnet ef migrations add FeatureName`

### Run Database Migrations
```bash
dotnet ef database update
```

### View Logs
Logs are saved in `Logs/` directory (created automatically)

---

## 🐛 Troubleshooting

### "JWT Key is not configured"
Run: `dotnet user-secrets set "Jwt:Key" "YourKeyHere"`

### "Cannot connect to database"
1. Check SQL Server is running
2. Verify connection string in `appsettings.Development.json`
3. Try: `dotnet ef database update`

### "Port already in use"
Change port in `Properties/launchSettings.json`

---

## 📚 Documentation

- **API Documentation**: `/swagger` (when running)
- **Health Checks**: `/health`
- **Fixes Summary**: `WEBSITE_FIXES_SUMMARY.md`

---

## ✅ What Works Out of the Box

- ✅ User registration & login
- ✅ Role-based authentication (Admin, Customer, Tailor)
- ✅ E-commerce (products, cart, checkout)
- ✅ Order management
- ✅ Profile management
- ✅ Database auto-migration
- ✅ Error handling
- ✅ Security headers
- ✅ Health checks

---

## 🔒 Security Notes

- JWT key must be at least 32 characters
- Change default admin credentials
- Use HTTPS in production
- Never commit secrets to git

---

**Need Help?** Check the logs in `Logs/` directory or review `WEBSITE_FIXES_SUMMARY.md`

