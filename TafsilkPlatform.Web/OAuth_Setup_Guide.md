# OAuth Setup Guide - Google & Facebook Authentication

This guide will help you configure Google and Facebook OAuth authentication for the Tafsilk Platform.

## 🔑 Using User Secrets (Recommended for Development)

User Secrets keep your OAuth credentials safe and prevent them from being committed to source control.

### Step 1: Initialize User Secrets

Open a terminal in the `TafsilkPlatform.Web` directory and run:

```bash
dotnet user-secrets init
```

### Step 2: Set Google OAuth Credentials

```bash
dotnet user-secrets set "Google:client_id" "YOUR_GOOGLE_CLIENT_ID"
dotnet user-secrets set "Google:client_secret" "YOUR_GOOGLE_CLIENT_SECRET"
```

### Step 3: Set Facebook OAuth Credentials

```bash
dotnet user-secrets set "Facebook:app_id" "YOUR_FACEBOOK_APP_ID"
dotnet user-secrets set "Facebook:app_secret" "YOUR_FACEBOOK_APP_SECRET"
```

---

## 🌐 Getting Google OAuth Credentials

### 1. Go to Google Cloud Console
Visit: https://console.cloud.google.com/

### 2. Create a New Project (or select existing)
- Click "Select a Project" → "New Project"
- Name: "Tafsilk Platform"
- Click "Create"

### 3. Enable Google+ API
- Navigate to "APIs & Services" → "Library"
- Search for "Google+ API"
- Click "Enable"

### 4. Create OAuth 2.0 Credentials
- Go to "APIs & Services" → "Credentials"
- Click "Create Credentials" → "OAuth client ID"
- Application type: "Web application"
- Name: "Tafsilk Web App"

### 5. Configure Authorized Redirect URIs
Add these URLs:

**For Development:**
```
https://localhost:7186/signin-google
http://localhost:5140/signin-google
```

**For Production:**
```
https://yourdomain.com/signin-google
```

### 6. Copy Credentials
- Copy the **Client ID**
- Copy the **Client Secret**
- Save them using the User Secrets commands above

---

## 📘 Getting Facebook OAuth Credentials

### 1. Go to Facebook Developers
Visit: https://developers.facebook.com/

### 2. Create a New App
- Click "My Apps" → "Create App"
- Select "Consumer" as the app type
- Fill in app details:
  - App Name: "Tafsilk Platform"
  - App Contact Email: your-email@example.com
- Click "Create App"

### 3. Set Up Facebook Login
- In your app dashboard, find "Facebook Login"
- Click "Set Up"
- Choose "Web" platform
- Enter Site URL: `https://localhost:7186` (for development)

### 4. Configure OAuth Settings
- Go to "Facebook Login" → "Settings"
- Add to "Valid OAuth Redirect URIs":

**For Development:**
```
https://localhost:7186/signin-facebook
http://localhost:5140/signin-facebook
```

**For Production:**
```
https://yourdomain.com/signin-facebook
```

- Save Changes

### 5. Get App Credentials
- Go to "Settings" → "Basic"
- Copy the **App ID**
- Click "Show" next to **App Secret** and copy it
- Save them using the User Secrets commands above

### 6. Make App Public (for production)
- Once ready for production, go to "App Review"
- Request permissions for:
  - `email`
  - `public_profile`
- Switch app to "Live" mode

---

## 🔧 Alternative: Environment Variables (for Production)

For production environments, use environment variables instead of User Secrets:

### Linux/macOS:
```bash
export Google__client_id="YOUR_GOOGLE_CLIENT_ID"
export Google__client_secret="YOUR_GOOGLE_CLIENT_SECRET"
export Facebook__app_id="YOUR_FACEBOOK_APP_ID"
export Facebook__app_secret="YOUR_FACEBOOK_APP_SECRET"
```

### Windows (PowerShell):
```powershell
$env:Google__client_id="YOUR_GOOGLE_CLIENT_ID"
$env:Google__client_secret="YOUR_GOOGLE_CLIENT_SECRET"
$env:Facebook__app_id="YOUR_FACEBOOK_APP_ID"
$env:Facebook__app_secret="YOUR_FACEBOOK_APP_SECRET"
```

### Azure App Service:
- Go to your App Service
- Navigate to "Configuration" → "Application settings"
- Add the following settings:
  - `Google:client_id`
  - `Google:client_secret`
  - `Facebook:app_id`
  - `Facebook:app_secret`

---

## ✅ Testing OAuth Authentication

### 1. Run the Application
```bash
dotnet run
```

### 2. Navigate to Login Page
Open: `https://localhost:7186/Account/Login`

### 3. Click OAuth Buttons
- Click the Google button to test Google login
- Click the Facebook button to test Facebook login

### 4. Verify Flow
- You should be redirected to the OAuth provider
- After authentication, you'll be redirected back to the app
- For new users, you'll see a registration completion form
- For existing users, you'll be signed in automatically

---

## 🐛 Troubleshooting

### "Redirect URI mismatch" Error
- Ensure the callback URL in your OAuth app matches exactly
- Check for `http` vs `https`
- Verify the port number matches your `launchSettings.json`

### "OAuth credentials not configured" Warning
- Check that User Secrets are set correctly
- Run `dotnet user-secrets list` to verify

### Google: "This app is blocked"
- Go to Google Cloud Console
- Add test users in "OAuth consent screen"
- Or publish your app (requires verification)

### Facebook: "App Not Set Up"
- Ensure Facebook Login is enabled in your app
- Verify redirect URIs are correct
- Check that app is in Development or Live mode

### Claims Not Available
- Check the scopes requested in `Program.cs`
- Verify claim mappings are correct
- Review the OAuth provider's documentation

---

## 🔒 Security Best Practices

1. **Never commit credentials to source control**
   - Use User Secrets for development
   - Use Environment Variables for production
   - Add `appsettings.Development.json` to `.gitignore`

2. **Use HTTPS in production**
   - OAuth providers require HTTPS for callbacks
   - Configure SSL certificates properly

3. **Implement CSRF protection**
   - Already implemented via correlation cookies
   - Don't disable `ValidateAntiForgeryToken`

4. **Rotate credentials regularly**
   - Change OAuth secrets every 6-12 months
   - Revoke old credentials after rotation

5. **Monitor OAuth usage**
   - Check Google/Facebook dashboards for unusual activity
   - Set up alerts for failed authentication attempts

---

## 📚 Additional Resources

- [Google OAuth Documentation](https://developers.google.com/identity/protocols/oauth2)
- [Facebook Login Documentation](https://developers.facebook.com/docs/facebook-login)
- [ASP.NET Core External Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/)
- [User Secrets in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)

---

## 📞 Support

If you encounter issues:
1. Check the troubleshooting section above
2. Review application logs for detailed error messages
3. Consult the OAuth provider's documentation
4. Check the ASP.NET Core documentation

---

**Last Updated:** January 2025
**Version:** 1.0
