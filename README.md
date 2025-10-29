# Tafsilk Platform - Setup Instructions

## 🔧 Initial Setup

### 1. Clone the Repository
```bash
git clone https://github.com/AhmedTyson/Tafsilk.git
cd Tafsilk
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Database Setup
```bash
cd TafsilkPlatform.Web
dotnet ef database update
```

## 🔐 OAuth Configuration (REQUIRED)

The OAuth credentials are **NOT** included in the repository for security reasons. You need to set them up using one of the following methods:

### Method 1: Using Setup Scripts (Recommended)

#### Windows:
```powershell
cd TafsilkPlatform.Web
.\setup-oauth.bat
```

#### Linux/macOS:
```bash
cd TafsilkPlatform.Web
chmod +x setup-oauth.sh
./setup-oauth.sh
```

### Method 2: Manual Configuration

1. **Initialize User Secrets:**
```bash
cd TafsilkPlatform.Web
dotnet user-secrets init
```

2. **Set Google OAuth Credentials:**
```bash
dotnet user-secrets set "Google:client_id" "YOUR_GOOGLE_CLIENT_ID"
dotnet user-secrets set "Google:client_secret" "YOUR_GOOGLE_CLIENT_SECRET"
```

3. **Set Facebook OAuth Credentials:**
```bash
dotnet user-secrets set "Facebook:app_id" "YOUR_FACEBOOK_APP_ID"
dotnet user-secrets set "Facebook:app_secret" "YOUR_FACEBOOK_APP_SECRET"
```

### Getting OAuth Credentials

#### Google OAuth:
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing
3. Enable Google+ API
4. Create OAuth 2.0 credentials (Web application)
5. Add redirect URI: `https://localhost:7186/signin-google`
6. Copy Client ID and Client Secret

#### Facebook OAuth:
1. Go to [Facebook Developers](https://developers.facebook.com/)
2. Create a new app (Consumer type)
3. Set up Facebook Login
4. Add redirect URI: `https://localhost:7186/signin-facebook`
5. Copy App ID and App Secret from Settings > Basic

📖 **For detailed instructions, see:** `TafsilkPlatform.Web/OAuth_Setup_Guide.md`

## ▶️ Running the Application

```bash
dotnet run --project TafsilkPlatform.Web
```

Then navigate to: `https://localhost:7186`

## 📚 Documentation

- **OAuth Setup Guide**: `TafsilkPlatform.Web/OAuth_Setup_Guide.md`
- **Settings User Guide**: `TafsilkPlatform.Web/SETTINGS_USER_GUIDE.md`
- **Project Analysis**: `TafsilkPlatform.Web/PROJECT_ANALYSIS.md`

## 🏗️ Project Structure

```
Tafsilk/
├── TafsilkPlatform.Web/   # Main web application (.NET 9)
│   ├── Controllers/    # MVC Controllers
│   ├── Models/  # Domain models
│   ├── Views/        # Razor views
│   ├── Data/                     # DbContext and migrations
│   ├── Services/         # Business logic services
│   ├── Repositories/     # Data access repositories
│   ├── Security/          # Security utilities
│   └── wwwroot/       # Static files (CSS, JS, images)
└── README.md   # This file
```

## ✨ Features

- ✅ User Authentication (Email/Password)
- ✅ OAuth Login (Google & Facebook)
- ✅ Role-based Access (Customer, Tailor, Corporate)
- ✅ Profile Management with Image Cropping
- ✅ Settings Page with Notifications
- ✅ Responsive Design
- ✅ RTL Support (Arabic)
- ✅ Modern UI with Gradients and Animations

## 🔒 Security Notes

- **Never commit OAuth secrets** to the repository
- Use **User Secrets** for development
- Use **Environment Variables** or **Azure Key Vault** for production
- OAuth credentials are protected by GitHub Secret Scanning
- All sensitive data is excluded from version control via `.gitignore`

## 🐛 Troubleshooting

### OAuth Not Working?
1. Verify User Secrets are set: `dotnet user-secrets list`
2. Check callback URLs match in OAuth provider settings
3. Ensure you're using HTTPS (required for OAuth)
4. Review logs in the console for detailed errors

### Database Issues?
```bash
# Reset database
dotnet ef database drop
dotnet ef database update
```

### Build Errors?
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

## 📞 Support

For issues or questions:
1. Check the documentation files in `TafsilkPlatform.Web/`
2. Review the OAuth Setup Guide
3. Check GitHub Issues

## 🚀 Deployment

### Azure App Service
1. Set OAuth credentials in App Service Configuration
2. Update connection string for production database
3. Enable HTTPS (required for OAuth)
4. Configure proper redirect URIs in OAuth providers

### Environment Variables for Production
```bash
Google__client_id=your_client_id
Google__client_secret=your_client_secret
Facebook__app_id=your_app_id
Facebook__app_secret=your_app_secret
```

## 📝 License

[Add your license information here]

## 👥 Contributors

[Add contributor information here]

---

**Last Updated:** January 2025  
**Version:** 1.0  
**Target Framework:** .NET 9
