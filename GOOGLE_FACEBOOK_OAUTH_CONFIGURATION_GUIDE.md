# ✅ GOOGLE & FACEBOOK OAUTH CONFIGURATION GUIDE

## **🎊 PROBLEM SOLVED!**

```
✅ Google OAuth Authentication ADDED
✅ Facebook OAuth Authentication ADDED
✅ Build Successful
✅ Authentication Handlers Registered
```

---

## **📋 WHAT WAS FIXED**

### **Problem:**
```
InvalidOperationException: No authentication handler is registered 
for the scheme 'Google'. The registered schemes are: Cookies, Jwt.
```

### **Root Cause:**
Your `Program.cs` was missing Google and Facebook OAuth authentication configuration, even though `appsettings.json` had `EnableGoogleOAuth: true`.

### **Solution Applied:**
1. ✅ Added `Microsoft.AspNetCore.Authentication.Google` using directive
2. ✅ Added `Microsoft.AspNetCore.Authentication.Facebook` using directive
3. ✅ Configured Google OAuth authentication
4. ✅ Configured Facebook OAuth authentication
5. ✅ Updated `appsettings.json` with OAuth credentials section
6. ✅ Added JWT Key (was missing!)

---

## **🔐 OAUTH SETUP GUIDE**

### **Step 1: Get Google OAuth Credentials**

#### **1.1 Create Google Cloud Project**
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing
3. Name: "Tafsilk Platform"

#### **1.2 Enable Google+ API**
1. Go to **APIs & Services** → **Library**
2. Search for "Google+ API"
3. Click **Enable**

#### **1.3 Create OAuth 2.0 Credentials**
1. Go to **APIs & Services** → **Credentials**
2. Click **Create Credentials** → **OAuth client ID**
3. Configure consent screen (if prompted):
   - User Type: **External**
- App name: **Tafsilk Platform**
   - Support email: your email
   - Developer contact: your email
4. Application type: **Web application**
5. Name: **Tafsilk Platform Web**
6. Authorized redirect URIs:
   ```
   https://localhost:5001/signin-google
   https://localhost:7001/signin-google
   http://localhost:5000/signin-google
   ```
   
   **For Production:**
   ```
   https://yourdomain.com/signin-google
   https://www.yourdomain.com/signin-google
 ```

7. Click **Create**
8. Copy the **Client ID** and **Client Secret**

#### **1.4 Update appsettings.json**
```json
{
  "Authentication": {
    "Google": {
   "ClientId": "YOUR_GOOGLE_CLIENT_ID_HERE.apps.googleusercontent.com",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET_HERE"
    }
  }
}
```

---

### **Step 2: Get Facebook OAuth Credentials**

#### **2.1 Create Facebook App**
1. Go to [Facebook Developers](https://developers.facebook.com/)
2. Click **My Apps** → **Create App**
3. Select **Consumer** or **Business** type
4. Display name: **Tafsilk Platform**
5. Contact email: your email
6. Click **Create App**

#### **2.2 Add Facebook Login Product**
1. In left sidebar, click **Add Product**
2. Find **Facebook Login** → Click **Set Up**
3. Select **Web** platform
4. Site URL: `https://localhost:5001`
5. Click **Save** → **Continue**

#### **2.3 Configure OAuth Settings**
1. Go to **Facebook Login** → **Settings**
2. Valid OAuth Redirect URIs:
   ```
   https://localhost:5001/signin-facebook
   https://localhost:7001/signin-facebook
   http://localhost:5000/signin-facebook
   ```
   
   **For Production:**
   ```
   https://yourdomain.com/signin-facebook
   https://www.yourdomain.com/signin-facebook
   ```

3. Click **Save Changes**

#### **2.4 Get App Credentials**
1. Go to **Settings** → **Basic**
2. Copy **App ID**
3. Click **Show** next to **App Secret**
4. Copy **App Secret**

#### **2.5 Update appsettings.json**
```json
{
  "Authentication": {
    "Facebook": {
      "AppId": "YOUR_FACEBOOK_APP_ID_HERE",
      "AppSecret": "YOUR_FACEBOOK_APP_SECRET_HERE"
    }
  }
}
```

---

## **⚙️ CONFIGURATION FILES**

### **Complete appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TafsilkPlatformDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "TafsilkPlatform_SuperSecretKey_MinimumLength32Characters_ChangeInProduction!",
    "Issuer": "TafsilkPlatform",
    "Audience": "TafsilkPlatformUsers",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID_HERE.apps.googleusercontent.com",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET_HERE"
    },
    "Facebook": {
      "AppId": "YOUR_FACEBOOK_APP_ID_HERE",
      "AppSecret": "YOUR_FACEBOOK_APP_SECRET_HERE"
    }
  },
  "Features": {
    "EnableGoogleOAuth": true,
    "EnableFacebookOAuth": true,
    "EnableEmailVerification": true
  }
}
```

### **For Production (appsettings.Production.json)**
```json
{
  "Jwt": {
    "Key": "YOUR_PRODUCTION_SECRET_KEY_MINIMUM_32_CHARACTERS_VERY_SECURE!"
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_PRODUCTION_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
   "ClientSecret": "YOUR_PRODUCTION_GOOGLE_CLIENT_SECRET"
    },
    "Facebook": {
      "AppId": "YOUR_PRODUCTION_FACEBOOK_APP_ID",
      "AppSecret": "YOUR_PRODUCTION_FACEBOOK_APP_SECRET"
    }
  },
  "Application": {
    "BaseUrl": "https://yourdomain.com"
  }
}
```

---

## **🧪 TESTING OAUTH**

### **Test Google OAuth**
1. Run your application
2. Go to `/Account/Register` or `/Account/Login`
3. Click **"Login with Google"** button
4. You should be redirected to Google's login page
5. Sign in with your Google account
6. Authorize the app
7. You should be redirected back to your app

### **Test Facebook OAuth**
1. Run your application
2. Go to `/Account/Register` or `/Account/Login`
3. Click **"Login with Facebook"** button
4. You should be redirected to Facebook's login page
5. Sign in with your Facebook account
6. Authorize the app
7. You should be redirected back to your app

### **Expected Flow**

#### **For Existing Users:**
```
1. User clicks "Login with Google/Facebook"
2. Redirects to OAuth provider
3. User authorizes
4. Redirects back to /Account/GoogleResponse or /Account/FacebookResponse
5. System finds user by email
6. User logged in
7. Redirects to dashboard
```

#### **For New Users:**
```
1. User clicks "Login with Google/Facebook"
2. Redirects to OAuth provider
3. User authorizes
4. Redirects back to /Account/GoogleResponse or /Account/FacebookResponse
5. System doesn't find user
6. Redirects to /Account/CompleteSocialRegistration
7. User selects role (Customer/Tailor)
8. Account created
9. User logged in
10. Redirects to dashboard
```

---

## **🔧 TROUBLESHOOTING**

### **Problem: "Redirect URI mismatch"**

**Solution:**
1. Check your OAuth provider settings
2. Ensure redirect URI in settings matches exactly:
   - Google: `https://localhost:5001/signin-google`
   - Facebook: `https://localhost:5001/signin-facebook`
3. No trailing slashes
4. HTTPS required in production

---

### **Problem: "App not configured for this user"**

**For Google:**
1. Go to Google Cloud Console
2. OAuth consent screen → **Add Test Users**
3. Add your email address

**For Facebook:**
1. Go to Facebook App Dashboard
2. App Roles → **Add Testers**
3. Add your Facebook account

---

### **Problem: Credentials not found**

**Check appsettings.json:**
```json
"Authentication": {
  "Google": {
    "ClientId": "MUST_NOT_BE_EMPTY",
    "ClientSecret": "MUST_NOT_BE_EMPTY"
  },
  "Facebook": {
    "AppId": "MUST_NOT_BE_EMPTY",
    "AppSecret": "MUST_NOT_BE_EMPTY"
  }
}
```

**Warning in logs:**
```
⚠️ Google OAuth enabled but credentials not configured
```

---

### **Problem: "Invalid client"**

**Solution:**
1. Double-check ClientId/AppId
2. Ensure no extra spaces
3. Ensure credentials are from correct environment (dev/prod)

---

## **🔐 SECURITY BEST PRACTICES**

### **1. Never Commit Credentials**
```bash
# Add to .gitignore
appsettings.json
appsettings.*.json
!appsettings.Example.json
```

### **2. Use User Secrets (Development)**
```bash
dotnet user-secrets init
dotnet user-secrets set "Authentication:Google:ClientId" "YOUR_CLIENT_ID"
dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_SECRET"
dotnet user-secrets set "Authentication:Facebook:AppId" "YOUR_APP_ID"
dotnet user-secrets set "Authentication:Facebook:AppSecret" "YOUR_SECRET"
dotnet user-secrets set "Jwt:Key" "YOUR_JWT_KEY_32_CHARACTERS_MINIMUM"
```

### **3. Use Environment Variables (Production)**
```bash
# Azure App Service / Linux
Authentication__Google__ClientId=YOUR_CLIENT_ID
Authentication__Google__ClientSecret=YOUR_SECRET
Authentication__Facebook__AppId=YOUR_APP_ID
Authentication__Facebook__AppSecret=YOUR_SECRET
Jwt__Key=YOUR_JWT_KEY
```

### **4. Rotate Secrets Regularly**
- Change OAuth secrets every 90 days
- Change JWT key every 180 days
- Use Azure Key Vault for production

---

## **📊 AUTHENTICATION SCHEMES REGISTERED**

After the fix, your application now supports:

```
✅ Cookies    - Default authentication (session-based)
✅ Jwt  - API authentication (token-based)
✅ Google     - OAuth 2.0 (Google login)
✅ Facebook   - OAuth 2.0 (Facebook login)
```

### **Usage in Controllers:**
```csharp
// Cookie authentication (default)
[Authorize]
public IActionResult Profile()

// JWT authentication (API)
[Authorize(AuthenticationSchemes = "Jwt")]
public IActionResult ApiEndpoint()

// Google OAuth challenge
public IActionResult GoogleLogin()
{
    return Challenge(new AuthenticationProperties 
    { 
        RedirectUri = "/Account/GoogleResponse" 
    }, "Google");
}

// Facebook OAuth challenge
public IActionResult FacebookLogin()
{
    return Challenge(new AuthenticationProperties 
    { 
     RedirectUri = "/Account/FacebookResponse" 
    }, "Facebook");
}
```

---

## **✅ VERIFICATION CHECKLIST**

- [x] ✅ Google OAuth NuGet package installed
- [x] ✅ Facebook OAuth NuGet package installed
- [x] ✅ Program.cs configured with Google OAuth
- [x] ✅ Program.cs configured with Facebook OAuth
- [x] ✅ appsettings.json has OAuth credentials section
- [x] ✅ JWT Key added to appsettings.json
- [x] ✅ Build successful
- [x] ✅ No compilation errors

### **Remaining Steps (You need to do):**
- [ ] Get Google OAuth credentials from Google Cloud Console
- [ ] Get Facebook OAuth credentials from Facebook Developers
- [ ] Update appsettings.json with real credentials
- [ ] Test Google login
- [ ] Test Facebook login
- [ ] Configure production credentials
- [ ] Set up User Secrets for development
- [ ] Set up environment variables for production

---

## **🎯 QUICK START**

### **Option 1: Disable OAuth (Temporary)**
If you want to test without OAuth first:

```json
{
  "Features": {
    "EnableGoogleOAuth": false,
    "EnableFacebookOAuth": false
  }
}
```

### **Option 2: Use Test Credentials**
Get credentials and test immediately:

1. **Google (5 minutes):**
   - Go to https://console.cloud.google.com/
   - Create project → Enable Google+ API → Create OAuth client
   - Copy ClientId & ClientSecret

2. **Facebook (5 minutes):**
   - Go to https://developers.facebook.com/
   - Create app → Add Facebook Login
   - Copy AppId & AppSecret

3. **Update appsettings.json**
4. **Run and test!**

---

## **📝 SUMMARY**

### **What Changed:**
1. ✅ Added `using Microsoft.AspNetCore.Authentication.Google;`
2. ✅ Added `using Microsoft.AspNetCore.Authentication.Facebook;`
3. ✅ Configured `authBuilder.AddGoogle()` with conditional check
4. ✅ Configured `authBuilder.AddFacebook()` with conditional check
5. ✅ Added `Authentication` section to appsettings.json
6. ✅ Added missing JWT Key to appsettings.json
7. ✅ Added feature flags to enable/disable OAuth

### **Authentication Flow:**
```
Traditional Login:
  Email/Password → Cookie Auth → Dashboard

Google OAuth:
  Google Button → Google Login → Callback → Cookie Auth → Dashboard

Facebook OAuth:
  Facebook Button → Facebook Login → Callback → Cookie Auth → Dashboard
```

### **Status:**
```
✅ Code Fixed
✅ Build Successful
✅ Configuration Ready
⚠️ Credentials Needed (Get from Google/Facebook)
```

---

**📅 Date Fixed:** 2025-01-20  
**Status:** ✅ **COMPLETE**  
**Next Step:** Get OAuth credentials and test!

---

**🎉 Your OAuth authentication is now configured and ready to use!**

**تفصيلك - نربط بينك وبين أفضل الخياطين** 🧵✂️
