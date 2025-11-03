# Quick Reference - Refactored Account Management

## 🎯 Common Operations Quick Guide

### **1. Get User's Full Name**

```csharp
// ✅ NEW WAY (anywhere you have IUserProfileHelper)
var fullName = await _profileHelper.GetUserFullNameAsync(userId);

// ❌ OLD WAY (don't do this anymore)
var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
var fullName = customer?.FullName ?? user.Email ?? "مستخدم";
// ... and repeat for tailor, corporate...
```

---

### **2. Get Profile Picture**

```csharp
// ✅ NEW WAY
var (imageData, contentType) = await _profileHelper.GetProfilePictureAsync(userId);
if (imageData != null)
{
    return File(imageData, contentType ?? "image/jpeg");
}

// ❌ OLD WAY (30+ lines of checking each profile type)
```

---

### **3. Build Authentication Claims**

```csharp
// ✅ NEW WAY
var claims = await _profileHelper.BuildUserClaimsAsync(user);
var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

// ❌ OLD WAY (40+ lines building claims manually)
```

---

### **4. Register a New User**

```csharp
var request = new RegisterRequest
{
    Email = email,
    Password = password,
    FullName = name,
    PhoneNumber = phoneNumber,
    Role = RegistrationRole.Customer // or Tailor, Corporate
};

var (success, error, user) = await _authService.RegisterAsync(request);

if (success && user != null)
{
    // Success - user created
}
else
{
    // Error - show error message
}
```

---

### **5. Validate Login Credentials**

```csharp
var (success, error, user) = await _authService.ValidateUserAsync(email, password);

if (success && user != null)
{
    // Build claims and sign in
    var claims = await _profileHelper.BuildUserClaimsAsync(user);
    // ... sign in
}
```

---

### **6. Redirect to User's Dashboard**

```csharp
// ✅ NEW WAY
return RedirectToUserDashboard(); // Uses current user's role

// or with explicit role
return RedirectToRoleDashboard(roleName);

// Automatically redirects:
// - Tailors → /Dashboards/Tailor
// - Corporates → /Dashboards/Corporate
// - Others → /Dashboards/Customer
```

---

### **7. Send Email Verification**

```csharp
// In background (non-blocking)
_ = Task.Run(async () =>
{
    try
    {
        await _emailService.SendEmailVerificationAsync(
            user.Email,
     fullName,
            verificationToken);
    }
    catch (Exception ex)
    {
_logger.LogError(ex, "Failed to send email");
    }
});
```

---

### **8. Verify Email**

```csharp
var (success, error) = await _authService.VerifyEmailAsync(token);

TempData[success ? "RegisterSuccess" : "ErrorMessage"] = 
success ? "تم تأكيد البريد بنجاح!" : error ?? "فشل التحقق";
```

---

## 🔧 Dependency Injection Setup

### **In Your Controller Constructor**

```csharp
public class YourController : Controller
{
    private readonly IAuthService _authService;
    private readonly IUserProfileHelper _profileHelper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<YourController> _logger;

  public YourController(
        IAuthService authService,
     IUserProfileHelper profileHelper,
        IUnitOfWork unitOfWork,
        ILogger<YourController> logger)
    {
        _authService = authService;
 _profileHelper = profileHelper;
   _unitOfWork = unitOfWork;
        _logger = logger;
    }
}
```

### **Already Registered in Program.cs**

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserProfileHelper, UserProfileHelper>();
// ... other services
```

---

## 📋 Code Structure Navigator

### **AccountController Structure**

```
AccountController.cs
│
├─ #region Registration
│   ├─ GET  Register()          → Show registration form
│   └─ POST Register(...)       → Process registration
│
├─ #region Login/Logout
│   ├─ GET  Login()   → Show login form
│   ├─ POST Login(...)          → Process login
│   └─ POST Logout()            → Sign out user
│
├─ #region Email Verification
│   ├─ GET  VerifyEmail(token)
│   ├─ GET  ResendVerificationEmail()
│   └─ POST ResendVerificationEmail(email)
│
├─ #region Tailor Evidence Submission
│   ├─ GET  ProvideTailorEvidence()
│   └─ POST ProvideTailorEvidence(model)
│
├─ #region OAuth (Google/Facebook)
│   ├─ GoogleLogin()
│   ├─ GoogleResponse()
│   ├─ FacebookLogin()
│   ├─ FacebookResponse()
│   └─ CompleteSocialRegistration()
│
├─ #region Password Management
│   ├─ GET  ChangePassword()
│   └─ POST ChangePassword(model)
│
├─ #region Role Management
│   ├─ GET  RequestRoleChange()
│ └─ POST RequestRoleChange(model)
│
├─ #region Profile Picture
│   └─ GET  ProfilePicture(id)
│
├─ #region Optional Profile Completion
│   ├─ GET  CompleteTailorProfile()
│   └─ POST CompleteTailorProfile(model)
│
└─ #region Private Helper Methods
 ├─ RedirectToUserDashboard()
    ├─ RedirectToRoleDashboard(roleName)
    ├─ RedirectToTailorEvidence(...)
    ├─ ExtractOAuthProfilePicture(...)
  ├─ SignInExistingUserAsync(...)
    ├─ RedirectToCompleteOAuthRegistration(...)
    ├─ CreateTailorProfileAsync(...)
    ├─ SavePortfolioImagesAsync(...)
    ├─ ConvertCustomerToTailor(...)
    └─ GenerateEmailVerificationToken()
```

---

### **UserProfileHelper Structure**

```
UserProfileHelper.cs
│
├─ GetUserFullNameAsync(userId, roleName?)
│   └─ Gets full name from appropriate profile
│
├─ GetProfilePictureAsync(userId)
│   └─ Returns (imageData, contentType)
│
├─ BuildUserClaimsAsync(user)
│   └─ Returns List<Claim> for authentication
│
└─ Private Helpers
 ├─ GetCustomerNameAsync(userId)
    ├─ GetTailorNameAsync(userId)
    ├─ GetCorporateNameAsync(userId)
    └─ AddRoleSpecificClaimsAsync(claims, userId, roleName)
```

---

### **AuthService Structure**

```
AuthService.cs
│
├─ #region Registration
│   ├─ RegisterAsync(request)
│   └─ Helpers: ValidateRegistrationRequest, CreateUserEntity, CreateProfileAsync
│
├─ #region Login Validation
│   └─ ValidateUserAsync(email, password)
│
├─ #region Email Verification
│   ├─ VerifyEmailAsync(token)
│   └─ ResendVerificationEmailAsync(email)
│
├─ #region Password Management
│   └─ ChangePasswordAsync(userId, currentPassword, newPassword)
│
├─ #region User Queries
│   ├─ GetUserByIdAsync(userId)
│   ├─ GetUserByEmailAsync(email)
│   └─ IsInRoleAsync(userId, roleName)
│
├─ #region Claims Building
│   └─ GetUserClaimsAsync(user)
│
├─ #region Admin Operations
│   ├─ SetUserActiveStatusAsync(userId, isActive)
│   ├─ VerifyTailorAsync(tailorId, isVerified)
│   └─ ApproveCorporateAsync(corporateId, isApproved)
│
└─ #region Private Helper Methods
    ├─ ValidateRegistrationRequest(request)
    ├─ IsEmailTakenAsync(email)
    ├─ IsPhoneTakenAsync(phoneNumber)
    ├─ IsValidEmail(email)
    ├─ ValidatePassword(password)
    ├─ CreateUserEntity(request)
    ├─ CreateProfileAsync(userId, request)
    ├─ SendEmailVerificationAsync(user, fullName)
    ├─ EnsureRoleAsync(role)
    ├─ UpdateLastLoginAsync(userId)
    ├─ GetUserFullNameAsync(userId)
    ├─ AddRoleSpecificClaims(claims, user)
    └─ GenerateEmailVerificationToken()
```

---

## 🎯 Common Patterns

### **Pattern 1: Service Method with Error Tuple**

```csharp
// Most auth methods return (bool Succeeded, string? Error, T? Data)
var (success, error, user) = await _authService.RegisterAsync(request);

if (success && user != null)
{
    // Success path
}
else
{
    // Error path - show error message
    ModelState.AddModelError(string.Empty, error ?? "Operation failed");
}
```

---

### **Pattern 2: Background Task (Fire and Forget)**

```csharp
// For non-critical operations like sending emails
_ = Task.Run(async () =>
{
    try
    {
        await _emailService.SendEmailAsync(...);
    }
    catch (Exception ex)
    {
    _logger.LogError(ex, "Background task failed");
    }
});
```

---

### **Pattern 3: TempData for Cross-Request Data**

```csharp
// Store data for next request
TempData["UserId"] = userId.ToString();
TempData["InfoMessage"] = "Success message";

// Read in next action
var userIdStr = TempData["UserId"]?.ToString();

// Keep data for another request
TempData.Keep("UserId");

// Peek without removing
var email = TempData.Peek("UserEmail")?.ToString();
```

---

### **Pattern 4: Early Return for Validation**

```csharp
// Check conditions and return early
if (string.IsNullOrEmpty(email))
{
    ModelState.AddModelError(nameof(email), "Email is required");
    return View(model);
}

if (User.Identity?.IsAuthenticated == true)
{
    return RedirectToUserDashboard();
}

// Main logic continues here...
```

---

## ⚡ Performance Tips

### **1. Use AsNoTracking for Read-Only Queries**

```csharp
// ✅ Good for login (read-only)
var user = await _db.Users
    .AsNoTracking()
    .Include(u => u.Role)
  .FirstOrDefaultAsync(u => u.Email == email);

// ❌ Don't track if you won't update
```

---

### **2. Background Tasks for Non-Critical Operations**

```csharp
// ✅ Don't block registration for email sending
_ = Task.Run(async () => await SendEmailAsync());

// ❌ Don't await if not critical
await SendEmailAsync(); // This blocks the user
```

---

### **3. Cache Role Lookups**

```csharp
// If roles are static, consider caching
// For now, database lookup is fine for small scale
```

---

## 🐛 Debugging Tips

### **Enable Detailed Logging**

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "TafsilkPlatform.Web.Controllers.AccountController": "Information",
      "TafsilkPlatform.Web.Services.AuthService": "Information",
      "TafsilkPlatform.Web.Services.UserProfileHelper": "Information"
    }
  }
}
```

---

### **Check Logs for Authentication Issues**

```
[AuthService] Registration attempt: {Email}, Role: {Role}
[AuthService] User created: {UserId}, Email: {Email}
[AuthService] Login attempt for: {Email}
[AuthService] Login successful: {UserId}
```

---

### **Common Issues & Solutions**

| Issue | Check | Solution |
|-------|-------|----------|
| Claims not updating after profile change | Cookie not refreshed | Sign out and sign in again |
| Profile picture not loading | User ID correct? | Check ProfilePicture endpoint logs |
| Email not sending | SMTP configured? | Check Email:Username in user secrets |
| Tailor can't login | Profile created? | Check if ProvideTailorEvidence was completed |

---

## 📱 Testing Workflows

### **1. Customer Registration**
1. GET  `/Account/Register`
2. POST `/Account/Register` (userType=customer)
3. Email verification sent
4. GET  `/Account/VerifyEmail?token=...`
5. GET  `/Account/Login`
6. POST `/Account/Login`
7. Redirects to `/Dashboards/Customer`

---

### **2. Tailor Registration**
1. GET  `/Account/Register`
2. POST `/Account/Register` (userType=tailor)
3. Redirects to `/Account/ProvideTailorEvidence`
4. POST `/Account/ProvideTailorEvidence` (with documents)
5. Email verification sent
6. GET  `/Account/VerifyEmail?token=...`
7. GET  `/Account/Login`
8. POST `/Account/Login`
9. Redirects to `/Dashboards/Tailor` (pending admin approval)

---

### **3. OAuth Login (Google)**
1. GET `/Account/GoogleLogin`
2. Redirects to Google
3. GET `/Account/GoogleResponse` (callback)
4. If new user → `/Account/CompleteSocialRegistration`
5. POST `/Account/CompleteSocialRegistration`
6. Redirects to appropriate dashboard

---

## 🔐 Security Notes

✅ **Implemented:**
- Password hashing with BCrypt
- Email verification tokens
- Anti-forgery tokens on forms
- HTTPS in production
- Secure cookies (HttpOnly, Secure)
- Account lockout after failed logins (via middleware)

✅ **Best Practices:**
- Never log passwords
- Use parameterized queries (EF Core handles this)
- Validate all user input
- Use transactions for critical operations

---

## 📚 Further Reading

- **ASP.NET Core Authentication**: [Microsoft Docs](https://docs.microsoft.com/aspnet/core/security/authentication/)
- **Async Best Practices**: Avoid blocking, use async all the way
- **Dependency Injection**: Constructor injection pattern
- **Repository Pattern**: IUnitOfWork wraps repositories

---

**Happy Coding! 🚀**

This refactored code is designed to be:
- ✅ Easy to understand for beginners
- ✅ Simple to maintain
- ✅ Quick to extend
- ✅ Safe and secure
- ✅ Production-ready for small-scale projects
