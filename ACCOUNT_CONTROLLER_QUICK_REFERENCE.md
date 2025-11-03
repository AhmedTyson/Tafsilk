# AccountController Quick Reference Card 🎯

## 📋 Quick Overview

**File**: `TafsilkPlatform.Web\Controllers\AccountController.cs`
**Status**: ✅ Fully Functional
**Build**: ✅ Success
**Views**: ✅ Complete

---

## 🔐 Authentication Endpoints

### Registration
```
GET  /Account/Register
POST /Account/Register
```
**Parameters**: `name`, `email`, `password`, `userType`, `phoneNumber?`

### Login
```
GET  /Account/Login
POST /Account/Login
```
**Parameters**: `email`, `password`, `rememberMe`, `returnUrl?`

### Logout
```
POST /Account/Logout
```

---

## 👔 Tailor-Specific Endpoints

### Evidence Submission (NEW ✨)
```
GET  /Account/ProvideTailorEvidence
POST /Account/ProvideTailorEvidence
```
**Purpose**: ONE-TIME submission of identity documents and portfolio
**Files**: ID document + 3+ portfolio images

### Profile Completion
```
GET  /Account/CompleteTailorProfile
POST /Account/CompleteTailorProfile
```
**Purpose**: Add additional workshop details after verification

---

## 🔗 OAuth Endpoints

### Google
```
GET /Account/GoogleLogin
GET /Account/GoogleResponse
```

### Facebook
```
GET /Account/FacebookLogin
GET /Account/FacebookResponse
```

### Complete Social Registration
```
GET  /Account/CompleteSocialRegistration
POST /Account/CompleteSocialRegistration
```

---

## 🔑 Password Management

### Change Password (Authenticated)
```
GET  /Account/ChangePassword
POST /Account/ChangePassword
```
**Parameters**: `CurrentPassword`, `NewPassword`, `ConfirmPassword`

### Forgot Password
```
GET  /Account/ForgotPassword
POST /Account/ForgotPassword
```
**Parameters**: `email`

### Reset Password
```
GET  /Account/ResetPassword?token={token}
POST /Account/ResetPassword
```
**Parameters**: `Token`, `NewPassword`, `ConfirmPassword`

---

## ✉️ Email Verification

### Verify Email
```
GET /Account/VerifyEmail?token={token}
```

### Resend Verification
```
GET  /Account/ResendVerificationEmail
POST /Account/ResendVerificationEmail
```
**Parameters**: `email`

---

## 🎭 Role Management

### Request Role Change
```
GET  /Account/RequestRoleChange
POST /Account/RequestRoleChange
```
**Supports**: Customer → Tailor conversion

---

## 🖼️ Utility Endpoints

### Profile Picture
```
GET /Account/ProfilePicture/{userId}
```
**Returns**: Image from database (JPEG/PNG)

### Settings
```
GET /Account/Settings
```
**Redirects** to role-specific dashboard

---

## 🎯 Registration Flows

### Customer Registration
```
1. Register → Email Verification → Login → Customer Dashboard
```

### Tailor Registration (NEW FLOW ✨)
```
1. Register as Tailor
2. ProvideTailorEvidence (ID + Portfolio)
3. Email Verification
4. Login → Tailor Dashboard (IsActive=true, IsVerified=false)
5. Admin Review → IsVerified=true
```

### Corporate Registration
```
1. Register → Email Verification → Login → Corporate Dashboard
```

---

## 📊 Response Patterns

### Success Redirects

| Role | Dashboard URL |
|------|--------------|
| Customer | `/Dashboards/Customer` |
| Tailor | `/Dashboards/Tailor` |
| Corporate | `/Dashboards/Corporate` |
| Admin | `/AdminDashboard/Index` |

### TempData Messages

| Key | Usage |
|-----|-------|
| `RegisterSuccess` | Registration success message |
| `SuccessMessage` | General success message |
| `ErrorMessage` | General error message |
| `InfoMessage` | Informational message |
| `UserId` | Pass user ID between actions |
| `UserEmail` | Pass email between actions |
| `UserName` | Pass name between actions |
| `OAuthProvider` | OAuth provider name |
| `OAuthEmail` | OAuth email |
| `OAuthName` | OAuth name |
| `OAuthPicture` | OAuth profile picture URL |
| `OAuthId` | OAuth provider ID |

---

## 🛡️ Security Features

### Password Security
- ✅ BCrypt hashing via `PasswordHasher`
- ✅ Token expiration (1 hour for reset, 24 hours for verification)

### CSRF Protection
- ✅ `[ValidateAntiForgeryToken]` on all POST actions

### Authorization
- ✅ `[Authorize]` at controller level
- ✅ `[AllowAnonymous]` on public actions

---

## 📝 Model Classes Used

| Model | Purpose |
|-------|---------|
| `RegisterRequest` | User registration |
| `CompleteGoogleRegistrationViewModel` | OAuth registration |
| `CompleteTailorProfileRequest` | Tailor profile/evidence |
| `ChangePasswordViewModel` | Password change |
| `ResetPasswordViewModel` | Password reset |
| `RoleChangeRequestViewModel` | Role conversion |

---

## 🚀 Quick Commands

### Build
```bash
cd TafsilkPlatform.Web
dotnet build
```

### Run
```bash
cd TafsilkPlatform.Web
dotnet run
```

### Database
```bash
# Drop and recreate
dotnet ef database drop --force
dotnet ef database update
```

---

## 🐛 Common Issues & Solutions

### Issue: "ProvideTailorEvidence view not found"
**Solution**: ✅ FIXED - View created at `Views/Account/ProvideTailorEvidence.cshtml`

### Issue: "AppSettings table already exists"
**Solution**: ✅ FIXED - Database initialization now uses migrations only

### Issue: Tailor can access evidence page multiple times
**Solution**: ✅ IMPLEMENTED - One-time check prevents duplicate submissions

---

## 📞 Dependencies

```csharp
public AccountController(
  IAuthService auth,         // Authentication logic
IUserRepository userRepository,         // User queries
    IUnitOfWork unitOfWork,        // Database transactions
    IFileUploadService fileUploadService,   // File handling
ILogger<AccountController> logger,      // Logging
    IDateTimeService dateTime)              // Time operations
```

---

## 📚 Related Documentation

- `ACCOUNT_CONTROLLER_REFIX_COMPLETE.md` - Complete fix documentation
- `DATABASE_INITIALIZATION_FIX_COMPLETE.md` - Database fix details
- `TAILOR_VERIFICATION_COMPLETE_FLOW.md` - Tailor verification process

---

## ✅ Status Summary

| Component | Status |
|-----------|--------|
| Build | ✅ Success |
| Views | ✅ Complete (10/10) |
| Authentication | ✅ Working |
| OAuth | ✅ Configured |
| Password Reset | ✅ Working |
| Email Verification | ✅ Working |
| Tailor Evidence | ✅ NEW - Working |
| Security | ✅ Implemented |
| Documentation | ✅ Complete |

---

**Last Updated**: November 3, 2025
**Version**: 1.0
**Status**: PRODUCTION READY 🚀
