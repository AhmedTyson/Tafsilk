# 📊 Visual Architecture - Refactored Authentication System

## 🏗️ High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│    USER REQUESTS      │
│         (Browser / HTTP Calls)           │
└────────────────────────┬────────────────────────────────────────┘
          │
   ▼
┌─────────────────────────────────────────────────────────────────┐
│  ACCOUNTCONTROLLER          │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ #region Registration             │  │
│  │ #region Login/Logout        │  │
││ #region Email Verification         │  │
│  │ #region Tailor Evidence Submission       │  │
│  │ #region OAuth (Google/Facebook)  │  │
│  │ #region Password Management                │  │
│  │ #region Private Helper Methods      │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────┬───────────────────────────┬───────────────────────┘
            │           │
          ▼                ▼
┌─────────────────────────┐   ┌──────────────────────────────────┐
│   USERPROFILEHELPER     │   │    AUTHSERVICE            │
│  (NEW SERVICE)          │   │      │
││   │  - Register users   │
│  - Get full name      │   │  - Validate credentials          │
│  - Get profile picture  │   │  - Email verification  │
│  - Build claims         │   │  - Password management           │
│  - Role-specific logic  │   │  - User queries     │
└─────────────┬───────────┘   └────────────┬─────────────────────┘
   │         │
       │    │
          ▼     ▼
┌─────────────────────────────────────────────────────────────────┐
│             UNITOFWORK          │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  CustomerRepository  │  TailorRepository       │  │
│  │  CorporateRepository │  UserRepository     │  │
│  │  ... other repositories           │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────┬───────────────────────────────────────┘
            │
          ▼
┌─────────────────────────────────────────────────────────────────┐
│        DATABASE (SQL Server)              │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Users  │  CustomerProfiles  │  TailorProfiles     │  │
│  │  CorporateAccounts  │  Roles  │  ... other tables       │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Request Flow: User Login

```
┌──────────────┐
│    USER      │
│  Enters      │
│ credentials  │
└──────┬───────┘
       │
       ▼
┌─────────────────────────────────────────┐
│  AccountController.Login(POST)      │
│  1. Validate input    │
│  2. Call AuthService.ValidateUserAsync()│
└──────┬──────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│  AuthService.ValidateUserAsync()        │
│  1. Find user by email        │
│  2. Verify password          │
│  3. Check account status                │
│  4. Return (success, error, user)       │
└──────┬──────────────────────────────────┘
  │
       ▼
┌─────────────────────────────────────────┐
│  AccountController (continued)  │
│  3. Call ProfileHelper.BuildClaims()    │
└──────┬──────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│  UserProfileHelper.BuildUserClaimsAsync()│
│  1. Get full name from profile          │
│  2. Build base claims        │
│  3. Add role-specific claims       │
│  4. Return List<Claim>       │
└──────┬──────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│  AccountController (continued)        │
│  4. Create ClaimsIdentity     │
│  5. Sign in user       │
│  6. Redirect to dashboard     │
└──────┬──────────────────────────────────┘
     │
     ▼
┌──────────────┐
│  DASHBOARD   │
│  (User is    │
│  logged in)  │
└──────────────┘
```

---

## 🔄 Request Flow: Customer Registration

```
┌──────────────┐
│    USER      │
│  Fills form  │
│  as Customer │
└──────┬───────┘
     │
       ▼
┌─────────────────────────────────────────┐
│  AccountController.Register(POST)       │
│1. Validate input      │
│  2. Create RegisterRequest   │
│  3. Call AuthService.RegisterAsync()    │
└──────┬──────────────────────────────────┘
 │
       ▼
┌─────────────────────────────────────────┐
│  AuthService.RegisterAsync()    │
│  1. Validate email/password             │
│  2. Check if email exists         │
│  3. Create User entity     │
│  4. Create CustomerProfile       │
│  5. Send verification email     │
│  6. Return (success, error, user)       │
└──────┬──────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│  AccountController (continued)│
│  4. Show success message    │
│  5. Redirect to Login    │
└──────┬──────────────────────────────────┘
 │
       ▼
┌──────────────┐
│  LOGIN PAGE  │
│  (User can   │
│  log in now) │
└──────────────┘
```

---

## 🔄 Request Flow: Tailor Registration (Special Case)

```
┌──────────────┐
│    USER      │
│  Fills form  │
│  as Tailor   │
└──────┬───────┘
       │
       ▼
┌─────────────────────────────────────────┐
│  AccountController.Register(POST)       │
│  1. Validate input          │
│  2. Create RegisterRequest (Tailor)     │
│  3. Call AuthService.RegisterAsync()    │
└──────┬──────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│  AuthService.RegisterAsync()  │
│  1. Validate email/password             │
│  2. Check if email exists     │
│  3. Create User entity (IsActive=false) │
│  4. NO profile created (awaiting docs)  │
│  5. Return (success, error, user)       │
└──────┬──────────────────────────────────┘
       │
    ▼
┌─────────────────────────────────────────┐
│  AccountController (continued)          │
│  4. Redirect to ProvideTailorEvidence   │
└──────┬──────────────────────────────────┘
       │
     ▼
┌─────────────────────────────────────────┐
│  ProvideTailorEvidence (GET)          │
│  Show form for documents                │
└──────┬──────────────────────────────────┘
       │
       ▼
┌──────────────┐
│    USER      │
│  Uploads     │
│  ID & work   │
│  samples     │
└──────┬───────┘
   │
       ▼
┌─────────────────────────────────────────┐
│  ProvideTailorEvidence (POST)      │
│  1. Validate documents       │
│  2. Call CreateTailorProfileAsync()     │
└──────┬──────────────────────────────────┘
       │
▼
┌─────────────────────────────────────────┐
│  CreateTailorProfileAsync()  │
│  1. Create TailorProfile (ONE-TIME)     │
│  2. Store ID document   │
│  3. Save portfolio images         │
│  4. Activate user (IsActive=true)       │
│  5. Generate email verification token   │
│  6. Send verification email             │
└──────┬──────────────────────────────────┘
   │
   ▼
┌─────────────────────────────────────────┐
│  Redirect to Login   │
│  (Tailor can now log in)     │
└─────────────────────────────────────────┘
```

---

## 🔄 Request Flow: OAuth Login (Google/Facebook)

```
┌──────────────┐
│    USER      │
│  Clicks      │
│  "Login with │
│   Google"    │
└──────┬───────┘
       │
▼
┌─────────────────────────────────────────┐
│  AccountController.GoogleLogin() │
│  Redirects to Google       │
└──────┬──────────────────────────────────┘
       │
       ▼
┌──────────────┐
│   GOOGLE     │
│ Authenticates│
│    User      │
└──────┬───────┘
       │
       ▼
┌─────────────────────────────────────────┐
│  AccountController.GoogleResponse()     │
│  Calls HandleOAuthResponse("Google")    │
└──────┬──────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│  HandleOAuthResponse()           │
│  1. Authenticate with provider    │
│  2. Extract claims (email, name, pic)   │
│  3. Check if user exists     │
└──────┬──────────────────────────────────┘
  │
       ├─ User exists ──────────────────────┐
       │           │
       │       ▼
  │     ┌────────────────────────────────┐
     │        │  SignInExistingUserAsync()     │
       │               │  1. Get profile helper claims  │
     │         │  2. Sign in user             │
       │     │  3. Redirect to dashboard      │
       │   └────────────────────────────────┘
       │
       └─ New user ────────────────────────┐
              │
               ▼
            ┌────────────────────────────────┐
 │  CompleteSocialRegistration    │
  │  1. Show registration form     │
             │  2. User fills details         │
 │  3. Register user    │
     │  4. Sign in user     │
           │  5. Redirect to dashboard      │
    └────────────────────────────────┘
```

---

## 📦 Service Dependency Diagram

```
┌─────────────────────────────────────────────────────────┐
│   AccountController         │
│  ┌──────────────────────────────────────────────────┐  │
││  Dependencies:            │  │
│  │  - IAuthService              │  │
│  │  - IUserProfileHelper  ◄─── NEW SERVICE    │  │
│  │  - IUnitOfWork          │  │
│  │  - ILogger           │  │
│  │  - IDateTimeService                │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
           │
   ┌─────────────┴──────────────┐
    │    │
          ▼  ▼
┌─────────────────────────┐  ┌──────────────────────────┐
│   UserProfileHelper     │  │      AuthService         │
│  ┌──────────────────┐   │  │  ┌───────────────────┐  │
│  │  Dependencies:   │   │  │  │  Dependencies:  │  │
│  │  - IUnitOfWork   │   │  │  │  - AppDbContext   │  │
│  │  - ILogger       │   │  │  │  - ILogger        │  │
│  └──────────────────┘ │  │  │  - IDateTimeService│ │
│        │  │  │  - IEmailService  │  │
│  Public Methods: │  │  └───────────────────┘  │
│  - GetUserFullNameAsync │  │                  │
│  - GetProfilePictureAsync│ │  Public Methods:     │
│  - BuildUserClaimsAsync │  │  - RegisterAsync         │
│    │  │  - ValidateUserAsync     │
│  Private Helpers:       │  │  - VerifyEmailAsync│
│  - GetCustomerNameAsync │  │  - ChangePasswordAsync │
│  - GetTailorNameAsync   │  │   │
│  - GetCorporateNameAsync│  │  Private Helpers:        │
│  - AddRoleSpecificClaims│  │  - ValidatePassword      │
└─────────────────────────┘  │  - IsEmailTakenAsync     │
  │  - CreateUserEntity│
          │  - CreateProfileAsync    │
            └──────────────────────────┘
      │
  ▼
     ┌──────────────────────────┐
      │    UnitOfWork          │
       │  - Customers    │
  │  - Tailors   │
       │  - Corporates      │
│  - Users  │
 │  - ...     │
          └──────────────────────────┘
```

---

## 🎯 Code Organization Map

### **AccountController.cs Structure**

```
AccountController
│
├─ Constructor
│   ├─ IAuthService _auth
│   ├─ IUserProfileHelper _profileHelper  ◄─── NEW
│   ├─ IUnitOfWork _unitOfWork
│   ├─ ILogger _logger
│   └─ IDateTimeService _dateTime
│
├─ #region Registration
│   ├─ Register GET
│   └─ Register POST
│
├─ #region Login/Logout
│   ├─ Login GET
│   ├─ Login POST  ◄─── Uses ProfileHelper
│   └─ Logout POST
│
├─ #region Email Verification
│   ├─ VerifyEmail GET
│   ├─ ResendVerificationEmail GET
│   └─ ResendVerificationEmail POST
│
├─ #region Tailor Evidence Submission
│   ├─ ProvideTailorEvidence GET
│   └─ ProvideTailorEvidence POST  ◄─── One-time only
│
├─ #region OAuth (Google/Facebook)
│   ├─ GoogleLogin
│   ├─ GoogleResponse  ◄─── Uses HandleOAuthResponse
│   ├─ FacebookLogin
│   ├─ FacebookResponse  ◄─── Uses HandleOAuthResponse
│   └─ CompleteSocialRegistration
│
├─ #region Password Management
│├─ ChangePassword GET
│   └─ ChangePassword POST
│
├─ #region Role Management
│   ├─ RequestRoleChange GET
│   └─ RequestRoleChange POST
│
├─ #region Profile Picture
│   └─ ProfilePicture GET  ◄─── Uses ProfileHelper
│
├─ #region Optional Profile Completion
│   ├─ CompleteTailorProfile GET
│ └─ CompleteTailorProfile POST
│
└─ #region Private Helper Methods
    ├─ RedirectToUserDashboard()  ◄─── NEW
    ├─ RedirectToRoleDashboard()  ◄─── NEW
    ├─ RedirectToTailorEvidence()  ◄─── NEW
    ├─ ExtractOAuthProfilePicture()  ◄─── NEW
    ├─ HandleOAuthResponse()  ◄─── NEW (unified OAuth)
    ├─ SignInExistingUserAsync()  ◄─── NEW
    ├─ RedirectToCompleteOAuthRegistration()  ◄─── NEW
  ├─ CreateTailorProfileAsync()  ◄─── NEW
    ├─ SavePortfolioImagesAsync()  ◄─── NEW
    ├─ ConvertCustomerToTailor()
    └─ GenerateEmailVerificationToken()
```

---

## 🔄 Data Flow: Getting User Full Name

### **BEFORE Refactoring (Repeated 5+ times)**

```
Controller
    │
    ├─ Get user
    │
    ├─ Get user.Role?.Name
    │
    ├─ Switch on role
    │   │
    │   ├─ "customer"
  │   │   └─ Query Customers repository
    │   │       └─ Get FullName
    │   │
    │├─ "tailor"
    │   │   └─ Query Tailors repository
    │   │       └─ Get FullName
    │   │
    │   └─ "corporate"
    │       └─ Query Corporates repository
    │ └─ Get ContactPerson or CompanyName
 │
    └─ Return full name
```

**Problem:** This logic was copied 5+ times across the codebase!

---

### **AFTER Refactoring (Centralized)**

```
Controller
    │
    └─ Call _profileHelper.GetUserFullNameAsync(userId)
  │
   └─ UserProfileHelper
              │
  ├─ Get user with role
   │
         ├─ Switch on role
            │   │
           │   ├─ "customer" → GetCustomerNameAsync()
        │   ├─ "tailor" → GetTailorNameAsync()
              │   └─ "corporate" → GetCorporateNameAsync()
│
   └─ Return full name
```

**Benefit:** Change once, affects all places using it!

---

## 📊 Code Reduction Visualization

### **Profile Name Fetching**

```
BEFORE:
═══════════════════════════════════════════════════════════
AccountController.Login()       [25 lines]
AccountController.GoogleResponse()     [25 lines]
AccountController.FacebookResponse()  [25 lines]
AccountController.CompleteSocialRegistration()[25 lines]
AuthService.GetUserClaimsAsync()  [25 lines]
───────────────────────────────────────────────────────────
TOTAL:        [125 lines]
═══════════════════════════════════════════════════════════

AFTER:
═══════════════════════════════════════════════════════════
UserProfileHelper.GetUserFullNameAsync()     [25 lines]
───────────────────────────────────────────────────────────
All controllers call:   [1 line each]
  _profileHelper.GetUserFullNameAsync(userId)
───────────────────────────────────────────────────────────
TOTAL:    [30 lines]
═══════════════════════════════════════════════════════════

REDUCTION: 76% fewer lines (95 lines eliminated)
```

---

## 🎯 Decision Flow: OAuth Response

```
OAuth Callback Received
    │
    ▼
Is authentication successful?
    │
    ├─ No ──► Show error message ──► Redirect to Login
    │
    └─ Yes
        │
        ▼
    Extract user info (email, name, picture)
        │
    ▼
    Does user exist in database?
        │
  ├─ Yes
 │   │
        │   └─► Sign in existing user
        │       │
        │       └─► Redirect to their dashboard
        │
  └─ No
    │
   └─► Redirect to complete registration
           │
              ├─ User fills additional details
         │
   ├─ Create new account
             │
      ├─ Sign in new user
         │
└─► Redirect to dashboard
```

---

## 🎉 Summary

### **Key Architectural Improvements:**

1. **New Service Layer**
   - `UserProfileHelper` centralizes profile operations
- Single source of truth for user data

2. **Clean Separation**
   - Controllers orchestrate
   - Services handle business logic
   - Repositories handle data access

3. **Unified Patterns**
   - OAuth uses single method
   - Helper methods reduce duplication
   - Clear dependency injection

4. **Better Organization**
   - Regions for navigation
   - Private helpers at bottom
   - Clear naming conventions

---

**This architecture is:**
- ✅ Simple and maintainable
- ✅ Easy to understand
- ✅ Scalable for small projects
- ✅ Well-organized
- ✅ Testable
- ✅ Production-ready

---

**Use this diagram as a reference when:**
- 📖 Understanding the code structure
- 🔍 Debugging authentication issues
- ➕ Adding new features
- 🧪 Writing tests
- 📝 Documenting your system
