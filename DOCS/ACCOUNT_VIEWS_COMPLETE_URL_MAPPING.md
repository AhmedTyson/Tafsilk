# 🗺️ Complete Account Views URL Mapping & Redirect Analysis

## 📋 Executive Summary

This document maps **ALL** Account views to their corresponding AccountController actions, showing:
- Which forms submit where
- What GET requests load which actions
- OAuth flows and redirects
- All potential navigation paths

---

## 🔍 View-by-View Analysis

### 1️⃣ **Login.cshtml**

**View Path**: `Views/Account/Login.cshtml`

#### 📥 **Incoming GET Requests**
```csharp
URL: /Account/Login
Action: AccountController.Login() [GET]
```

#### 📤 **Form Submissions (POST)**
```html
<form action="/Account/Login" method="POST">
```
```csharp
Submits to: AccountController.Login(email, password, rememberMe, returnUrl) [POST]
Parameters:
  - email: string
  - password: string
  - rememberMe: bool (checkbox)
  - returnUrl: string? (optional)
```

#### 🔗 **Links & Redirects**
| Link Text | URL | Controller Action |
|-----------|-----|-------------------|
| "GoogleLogin" | `/Account/GoogleLogin` | `AccountController.GoogleLogin(returnUrl)` [GET] |
| "FacebookLogin" | `/Account/FacebookLogin` | `AccountController.FacebookLogin(returnUrl)` [GET] |
| "Register" | `/Account/Register` | `AccountController.Register()` [GET] |
| "Forgot Password" | `#` | ⚠️ Not implemented (just `#`) |

#### ✅ **Successful Login Redirects**
From `AccountController.Login` [POST]:
```csharp
// If returnUrl provided:
return Redirect(returnUrl);

// Otherwise, by role:
- Tailor → /Dashboards/Tailor
- Corporate → /Dashboards/Corporate
- Customer → /Dashboards/Customer (default)
```

#### ❌ **Failed Login Scenarios**
```csharp
// Tailor without profile:
return RedirectToAction("CompleteTailorProfile", new { incomplete = true });

// Rate limited:
return View(); // Stay on login page with error

// Invalid credentials:
return View(); // Stay on login page with error

// Inactive/Deleted account:
return View(); // Stay on login page with error
```

---

### 2️⃣ **Register.cshtml**

**View Path**: `Views/Account/Register.cshtml`

#### 📥 **Incoming GET Requests**
```csharp
URL: /Account/Register
Action: AccountController.Register() [GET]
```

#### 📤 **Form Submissions (POST)**
```html
<form action="/Account/Register" method="POST">
```
```csharp
Submits to: AccountController.Register(name, email, password, userType, phoneNumber) [POST]
Parameters:
  - name: string
  - email: string
  - password: string
  - userType: string ("customer", "tailor", "corporate")
  - phoneNumber: string? (optional)
```

#### ✅ **Successful Registration Redirects**
```csharp
// For Customer/Corporate:
TempData["RegisterSuccess"] = "تم إنشاء الحساب بنجاح...";
return RedirectToAction("Login");

// For Tailor:
return RedirectToTailorEvidenceSubmission(userId, email, name);
// Which sets TempData and redirects to:
return RedirectToAction("CompleteTailorProfile");
```

#### 🔗 **Links**
| Link Text | URL | Controller Action |
|-----------|-----|-------------------|
| "Login" | `/Account/Login` | `AccountController.Login()` [GET] |

---

### 3️⃣ **CompleteTailorProfile.cshtml**

**View Path**: `Views/Account/CompleteTailorProfile.cshtml`

#### 📥 **Incoming GET Requests**
```csharp
URL: /Account/CompleteTailorProfile
Action: AccountController.CompleteTailorProfile(incomplete) [GET]
Alias: AccountController.ProvideTailorEvidence(incomplete) [GET]

Sources:
  1. After tailor registration (via TempData)
  2. After tailor login without profile (authenticated)
  3. Middleware redirect for incomplete tailors
```

#### 📤 **Form Submissions (POST)**
```html
<form action="/Account/CompleteTailorProfile" method="POST" enctype="multipart/form-data">
```
```csharp
Submits to: AccountController.CompleteTailorProfile(model) [POST]
Alias: AccountController.ProvideTailorEvidence(model) [POST]

Model: CompleteTailorProfileRequest {
  UserId: Guid
  Email: string
  FullName: string
  WorkshopName: string
  WorkshopType: string
  PhoneNumber: string
  Address: string
  City: string
  Description: string
  ExperienceYears: int?
  IdDocument: IFormFile (REQUIRED)
  PortfolioImages: List<IFormFile> (REQUIRED, 3+ images)
  WorkSamples: List<IFormFile> (alias for PortfolioImages)
  AdditionalDocuments: List<IFormFile>? (optional)
  AgreeToTerms: bool (REQUIRED)
}
```

#### ✅ **Successful Submission Redirects**
```csharp
TempData["RegisterSuccess"] = "تم إكمال تسجيل الخياط بنجاح...";
return RedirectToAction("Login");
```

#### ❌ **Failed Submission Scenarios**
```csharp
// Duplicate submission:
TempData["InfoMessage"] = "تم تقديم الأوراق الثبوتية بالفعل";
return RedirectToAction("Login");

// Validation errors:
return View("CompleteTailorProfile", model); // Stay on page with errors
```

---

### 4️⃣ **CompleteGoogleRegistration.cshtml**
(Also handles Facebook registration)

**View Path**: `Views/Account/CompleteGoogleRegistration.cshtml`

#### 📥 **Incoming GET Requests**
```csharp
URL: /Account/CompleteSocialRegistration
Action: AccountController.CompleteSocialRegistration() [GET]
Aliases: 
  - AccountController.CompleteGoogleRegistration() [GET]

Sources:
  1. After OAuth callback (Google/Facebook)
  2. New user from OAuth (via TempData)
```

#### 📤 **Form Submissions (POST)**
```html
<form action="/Account/CompleteSocialRegistration" method="POST">
```
```csharp
Submits to: AccountController.CompleteSocialRegistration(model) [POST]
Alias: AccountController.CompleteGoogleRegistration(model) [POST]

Model: CompleteGoogleRegistrationViewModel {
  Email: string
  FullName: string
PhoneNumber: string?
  UserType: string ("customer", "tailor", "corporate")
  ProfilePictureUrl: string?
}
```

#### ✅ **Successful Submission Redirects**
```csharp
// For Customer/Corporate:
// Auto sign-in, then redirect by role:
- Tailor → /Dashboards/Tailor
- Corporate → /Dashboards/Corporate
- Customer → /Dashboards/Customer

// For Tailor (OAuth):
return RedirectToTailorEvidenceSubmission(userId, email, fullName);
// Which redirects to:
return RedirectToAction("CompleteTailorProfile");
```

---

### 5️⃣ **ChangePassword.cshtml**

**View Path**: `Views/Account/ChangePassword.cshtml`

#### 📥 **Incoming GET Requests**
```csharp
URL: /Account/ChangePassword
Action: AccountController.ChangePassword() [GET]
Requires: [Authorize] - Must be logged in
```

#### 📤 **Form Submissions (POST)**
```html
<form action="/Account/ChangePassword" method="POST">
```
```csharp
Submits to: AccountController.ChangePassword(model) [POST]

Model: ChangePasswordViewModel {
  CurrentPassword: string
  NewPassword: string
  ConfirmPassword: string
}
```

#### ✅ **Successful Change Redirects**
```csharp
TempData["SuccessMessage"] = "تم تغيير كلمة المرور بنجاح!";
return RedirectToUserDashboard(); // Redirects by role
```

#### 🔗 **Links**
| Link Text | URL | Notes |
|-----------|-----|-------|
| "Cancel" | `/Account/Settings` | ⚠️ Settings action not found in AccountController |

---

### 6️⃣ **RequestRoleChange.cshtml**

**View Path**: `Views/Account/RequestRoleChange.cshtml`

#### 📥 **Incoming GET Requests**
```csharp
URL: /Account/RequestRoleChange
Action: AccountController.RequestRoleChange() [GET]
Requires: [Authorize] - Must be logged in
```

#### 📤 **Form Submissions (POST)**
```html
<form action="/Account/RequestRoleChange" method="POST" enctype="multipart/form-data">
```
```csharp
Submits to: AccountController.RequestRoleChange(model) [POST]

Model: RoleChangeRequestViewModel {
  TargetRole: string (fixed as "Tailor")
  ShopName: string (REQUIRED)
  Address: string (REQUIRED)
  ExperienceYears: int?
  Reason: string (REQUIRED, 20-500 chars)
  BusinessLicenseImage: IFormFile? (optional)
}
```

#### ✅ **Successful Change Redirects**
```csharp
// Customer → Tailor conversion:
await HttpContext.SignOutAsync(...);
TempData["RegisterSuccess"] = "تم تحويل حسابك إلى خياط بنجاح...";
return RedirectToAction("Login"); // Must re-login
```

#### ❌ **Failed Change Scenarios**
```csharp
// Invalid conversion (Tailor → Customer):
ModelState.AddModelError(...);
return View(model); // Stay on page

// Missing required fields:
ModelState.AddModelError(...);
return View(model); // Stay on page
```

#### 🔗 **Links**
| Link Text | URL | Notes |
|-----------|-----|-------|
| "Cancel" | `/Account/Settings` | ⚠️ Settings action not found |

---

### 7️⃣ **ResendVerificationEmail.cshtml**

**View Path**: `Views/Account/ResendVerificationEmail.cshtml`

#### 📥 **Incoming GET Requests**
```csharp
URL: /Account/ResendVerificationEmail
Action: AccountController.ResendVerificationEmail() [GET]
```

#### 📤 **Form Submissions (POST)**
```html
<form action="/Account/ResendVerificationEmail" method="POST">
```
```csharp
Submits to: AccountController.ResendVerificationEmail(email) [POST]
Parameters:
  - email: string
```

#### ✅ **After Submission**
```csharp
// Success or failure:
return View(); // Stay on same page with TempData message
// No redirect - shows success/error message on same page
```

#### 🔗 **Links**
| Link Text | URL | Controller Action |
|-----------|-----|-------------------|
| "Back to Login" | `/Account/Login` | `AccountController.Login()` [GET] |

---

## 🌐 OAuth Flow Mapping

### Google OAuth Flow
```
1. User clicks "Login with Google" button
   ↓
   GET /Account/GoogleLogin
   ↓
2. AccountController.GoogleLogin(returnUrl) [GET]
   Creates challenge for "Google" scheme
   ↓
3. User authenticates with Google
   ↓
4. Callback: GET /Account/GoogleResponse
   ↓
5. AccountController.GoogleResponse(returnUrl) [GET]
   ↓
6a. Existing user → Sign in → Redirect to dashboard
   ↓
6b. New user → TempData stored → Redirect to /Account/CompleteSocialRegistration
   ↓
7. Complete registration form
   ↓
8. POST /Account/CompleteSocialRegistration
   ↓
9a. Customer/Corporate → Auto sign-in → Redirect to dashboard
   ↓
9b. Tailor → Redirect to /Account/CompleteTailorProfile
```

### Facebook OAuth Flow
```
1. User clicks "Login with Facebook" button
   ↓
 GET /Account/FacebookLogin
   ↓
2. AccountController.FacebookLogin(returnUrl) [GET]
   Creates challenge for "Facebook" scheme
   ↓
3. User authenticates with Facebook
   ↓
4. Callback: GET /Account/FacebookResponse
   ↓
5. AccountController.FacebookResponse(returnUrl) [GET]
   ↓
6a. Existing user → Sign in → Redirect to dashboard
   ↓
6b. New user → TempData stored → Redirect to /Account/CompleteSocialRegistration
   ↓
[Same as Google flow from step 7]
```

---

## 🚪 Email Verification Flow

```
1. User registers (Customer/Corporate/Tailor after evidence)
   ↓
2. Verification token generated & email sent
   ↓
3. User clicks link in email:
   GET /Account/VerifyEmail?token={token}
   ↓
4. AccountController.VerifyEmail(token) [GET]
   ↓
5a. Success → TempData["RegisterSuccess"] → Redirect to /Account/Login
   ↓
5b. Failure → TempData["ErrorMessage"] → Redirect to /Account/Login
```

---

## 🔐 Logout Flow

```
User clicks logout button (typically in _Layout.cshtml)
↓
POST /Account/Logout
↓
AccountController.Logout() [POST]
Signs out user
↓
Redirect to /Home/Index
```

---

## 📊 Complete URL → Action Mapping Table

| HTTP | URL Pattern | Controller Action | View Returned | Auth Required |
|------|-------------|-------------------|---------------|---------------|
| GET | `/Account/Login` | `Login()` | `Login.cshtml` | ❌ No |
| POST | `/Account/Login` | `Login(email, password, rememberMe, returnUrl)` | Redirect | ❌ No |
| GET | `/Account/Register` | `Register()` | `Register.cshtml` | ❌ No |
| POST | `/Account/Register` | `Register(name, email, password, userType, phoneNumber)` | Redirect | ❌ No |
| GET | `/Account/CompleteTailorProfile` | `CompleteTailorProfile(incomplete)` | `CompleteTailorProfile.cshtml` | ⚠️ Mixed* |
| POST | `/Account/CompleteTailorProfile` | `CompleteTailorProfile(model)` | Redirect | ⚠️ Mixed* |
| GET | `/Account/ProvideTailorEvidence` | `ProvideTailorEvidence(incomplete)` | `CompleteTailorProfile.cshtml` | ⚠️ Mixed* |
| POST | `/Account/ProvideTailorEvidence` | `ProvideTailorEvidence(model)` | Redirect | ⚠️ Mixed* |
| GET | `/Account/CompleteSocialRegistration` | `CompleteSocialRegistration()` | `CompleteGoogleRegistration.cshtml` | ❌ No |
| POST | `/Account/CompleteSocialRegistration` | `CompleteSocialRegistration(model)` | Redirect | ❌ No |
| GET | `/Account/CompleteGoogleRegistration` | `CompleteGoogleRegistration()` | `CompleteGoogleRegistration.cshtml` | ❌ No |
| POST | `/Account/CompleteGoogleRegistration` | `CompleteGoogleRegistration(model)` | Redirect | ❌ No |
| GET | `/Account/ChangePassword` | `ChangePassword()` | `ChangePassword.cshtml` | ✅ Yes |
| POST | `/Account/ChangePassword` | `ChangePassword(model)` | Redirect | ✅ Yes |
| GET | `/Account/RequestRoleChange` | `RequestRoleChange()` | `RequestRoleChange.cshtml` | ✅ Yes |
| POST | `/Account/RequestRoleChange` | `RequestRoleChange(model)` | Redirect | ✅ Yes |
| GET | `/Account/ResendVerificationEmail` | `ResendVerificationEmail()` | `ResendVerificationEmail.cshtml` | ❌ No |
| POST | `/Account/ResendVerificationEmail` | `ResendVerificationEmail(email)` | Same View | ❌ No |
| GET | `/Account/VerifyEmail?token=...` | `VerifyEmail(token)` | Redirect to Login | ❌ No |
| GET | `/Account/GoogleLogin` | `GoogleLogin(returnUrl)` | External OAuth | ❌ No |
| GET | `/Account/GoogleResponse` | `GoogleResponse(returnUrl)` | Redirect | ❌ No |
| GET | `/Account/FacebookLogin` | `FacebookLogin(returnUrl)` | External OAuth | ❌ No |
| GET | `/Account/FacebookResponse` | `FacebookResponse(returnUrl)` | Redirect | ❌ No |
| POST | `/Account/Logout` | `Logout()` | Redirect to Home | ✅ Yes |
| GET | `/Account/ProfilePicture/{id}` | `ProfilePicture(id)` | File/Image | ❌ No |

*Mixed: Allows both authenticated and unauthenticated access

---

## 🔄 Common Redirect Patterns

### RedirectToUserDashboard()
```csharp
private IActionResult RedirectToUserDashboard()
{
    var roleName = User.FindFirstValue(ClaimTypes.Role);
    return RedirectToRoleDashboard(roleName);
}
```

### RedirectToRoleDashboard(roleName)
```csharp
private IActionResult RedirectToRoleDashboard(string? roleName)
{
    return (roleName?.ToLowerInvariant()) switch
    {
        "tailor" => RedirectToAction("Tailor", "Dashboards"),
 "corporate" => RedirectToAction("Corporate", "Dashboards"),
 _ => RedirectToAction("Customer", "Dashboards")
    };
}
```

### RedirectToTailorEvidenceSubmission(userId, email, name)
```csharp
private IActionResult RedirectToTailorEvidenceSubmission(Guid userId, string email, string name)
{
    TempData["TailorUserId"] = userId.ToString();
    TempData["TailorEmail"] = email;
    TempData["TailorName"] = name;
    TempData["InfoMessage"] = "تم إنشاء حساب الخياط بنجاح...";
    return RedirectToAction("CompleteTailorProfile");
}
```

---

## ⚠️ Issues Found

### 1. Broken Links
| View | Link | Issue |
|------|------|-------|
| `ChangePassword.cshtml` | Cancel button → `/Account/Settings` | ❌ Settings action doesn't exist in AccountController |
| `RequestRoleChange.cshtml` | Cancel button → `/Account/Settings` | ❌ Settings action doesn't exist in AccountController |
| `Login.cshtml` | "Forgot Password" → `#` | ❌ Not implemented |

### 2. Duplicate Action Names
```csharp
// These are aliases for the same functionality:
- CompleteTailorProfile() == ProvideTailorEvidence()
- CompleteSocialRegistration() == CompleteGoogleRegistration()
```
**Recommendation**: Keep one, mark other as `[Obsolete]` or remove.

### 3. TempData Dependencies
Several views rely on TempData being set correctly:
- `CompleteTailorProfile.cshtml` expects: `TailorUserId`, `TailorEmail`, `TailorName`
- `CompleteGoogleRegistration.cshtml` expects: `OAuthProvider`, `OAuthEmail`, `OAuthName`, `OAuthPicture`

**Risk**: If user refreshes page, TempData is lost → broken flow.

---

## 🎯 Form Validation Summary

| Form | Client-Side | Server-Side | Anti-Forgery |
|------|-------------|-------------|--------------|
| Login | ✅ JavaScript | ✅ ModelState | ✅ Token |
| Register | ✅ HTML5 required | ✅ ModelState | ✅ Token |
| CompleteTailorProfile | ✅ HTML5 + attributes | ✅ ModelState + custom | ✅ Token |
| CompleteGoogleRegistration | ✅ HTML5 required | ✅ ModelState | ✅ Token |
| ChangePassword | ✅ HTML5 + custom JS | ✅ ModelState | ✅ Token |
| RequestRoleChange | ✅ HTML5 required | ✅ ModelState | ✅ Token |
| ResendVerificationEmail | ✅ HTML5 required | ✅ Basic | ✅ Token |

---

## 📝 Recommendations

### 1. Fix Broken Links
```csharp
// Add Settings action to AccountController:
[HttpGet]
public IActionResult Settings()
{
  return RedirectToUserDashboard(); // Or create a Settings view
}
```

### 2. Implement Forgot Password
```csharp
[HttpGet]
[AllowAnonymous]
public IActionResult ForgotPassword()
{
    return View();
}

[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ForgotPassword(string email)
{
    // Implementation
}
```

### 3. Remove Duplicate Actions
```csharp
// Mark as obsolete:
[Obsolete("Use CompleteTailorProfile instead")]
public IActionResult ProvideTailorEvidence() => CompleteTailorProfile();
```

### 4. Add Missing Views
Create views for:
- `Settings.cshtml` (user settings page)
- `ForgotPassword.cshtml` (password reset request)
- `ResetPassword.cshtml` (password reset form)

---

## 🔍 Quick Reference: Where Does Each View Redirect After Success?

| View | Success Redirect |
|------|------------------|
| **Login** | Dashboard (by role) OR returnUrl |
| **Register** (Customer/Corporate) | `/Account/Login` |
| **Register** (Tailor) | `/Account/CompleteTailorProfile` |
| **CompleteTailorProfile** | `/Account/Login` |
| **CompleteGoogleRegistration** (Customer/Corporate) | Dashboard (by role) |
| **CompleteGoogleRegistration** (Tailor) | `/Account/CompleteTailorProfile` |
| **ChangePassword** | Dashboard (by role) |
| **RequestRoleChange** | `/Account/Login` (after sign out) |
| **ResendVerificationEmail** | Same page with message |
| **VerifyEmail** | `/Account/Login` |
| **Logout** | `/Home/Index` |

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**Total Views Analyzed**: 7  
**Total Actions Mapped**: 20+  
**Issues Found**: 3  
**Status**: ✅ Complete
