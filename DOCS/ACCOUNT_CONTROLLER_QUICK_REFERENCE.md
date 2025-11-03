# 🎯 Account Controller Quick Reference Card

## 📋 One-Page Cheat Sheet

### 🔍 Quick URL Lookup

| **View File** | **GET URL** | **POST URL** | **Success → ** |
|---------------|-------------|--------------|----------------|
| `Login.cshtml` | `/Account/Login` | `/Account/Login` | Dashboard (by role) |
| `Register.cshtml` | `/Account/Register` | `/Account/Register` | `/Account/Login` (C/C)<br>`/Account/CompleteTailorProfile` (T) |
| `CompleteTailorProfile.cshtml` | `/Account/CompleteTailorProfile` | `/Account/CompleteTailorProfile` | `/Account/Login` |
| `CompleteGoogleRegistration.cshtml` | `/Account/CompleteSocialRegistration` | `/Account/CompleteSocialRegistration` | Dashboard (C/C)<br>`/Account/CompleteTailorProfile` (T) |
| `ChangePassword.cshtml` | `/Account/ChangePassword` | `/Account/ChangePassword` | Dashboard |
| `RequestRoleChange.cshtml` | `/Account/RequestRoleChange` | `/Account/RequestRoleChange` | `/Account/Login` |
| `ResendVerificationEmail.cshtml` | `/Account/ResendVerificationEmail` | `/Account/ResendVerificationEmail` | Same page |

**(C/C)** = Customer/Corporate | **(T)** = Tailor

---

### 🎭 User Type Routing Matrix

| **User Type** | **After Login** | **After Register** | **After OAuth** |
|---------------|-----------------|--------------------|--------------------|
| **Customer** | `/Dashboards/Customer` | `/Account/Login` | `/Dashboards/Customer` |
| **Tailor** (complete) | `/Dashboards/Tailor` | `/Account/CompleteTailorProfile` | `/Account/CompleteTailorProfile` |
| **Tailor** (incomplete) | `/Account/CompleteTailorProfile` | N/A | N/A |
| **Corporate** | `/Dashboards/Corporate` | `/Account/Login` | `/Dashboards/Corporate` |

---

### 🚪 All GET Endpoints

| **URL** | **Action** | **View** | **Auth?** |
|---------|------------|----------|-----------|
| `/Account/Login` | `Login()` | `Login.cshtml` | ❌ |
| `/Account/Register` | `Register()` | `Register.cshtml` | ❌ |
| `/Account/CompleteTailorProfile` | `CompleteTailorProfile(incomplete)` | `CompleteTailorProfile.cshtml` | ⚠️ |
| `/Account/CompleteSocialRegistration` | `CompleteSocialRegistration()` | `CompleteGoogleRegistration.cshtml` | ❌ |
| `/Account/ChangePassword` | `ChangePassword()` | `ChangePassword.cshtml` | ✅ |
| `/Account/RequestRoleChange` | `RequestRoleChange()` | `RequestRoleChange.cshtml` | ✅ |
| `/Account/ResendVerificationEmail` | `ResendVerificationEmail()` | `ResendVerificationEmail.cshtml` | ❌ |
| `/Account/VerifyEmail?token=...` | `VerifyEmail(token)` | *Redirect* | ❌ |
| `/Account/GoogleLogin` | `GoogleLogin(returnUrl)` | *External* | ❌ |
| `/Account/FacebookLogin` | `FacebookLogin(returnUrl)` | *External* | ❌ |
| `/Account/GoogleResponse` | `GoogleResponse(returnUrl)` | *Redirect* | ❌ |
| `/Account/FacebookResponse` | `FacebookResponse(returnUrl)` | *Redirect* | ❌ |
| `/Account/ProfilePicture/{id}` | `ProfilePicture(id)` | *File* | ❌ |

---

### 📨 All POST Endpoints

| **URL** | **Action** | **Parameters** | **Redirect On Success** |
|---------|------------|----------------|-------------------------|
| `/Account/Login` | `Login(...)` | `email`, `password`, `rememberMe`, `returnUrl` | Dashboard or returnUrl |
| `/Account/Register` | `Register(...)` | `name`, `email`, `password`, `userType`, `phoneNumber` | `/Account/Login` or `/Account/CompleteTailorProfile` |
| `/Account/CompleteTailorProfile` | `CompleteTailorProfile(model)` | `CompleteTailorProfileRequest` | `/Account/Login` |
| `/Account/CompleteSocialRegistration` | `CompleteSocialRegistration(model)` | `CompleteGoogleRegistrationViewModel` | Dashboard or `/Account/CompleteTailorProfile` |
| `/Account/ChangePassword` | `ChangePassword(model)` | `ChangePasswordViewModel` | Dashboard |
| `/Account/RequestRoleChange` | `RequestRoleChange(model)` | `RoleChangeRequestViewModel` | `/Account/Login` |
| `/Account/ResendVerificationEmail` | `ResendVerificationEmail(email)` | `email` | Same page |
| `/Account/Logout` | `Logout()` | *None* | `/Home/Index` |

---

### 🔐 Authentication Requirements

| **Endpoint** | **Auth Required** | **Notes** |
|--------------|-------------------|-----------|
| Login (GET/POST) | ❌ No | Redirects if already authenticated |
| Register (GET/POST) | ❌ No | Blocks if already authenticated |
| CompleteTailorProfile (GET/POST) | ⚠️ Mixed | Allows both authenticated & unauthenticated (via TempData) |
| CompleteSocialRegistration (GET/POST) | ❌ No | Uses TempData for session |
| ChangePassword (GET/POST) | ✅ Yes | Requires authenticated user |
| RequestRoleChange (GET/POST) | ✅ Yes | Requires authenticated user |
| ResendVerificationEmail (GET/POST) | ❌ No | Public access |
| VerifyEmail (GET) | ❌ No | Public access via token |
| OAuth (GoogleLogin, etc.) | ❌ No | Public access |
| Logout (POST) | ✅ Yes | Must be logged in to log out |
| ProfilePicture (GET) | ❌ No | Public file access |

---

### 🎯 Form Models

| **Form** | **Model/Parameters** | **Required Fields** |
|----------|---------------------|---------------------|
| **Login** | `email`, `password`, `rememberMe`, `returnUrl` | `email`, `password` |
| **Register** | `name`, `email`, `password`, `userType`, `phoneNumber` | `name`, `email`, `password` |
| **CompleteTailorProfile** | `CompleteTailorProfileRequest` | `WorkshopName`, `PhoneNumber`, `Address`, `Description`, `IdDocument`, `PortfolioImages` (3+) |
| **CompleteSocialRegistration** | `CompleteGoogleRegistrationViewModel` | `FullName`, `UserType` |
| **ChangePassword** | `ChangePasswordViewModel` | `CurrentPassword`, `NewPassword`, `ConfirmPassword` |
| **RequestRoleChange** | `RoleChangeRequestViewModel` | `ShopName`, `Address`, `Reason` (20-500 chars) |
| **ResendVerificationEmail** | `email` | `email` |

---

### 🔄 Common Redirect Patterns

```csharp
// By Role:
Customer → /Dashboards/Customer
Tailor → /Dashboards/Tailor
Corporate → /Dashboards/Corporate

// After Registration:
Customer/Corporate → /Account/Login
Tailor → /Account/CompleteTailorProfile

// After Evidence Submission:
All Tailors → /Account/Login

// After Role Change:
All → /Account/Login (must re-authenticate)

// After Logout:
All → /Home/Index
```

---

### ⚠️ Special Cases

| **Scenario** | **What Happens** |
|--------------|------------------|
| **Tailor login without profile** | Signs in temporarily → Redirects to `/Account/CompleteTailorProfile?incomplete=true` |
| **Rate limited (5 failed logins)** | Shows error, blocks for 15 minutes |
| **OAuth new user** | Stores TempData → Redirects to `/Account/CompleteSocialRegistration` |
| **OAuth existing user** | Signs in → Redirects to dashboard |
| **Duplicate tailor evidence** | Shows error → Redirects to `/Account/Login` |
| **Expired email verification token** | Shows error → Redirects to `/Account/Login` |
| **User tries to register while logged in** | Blocks with error → Redirects to dashboard |

---

### 🔗 OAuth Flow Quick Map

```
Google:
  /Account/GoogleLogin → External Auth → /Account/GoogleResponse → Dashboard or CompleteSocialRegistration

Facebook:
  /Account/FacebookLogin → External Auth → /Account/FacebookResponse → Dashboard or CompleteSocialRegistration
```

---

### 📊 TempData Keys Used

| **Key** | **Used By** | **Purpose** |
|---------|-------------|-------------|
| `RegisterSuccess` | Multiple | Success message on login page |
| `ErrorMessage` | Multiple | Error message display |
| `InfoMessage` | Multiple | Info message display |
| `WarningMessage` | Multiple | Warning message display |
| `SuccessMessage` | ChangePassword | Success feedback |
| `TailorUserId` | CompleteTailorProfile | Store user ID for evidence submission |
| `TailorEmail` | CompleteTailorProfile | Store email for evidence submission |
| `TailorName` | CompleteTailorProfile | Store name for evidence submission |
| `OAuthProvider` | CompleteSocialRegistration | Store provider (Google/Facebook) |
| `OAuthEmail` | CompleteSocialRegistration | Store OAuth email |
| `OAuthName` | CompleteSocialRegistration | Store OAuth name |
| `OAuthPicture` | CompleteSocialRegistration | Store OAuth profile picture |
| `OAuthId` | CompleteSocialRegistration | Store OAuth provider ID |

---

### 🛡️ Security Features

| **Feature** | **Implementation** | **Location** |
|-------------|-------------------|--------------|
| **Rate Limiting** | 5 attempts → 15min lockout | Login action |
| **Input Sanitization** | HTML/SQL injection prevention | Login/Register actions |
| **Anti-Forgery Tokens** | All POST forms | `@Html.AntiForgeryToken()` |
| **Password Requirements** | 6+ characters minimum | Register/ChangePassword |
| **File Upload Validation** | Type/size checks (5MB max) | CompleteTailorProfile |
| **Email Verification** | Token-based (24h expiry) | VerifyEmail |
| **Duplicate Prevention** | Profile existence check | CompleteTailorProfile |

---

### ❌ Known Issues

| **Issue** | **Location** | **Impact** |
|-----------|--------------|------------|
| Settings action missing | `ChangePassword.cshtml` Cancel button | ⚠️ Broken link |
| Settings action missing | `RequestRoleChange.cshtml` Cancel button | ⚠️ Broken link |
| Forgot Password not implemented | `Login.cshtml` link | ⚠️ Goes to `#` |
| Duplicate action names | `ProvideTailorEvidence` = `CompleteTailorProfile` | ⚠️ Confusing |
| TempData dependency | Multiple views | ⚠️ Lost on refresh |

---

### 🎨 View File Paths

```
TafsilkPlatform.Web/Views/Account/
├── Login.cshtml
├── Register.cshtml
├── CompleteTailorProfile.cshtml
├── CompleteGoogleRegistration.cshtml
├── ChangePassword.cshtml
├── RequestRoleChange.cshtml
└── ResendVerificationEmail.cshtml
```

---

### 🔧 Quick Fixes Needed

```csharp
// 1. Add Settings action:
[HttpGet]
public IActionResult Settings()
{
    return RedirectToUserDashboard();
}

// 2. Implement Forgot Password:
[HttpGet]
[AllowAnonymous]
public IActionResult ForgotPassword() => View();

[HttpPost]
[AllowAnonymous]
public async Task<IActionResult> ForgotPassword(string email)
{
    // Implementation
}

// 3. Remove duplicate actions:
[Obsolete("Use CompleteTailorProfile")]
public IActionResult ProvideTailorEvidence() => CompleteTailorProfile();
```

---

### 📞 Related Controllers

| **Redirect Target** | **Controller** | **Action** |
|---------------------|----------------|------------|
| `/Dashboards/Customer` | `DashboardsController` | `Customer()` |
| `/Dashboards/Tailor` | `DashboardsController` | `Tailor()` |
| `/Dashboards/Corporate` | `DashboardsController` | `Corporate()` |
| `/Home/Index` | `HomeController` | `Index()` |

---

### 💾 Database Impact

| **Action** | **Tables Modified** |
|------------|---------------------|
| **Register** | `Users`, `CustomerProfiles` OR `CorporateAccounts` |
| **CompleteTailorProfile** | `TailorProfiles`, `PortfolioImages`, `Users` (IsActive) |
| **ChangePassword** | `Users` (PasswordHash) |
| **RequestRoleChange** | `Users` (RoleId), `TailorProfiles` |
| **VerifyEmail** | `Users` (EmailVerified, EmailVerificationToken) |
| **Login** | `Users` (LastLoginAt) |

---

### 🎯 Testing Checklist

- [ ] Login with valid credentials → Dashboard
- [ ] Login with invalid credentials → Error message
- [ ] Login 5 times with wrong password → Rate limit message
- [ ] Register as Customer → Success → Login page
- [ ] Register as Tailor → Redirect to evidence page
- [ ] Complete tailor evidence → Success → Can login
- [ ] OAuth (Google) new user → Complete registration
- [ ] OAuth (Google) existing user → Dashboard
- [ ] Change password → Success → Dashboard
- [ ] Request role change (Customer → Tailor) → Re-login required
- [ ] Verify email with valid token → Success
- [ ] Verify email with expired token → Error
- [ ] Resend verification email → Success message

---

**Quick Reference Version**: 1.0  
**Last Updated**: 2024  
**Print-Friendly**: ✅ Yes  
**Total Endpoints**: 20+  
**Total Views**: 7
