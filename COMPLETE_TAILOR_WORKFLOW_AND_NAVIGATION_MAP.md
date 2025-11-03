# 🗺️ Complete Tailor Workflow & Navigation Analysis

## 📊 Overview
This document maps **every page** a tailor encounters from registration to daily operations, including all redirect paths, middleware checks, and decision points.

---

## 🚀 Phase 1: Registration & Onboarding

### 1.1 Entry Point: Homepage
**View:** `Views/Home/Index.cshtml`
**Controller:** `HomeController.Index()`
**Route:** `/` or `/Home/Index`

**Tailor Actions:**
- Click "سجل كخياط" (Register as Tailor) button
- Redirects to → `Account/Register`

---

### 1.2 Registration Page
**View:** `Views/Account/Register.cshtml`
**Controller:** `AccountController.Register()` [GET]
**Route:** `/Account/Register`

**User Input:**
- Full Name
- Email
- Password
- Phone Number
- User Type: **Tailor** (Selected)

**POST Action:** `AccountController.Register(POST)`
**Logic:**
```csharp
// In AccountController.Register(POST)
if (role == RegistrationRole.Tailor)
{
    return RedirectToTailorEvidence(user.Id, email, name);
}
```

**Result:**
- Creates `User` entity with:
  - `IsActive = false`
  - `RoleId = TailorRoleId`
  - No `TailorProfile` created yet
- Stores user info in `TempData`:
  - `TempData["UserId"]`
  - `TempData["UserEmail"]`
  - `TempData["UserName"]`
- Redirects to → `ProvideTailorEvidence`

---

### 1.3 Evidence Submission Page (MANDATORY)
**View:** `Views/Account/ProvideTailorEvidence.cshtml`
**Controller:** `AccountController.ProvideTailorEvidence()` [GET]
**Route:** `/Account/ProvideTailorEvidence`

**Access Scenarios:**
1. **Just Registered** (TempData exists):
   - User info loaded from TempData
   - Not authenticated yet
   
2. **Redirected from Login** (Authenticated):
   - User authenticated but incomplete profile
   - Shows warning: "يجب إكمال عملية التحقق..."

3. **Redirected by Middleware:**
   - `UserStatusMiddleware` detected incomplete tailor
   - Sets `incomplete=true` parameter

**Required Inputs:**
- ✅ Workshop Name (مطلوب)
- ✅ Phone Number (مطلوب)
- ✅ City (مطلوب)
- ✅ Address (مطلوب)
- ✅ Description (مطلوب)
- ✅ ID Document Upload (مطلوب)
- ✅ Portfolio Images (3+ images مطلوب)
- ⚠️ Experience Years (Optional)
- ⚠️ Additional Documents (Optional)
- ✅ Agree to Terms (مطلوب)

**POST Action:** `AccountController.ProvideTailorEvidence(POST)`

**Backend Logic:**
```csharp
// Creates TailorProfile
var tailorProfile = new TailorProfile
{
    UserId = model.UserId,
    FullName = model.FullName,
    ShopName = model.WorkshopName,
    IsVerified = false,
    // ... store documents
};

// ACTIVATES the user account
user.IsActive = true;
user.EmailVerificationToken = GenerateToken();
```

**Result:**
- Creates `TailorProfile` in database
- Sets `User.IsActive = true`
- Sends email verification link
- Shows success message: "تم إكمال التسجيل بنجاح!"
- Redirects to → `Account/Login`

---

## 🔐 Phase 2: First Login Attempt

### 2.1 Login Page
**View:** `Views/Account/Login.cshtml`
**Controller:** `AccountController.Login()` [GET]
**Route:** `/Account/Login`

**User Enters:**
- Email
- Password

**POST Action:** `AccountController.Login(POST)`

**Backend Flow:**
```csharp
// 1. AuthService validates credentials
var (success, error, user) = await _auth.ValidateUserAsync(email, password);

// 2. Check for incomplete tailor
if (!success && error == "TAILOR_INCOMPLETE_PROFILE" && user != null)
{
    // Sign in the tailor (temporary)
    await HttpContext.SignInAsync(...);
    
    // Redirect to evidence page
    return RedirectToAction(nameof(ProvideTailorEvidence), new { incomplete = true });
}

// 3. Check if inactive (evidence submitted, awaiting admin)
if (!user.IsActive)
{
    return Error: "حسابك قيد المراجعة من قبل الإدارة...";
}

// 4. Successful login
return RedirectToRoleDashboard(user.Role?.Name);
```

**Decision Points:**

| Condition | User Status | Redirect To | Message |
|-----------|-------------|-------------|---------|
| No TailorProfile | `IsActive = false` | `ProvideTailorEvidence` | "يجب إكمال عملية التحقق..." |
| TailorProfile exists | `IsActive = false` | Login Page (Error) | "حسابك قيد المراجعة..." |
| TailorProfile exists | `IsActive = true` | Tailor Dashboard | Login Success ✅ |

---

## 🎯 Phase 3: Admin Approval Process

### 3.1 Admin Reviews Tailor
**View:** `Views/AdminDashboard/TailorVerification.cshtml`
**Controller:** `AdminDashboardController.TailorVerification()`
**Route:** `/AdminDashboard/TailorVerification`

**Admin Sees:**
- List of pending tailors
- Evidence documents
- Portfolio images

**Admin Actions:**
1. Click "مراجعة الخياط" → `AdminDashboard/ReviewTailor/{id}`

---

### 3.2 Review Tailor Details
**View:** `Views/AdminDashboard/ReviewTailor.cshtml`
**Controller:** `AdminDashboardController.ReviewTailor(id)` [GET]
**Route:** `/AdminDashboard/ReviewTailor/{tailorId}`

**Admin Can:**
- ✅ Approve Tailor → Sets `IsActive = true`, `IsVerified = true`
- ❌ Reject Tailor → Sends email with reason
- 📝 Request More Info → Sends notification

**POST Action:** `AdminDashboardController.ApproveTailor(POST)`

**Result:**
```csharp
// Approve
user.IsActive = true;
tailorProfile.IsVerified = true;
tailorProfile.VerifiedAt = DateTime.Now;

// Send notification email
await _emailService.SendTailorApprovalEmail(user.Email);
```

**Tailor Notification:**
- Email: "تهانينا! تم الموافقة على حسابك"
- Can now login and access full dashboard

---

## 🏠 Phase 4: Post-Approval - Tailor Dashboard Access

### 4.1 Successful Login → Dashboard
**View:** `Views/Dashboards/Tailor.cshtml`
**Controller:** `DashboardsController.Tailor()`
**Route:** `/Dashboards/Tailor`

**Authorization:**
```csharp
[Authorize(Policy = "TailorPolicy")]
public async Task<IActionResult> Tailor()
```

**Middleware Check:** `UserStatusMiddleware`
```csharp
// If tailor has no profile
if (isTailor && !hasTailorProfile)
{
    return Redirect("/Account/ProvideTailorEvidence?incomplete=true");
}

// If tailor inactive (pending approval)
if (isTailor && !user.IsActive)
{
    ViewData["PendingApproval"] = true;
    // Still allows access to dashboard
}
```

**Dashboard Shows:**
- ✅ Active Orders Count
- ✅ Completed Orders Count
- ✅ New Orders Count
- ✅ Monthly Revenue
- ✅ Recent Orders Table
- ✅ Activity Feed

**Sidebar Navigation:**
- 🏠 Dashboard
- 🖼️ Portfolio Management
- 🛎️ Services Management
- ⚙️ Settings
- 🚪 Logout

---

### 4.2 Getting Started Guide (Optional)
**View:** `Views/TailorManagement/GettingStarted.cshtml`
**Controller:** `TailorManagementController.GettingStarted()`
**Route:** `/TailorManagement/GettingStarted`

**Shows 4-Step Onboarding:**
1. Complete Profile → `EditTailorProfile`
2. Add Services → `ManageServices`
3. Add Portfolio → `ManagePortfolio`
4. Get Verified → Admin Review

**Purpose:** Help new tailors set up their account

---

## 📝 Phase 5: Profile Management

### 5.1 View Public Profile
**View:** `Views/TailorPortfolio/ViewPublicTailorProfile.cshtml`
**Controller:** `TailorPortfolioController.ViewPublicTailorProfile(id)`
**Route:** `/TailorPortfolio/ViewPublicTailorProfile/{id}`

**Accessible By:**
- Customers (browsing tailors)
- The tailor themselves (preview)

**Shows:**
- Profile picture
- Shop details
- Portfolio images
- Services offered
- Reviews & ratings
- Contact information

---

### 5.2 Edit Profile
**View:** `Views/Profiles/EditTailorProfile.cshtml`
**Controller:** `ProfilesController.EditTailorProfile()` [GET]
**Route:** `/Profiles/EditTailorProfile`

**Sections:**
1. **Personal Information**
   - Full Name
   - Phone Number
   - Email (read-only)

2. **Shop Details**
   - Shop Name
   - Description
   - Specialization
   - Years of Experience

3. **Location**
   - City
   - District
   - Address
   - Latitude/Longitude

4. **Bio & Hours**
   - Bio (1000 chars max)
   - Business Hours

5. **Social Media**
   - Facebook URL
   - Instagram URL
   - Twitter URL
   - Website URL

6. **Profile Picture**
 - Upload new image

**POST Action:** `ProfilesController.EditTailorProfile(POST)`

**Updates:**
- `TailorProfile` table
- Profile picture stored in database
- Shows success: "تم تحديث ملفك الشخصي بنجاح!"

---

### 5.3 Alternative: Complete Tailor Profile (Authenticated)
**View:** `Views/Account/CompleteTailorProfile.cshtml`
**Controller:** `AccountController.CompleteTailorProfile()` [GET]
**Route:** `/Account/CompleteTailorProfile`

**Authorization:** `[Authorize(Policy = "TailorPolicy")]`

**Difference from EditTailorProfile:**
- More structured (3-step wizard)
- Focus on essential info first
- Used for **optional** updates after initial setup

**Steps:**
1. Basic Information
2. Documents & Uploads
3. Review & Submit

---

## 🖼️ Phase 6: Portfolio Management

### 6.1 Manage Portfolio
**View:** `Views/TailorManagement/ManagePortfolio.cshtml`
**Controller:** `TailorManagementController.ManagePortfolio()`
**Route:** `/TailorManagement/ManagePortfolio`

**Shows:**
- Grid of portfolio images
- Upload button
- Edit/Delete actions

**Actions:**
- Add Image → `AddPortfolioImage`
- Edit Image → `EditPortfolioImage`
- Delete Image → `DeletePortfolioImage`

---

### 6.2 Add Portfolio Image
**View:** `Views/TailorManagement/AddPortfolioImage.cshtml`
**Controller:** `TailorManagementController.AddPortfolioImage()` [GET]
**Route:** `/TailorManagement/AddPortfolioImage`

**Inputs:**
- Image Upload
- Title (Optional)
- Description (Optional)
- Is Before/After?

**POST Action:** `TailorManagementController.AddPortfolioImage(POST)`

**Stores:**
- Image in `/wwwroot/uploads/portfolio/{tailorId}/`
- Record in `PortfolioImages` table

---

### 6.3 Edit Portfolio Image
**View:** `Views/TailorManagement/EditPortfolioImage.cshtml`
**Controller:** `TailorManagementController.EditPortfolioImage(id)` [GET]
**Route:** `/TailorManagement/EditPortfolioImage/{id}`

**Can Update:**
- Title
- Description
- Replace image
- Toggle Before/After status

---

## 🛎️ Phase 7: Services Management

### 7.1 Manage Services
**View:** `Views/TailorManagement/ManageServices.cshtml`
**Controller:** `TailorManagementController.ManageServices()`
**Route:** `/TailorManagement/ManageServices`

**Shows:**
- Table of all services
- Service name, price, duration
- Active/Inactive status

**Actions:**
- Add Service → `AddService`
- Edit Service → `EditService`
- Delete Service → `DeleteService`

---

### 7.2 Add Service
**View:** `Views/TailorManagement/AddService.cshtml`
**Controller:** `TailorManagementController.AddService()` [GET]
**Route:** `/TailorManagement/AddService`

**Inputs:**
- Service Name (e.g., "تفصيل ثوب")
- Description
- Base Price
- Estimated Duration
- Category

**POST Action:** `TailorManagementController.AddService(POST)`

**Creates:**
- Record in `TailorServices` table
- Links to `TailorProfile`

---

### 7.3 Edit Service
**View:** `Views/TailorManagement/EditService.cshtml`
**Controller:** `TailorManagementController.EditService(id)` [GET]
**Route:** `/TailorManagement/EditService/{id}`

**Can Update:**
- Service details
- Pricing
- Availability

---

### 7.4 Manage Pricing
**View:** `Views/TailorManagement/ManagePricing.cshtml`
**Controller:** `TailorManagementController.ManagePricing()`
**Route:** `/TailorManagement/ManagePricing`

**Purpose:**
- Set custom pricing for different garment types
- Bulk price updates
- Special offers/discounts

---

## ⚙️ Phase 8: Settings & Account

### 8.1 Change Password
**View:** `Views/Account/ChangePassword.cshtml`
**Controller:** `AccountController.ChangePassword()` [GET]
**Route:** `/Account/ChangePassword`

**Inputs:**
- Current Password
- New Password
- Confirm New Password

**POST Action:** `AccountController.ChangePassword(POST)`

**Validates:**
- Current password is correct
- New password meets requirements
- Updates `User.PasswordHash`

---

### 8.2 Request Role Change
**View:** `Views/Account/RequestRoleChange.cshtml`
**Controller:** `AccountController.RequestRoleChange()` [GET]
**Route:** `/Account/RequestRoleChange`

**Use Case:**
- Customer wants to become Tailor
- Tailor wants additional roles

**Note:** For Tailor → Customer conversion, must contact support (not allowed directly)

---

## 📧 Phase 9: Email Verification

### 9.1 Verify Email
**Controller:** `AccountController.VerifyEmail(token)` [GET]
**Route:** `/Account/VerifyEmail?token={token}`

**Flow:**
1. User clicks link in email
2. Token validated
3. Sets `User.EmailVerified = true`
4. Shows success message
5. Redirects to Login

---

### 9.2 Resend Verification Email
**View:** `Views/Account/ResendVerificationEmail.cshtml`
**Controller:** `AccountController.ResendVerificationEmail()` [GET]
**Route:** `/Account/ResendVerificationEmail`

**Inputs:**
- Email address

**POST Action:** `AccountController.ResendVerificationEmail(POST)`

**Generates:**
- New verification token
- Sends email with new link

---

## 🚨 Middleware & Redirects

### UserStatusMiddleware Flow
**File:** `Middleware/UserStatusMiddleware.cs`

**Checks Every Request:**
```csharp
// 1. Is user authenticated?
if (context.User.Identity?.IsAuthenticated != true)
    return; // Skip

// 2. Is user a tailor?
var role = context.User.FindFirstValue(ClaimTypes.Role);
if (role?.ToLower() != "tailor")
    return; // Skip

// 3. Does tailor have profile?
var hasTailorProfile = await _unitOfWork.Tailors.GetByUserIdAsync(userId);

if (hasTailorProfile == null)
{
    // REDIRECT: Incomplete profile
    context.Response.Redirect("/Account/ProvideTailorEvidence?incomplete=true");
    return;
}

// 4. Is tailor active?
if (!user.IsActive)
{
    // Allow dashboard access but show "Pending Approval" banner
    // Set ViewData flag for dashboard
}
```

**Pages Checked:**
- All pages under `/Dashboards/Tailor`
- All pages under `/TailorManagement/*`
- All pages under `/Profiles/EditTailorProfile`

**Pages Exempt:**
- `/Account/*` (login, register, logout)
- `/Home/*` (public pages)
- `/AdminDashboard/*` (admin only)

---

## 🗺️ Complete Navigation Map

```
┌─────────────────────────────────────────────────────────────────┐
│       TAILOR REGISTRATION FLOW    │
└─────────────────────────────────────────────────────────────────┘

   START
     │
     ├──> /Home/Index
     │    Click "Register as Tailor"
   │
     ├──> /Account/Register
     │    Enter: Name, Email, Password, Phone
     │    Select: Tailor Role
     │    [POST] Creates User (IsActive=false)
     │
     ├──> /Account/ProvideTailorEvidence
     │    Upload: ID, Portfolio (3+), Business Info
     │ [POST] Creates TailorProfile (IsVerified=false)
     │     Sets User.IsActive = true
     │
     ├──> /Account/Login
     │    Shows: "تم إكمال التسجيل بنجاح!"
     │
     ├──> [User attempts login]
     │    Password Validated ✅
   │    IsActive = true ✅ (evidence submitted)
     │    BUT IsVerified = false ⏳ (awaiting admin)
     │
     ├──> Login ERROR: "حسابك قيد المراجعة من قبل الإدارة..."
     │
     ╔═══════════════════════════════════════════════════════════╗
     ║ ADMIN APPROVAL REQUIRED           ║
     ╚═══════════════════════════════════════════════════════════╝
     │
     ├──> /AdminDashboard/TailorVerification
     │    Admin sees pending tailor
     │
     ├──> /AdminDashboard/ReviewTailor/{id}
     │    Admin reviews evidence
     │    [APPROVE] Sets IsVerified = true
     │   Sends email notification
     │
     ├──> [User attempts login again]
     │    Password Validated ✅
     │    IsActive = true ✅
     │    IsVerified = true ✅
     │
     ├──> /Dashboards/Tailor ✅ SUCCESS!
│
     └──> OPERATIONAL PHASE
          │
          ├──> Profile Management
          │    ├──> /Profiles/EditTailorProfile
          │    ├──> /TailorPortfolio/ViewPublicTailorProfile
        │    └──> /Account/CompleteTailorProfile
    │
          ├──> Portfolio Management
        │    ├──> /TailorManagement/ManagePortfolio
       │    ├──> /TailorManagement/AddPortfolioImage
          │ └──> /TailorManagement/EditPortfolioImage
          │
          ├──> Services Management
        │    ├──> /TailorManagement/ManageServices
 │    ├──> /TailorManagement/AddService
       │    ├──> /TailorManagement/EditService
          │    └──> /TailorManagement/ManagePricing
          │
          ├──> Account Settings
          │    ├──> /Account/ChangePassword
    │    └──> /Account/RequestRoleChange
        │
      └──> Help & Support
          └──> /TailorManagement/GettingStarted
```

---

## 🔄 Redirect Path Summary

### Scenario 1: Just Registered (No Evidence)
```
Register → ProvideTailorEvidence (TempData)
```

### Scenario 2: Registered, Try Login (No Evidence)
```
Login → AuthService detects no profile
      → Signs in temporarily
   → Redirects to ProvideTailorEvidence?incomplete=true
```

### Scenario 3: Evidence Submitted, Try Login (Pending Approval)
```
Login → AuthService detects IsActive=false
      → Error: "حسابك قيد المراجعة..."
      → Stay on Login page
```

### Scenario 4: Approved, First Login
```
Login → AuthService validates all checks ✅
      → Redirects to /Dashboards/Tailor
```

### Scenario 5: Middleware Catch (Incomplete Profile)
```
Access any /Dashboards/* or /TailorManagement/*
      → Middleware detects no TailorProfile
      → Redirects to /Account/ProvideTailorEvidence?incomplete=true
```

### Scenario 6: Middleware Catch (Inactive Account)
```
Access /Dashboards/Tailor
      → Middleware detects IsActive=false
      → Allows access BUT sets ViewData["PendingApproval"]=true
      → Dashboard shows banner: "حسابك قيد المراجعة..."
```

---

## 📋 Critical Checkpoints

### ✅ Database State Tracking

| Stage | User.IsActive | TailorProfile Exists | TailorProfile.IsVerified | Can Login? | Can Access Dashboard? |
|-------|---------------|----------------------|--------------------------|------------|---------------------|
| Just Registered | `false` | ❌ No | N/A | ❌ No | ❌ No |
| Evidence Submitted | `true` | ✅ Yes | `false` | ❌ No* | ⚠️ Limited** |
| Admin Approved | `true` | ✅ Yes | `true` | ✅ Yes | ✅ Yes |

*Login attempt shows: "حسابك قيد المراجعة..."
**If they bypass login check, dashboard shows "Pending Approval" banner

---

## 🎯 Key Views Summary

| View File | Purpose | When Accessed |
|-----------|---------|---------------|
| `Account/Register.cshtml` | Initial registration | Entry point for new tailors |
| `Account/ProvideTailorEvidence.cshtml` | Evidence submission | After registration OR login redirect |
| `Account/Login.cshtml` | Authentication | Before every session |
| `Dashboards/Tailor.cshtml` | Main dashboard | Post-approval, daily access |
| `TailorManagement/GettingStarted.cshtml` | Onboarding guide | First-time setup help |
| `Profiles/EditTailorProfile.cshtml` | Profile editing | Update personal/business info |
| `TailorManagement/ManagePortfolio.cshtml` | Portfolio CRUD | Add/edit work samples |
| `TailorManagement/ManageServices.cshtml` | Services CRUD | Define offerings & pricing |
| `AdminDashboard/TailorVerification.cshtml` | Admin review list | Admin approval workflow |
| `AdminDashboard/ReviewTailor.cshtml` | Detailed review | Approve/reject tailors |

---

## 🚀 Workflow Best Practices

### For New Tailors:
1. ✅ Complete evidence submission IMMEDIATELY after registration
2. ✅ Check email for verification link
3. ⏳ Wait for admin approval (2-3 business days)
4. ✅ Upon approval, complete profile details
5. ✅ Add portfolio images (showcase quality)
6. ✅ Define services and pricing
7. ✅ Start receiving orders!

### For Development:
1. **Never skip evidence submission** - it's MANDATORY
2. **Middleware catches incomplete profiles** - no backdoors
3. **Admin approval required** - no auto-activation
4. **Email verification separate** - doesn't affect login
5. **Three states to handle:**
   - No profile (redirect to evidence)
   - Inactive (show pending message)
   - Active & Verified (full access)

---

## 📝 TODO: Missing Views/Features

Based on the analysis, these views/features may need creation:

1. ❓ **Tailor Notifications Page**
   - `/TailorManagement/Notifications`
   - Show approval notifications, order updates

2. ❓ **Tailor Orders Page**
   - `/TailorManagement/Orders`
   - List incoming/active/completed orders

3. ❓ **Tailor Analytics Page**
   - `/TailorManagement/Analytics`
   - Revenue charts, performance metrics

4. ❓ **Help/FAQ Page for Tailors**
   - `/TailorManagement/Help`
   - Support documentation

5. ❓ **Email Templates**
   - Approval email
   - Rejection email
   - Verification reminder

---

## 🔗 Related Documentation

- `TAILOR_AUTHENTICATION_FLOW_ANALYSIS.md` - Detailed auth logic
- `TAILOR_EVIDENCE_REDIRECT_FIX.md` - Recent redirect implementation
- `TAILOR_VERIFICATION_COMPLETE_FLOW.md` - Verification process
- `FIX_EVIDENCE_PAGE_REDIRECT.md` - Redirect troubleshooting

---

## 📞 Support Contacts

For tailors needing help:
- Email: support@tafsilk.com
- Phone: +20 123 456 7890
- Chat: Available in dashboard (post-approval)

---

**Last Updated:** Based on codebase analysis as of current date
**Status:** Complete workflow mapped and validated
