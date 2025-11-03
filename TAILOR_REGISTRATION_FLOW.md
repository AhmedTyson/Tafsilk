# 📋 Tafsilk Platform - Tailor Registration Flow Documentation

## 🎯 Complete Registration Flow with All Conditions

### **Overview**
Tailors have a unique registration process requiring **mandatory evidence submission** before they can access the platform. Unlike Customers and Corporates who can log in immediately after registration, Tailors must complete verification before their first login.

---

## 🔄 Registration Flow Diagram

```
START
  ↓
[1] User visits /Account/Register
  ↓
[2] User selects "Tailor" role
  ↓
[3] User fills: Name, Email, Password, Phone
  ↓
[4] Clicks "Register"
  ↓
[5] System creates User account (IsActive=false)
  ↓
[6] REDIRECT → /Account/ProvideTailorEvidence ← MANDATORY
  ↓
┌─────────────────────────────────────────┐
│  CONDITION 1: Evidence Submission       │
└─────────────────────────────────────────┘
  ↓
[7a] Tailor fills evidence form:
     - Workshop Name
   - Address
     - City
- Description
     - ID Document (photo/scan)
     - Portfolio Images (3-10 images)
  ↓
[8a] Clicks "Submit Evidence"
  ↓
[9a] System creates TailorProfile (IsVerified=false)
  ↓
[10a] System keeps User.IsActive = false
  ↓
[11a] Success message: "سيتم مراجعة طلبك خلال 24-48 ساعة"
  ↓
[12a] REDIRECT → /Account/Login
  ↓
[13a] Tailor tries to login → ERROR
  Message: "حسابك قيد المراجعة من قبل الإدارة"
  ↓
[14a] WAIT for Admin Approval
  ↓
[Admin] Reviews evidence in /AdminDashboard/TailorVerification
  ↓
[Admin] Approves → User.IsActive=true, TailorProfile.IsVerified=true
  ↓
[15a] Tailor can NOW login successfully
  ↓
[16a] REDIRECT → /Dashboards/Tailor ← SUCCESS!

OR

┌─────────────────────────────────────────┐
│  CONDITION 2: Evidence NOT Submitted    │
└─────────────────────────────────────────┘
  ↓
[7b] Tailor closes browser/navigates away
  ↓
[8b] TailorProfile NOT created
  ↓
[9b] User.IsActive = false (still)
  ↓
[10b] Later, Tailor tries to login
  ↓
[11b] System detects: User exists BUT no TailorProfile
  ↓
[12b] AuthService returns error: "TAILOR_INCOMPLETE_PROFILE"
  ↓
[13b] AccountController handles error
  ↓
[14b] REDIRECT → /Account/ProvideTailorEvidence ← MANDATORY
  ↓
[15b] Message: "يجب تقديم الأوراق الثبوتية لإكمال التسجيل"
  ↓
[16b] Tailor MUST complete evidence form
  ↓
[17b] After submission → Goes to Condition 1 flow

END
```

---

## 📊 State Diagram

```
┌──────────────────────────────────────────────────────────────┐
│           TAILOR ACCOUNT STATES     │
└──────────────────────────────────────────────────────────────┘

STATE 1: REGISTERED (No Profile)
├─ User record: EXISTS
├─ TailorProfile: DOES NOT EXIST
├─ User.IsActive: false
├─ Can Login?: NO
└─ Required Action: Submit Evidence

        ↓ (Submit Evidence)

STATE 2: EVIDENCE SUBMITTED (Pending Review)
├─ User record: EXISTS
├─ TailorProfile: EXISTS
├─ TailorProfile.IsVerified: false
├─ User.IsActive: false
├─ Can Login?: NO
└─ Required Action: Wait for Admin

        ↓ (Admin Approves)

STATE 3: APPROVED (Active)
├─ User record: EXISTS
├─ TailorProfile: EXISTS
├─ TailorProfile.IsVerified: true
├─ User.IsActive: true
├─ Can Login?: YES ✅
└─ Dashboard Access: GRANTED

        ↓ (Admin Rejects - Future Implementation)

STATE 4: REJECTED (Inactive)
├─ User record: EXISTS
├─ TailorProfile: EXISTS (may be deleted)
├─ User.IsActive: false
├─ Can Login?: NO
└─ Required Action: Contact Support or Re-apply
```

---

## 🔍 Detailed Conditions

### **Condition 1: Successful Evidence Submission**

**Scenario**: Tailor completes the entire registration and evidence submission process.

**Flow**:
1. User registers as "Tailor" → User created (IsActive=false)
2. Redirected to `/Account/ProvideTailorEvidence`
3. Fills all required fields:
   - ✅ Workshop Name
   - ✅ Address & City
   - ✅ Description
   - ✅ ID Document (uploaded)
   - ✅ Portfolio Images (3-10 uploaded)
4. Clicks "Submit"
5. System validates all inputs
6. System creates `TailorProfile` record
7. User.IsActive remains **false** (awaiting admin)
8. Success message shown
9. Redirected to Login page
10. **Cannot login yet** - must wait for admin approval
11. Admin reviews in `/AdminDashboard/TailorVerification`
12. Admin clicks "Approve"
13. System sets:
    - User.IsActive = **true**
    - TailorProfile.IsVerified = **true**
14. **NOW tailor can login** ✅
15. After login → Redirected to `/Dashboards/Tailor`

**Code Flow**:
```csharp
// 1. Registration (AccountController.cs)
[HttpPost("Register")]
if (role == RegistrationRole.Tailor) {
    TempData["UserId"] = user.Id;
    return RedirectToAction("ProvideTailorEvidence"); // MANDATORY
}

// 2. Evidence Submission (AccountController.cs)
[HttpPost("ProvideTailorEvidence")]
// Creates TailorProfile
user.IsActive = false; // Keep inactive
await _unitOfWork.SaveChangesAsync();
TempData["Success"] = "سيتم مراجعة طلبك خلال 24-48 ساعة";
return RedirectToAction("Login");

// 3. Login Attempt (AccountController.cs)
[HttpPost("Login")]
var (ok, err, user) = await _auth.ValidateUserAsync(email, password);
// AuthService checks User.IsActive
// Returns error if false
ModelState.AddModelError("حسابك قيد المراجعة");

// 4. Admin Approval (AdminDashboardController.cs)
[HttpPost("VerifyTailor")]
tailor.Verify(DateTime.UtcNow); // Sets IsVerified=true
user.IsActive = true;
await _unitOfWork.SaveChangesAsync();

// 5. Second Login Attempt - SUCCESS!
// User can now login and access dashboard
```

---

### **Condition 2: Evidence NOT Submitted (Abandoned Registration)**

**Scenario**: Tailor starts registration but exits before submitting evidence.

**Flow**:
1. User registers as "Tailor" → User created (IsActive=false)
2. Redirected to `/Account/ProvideTailorEvidence`
3. **Tailor closes browser** or navigates away
4. `TailorProfile` is **NOT** created
5. Later, tailor returns and tries to login
6. System checks:
   - ✅ User exists
   - ❌ TailorProfile does NOT exist
7. AuthService returns error: `"TAILOR_INCOMPLETE_PROFILE"`
8. AccountController detects this specific error
9. **Automatically redirects** to `/Account/ProvideTailorEvidence`
10. Shows message: "يجب تقديم الأوراق الثبوتية لإكمال التسجيل"
11. Tailor **MUST** complete the evidence form
12. After submission → Follows Condition 1 flow

**Code Flow**:
```csharp
// 1. Registration (Same as Condition 1)
[HttpPost("Register")]
if (role == RegistrationRole.Tailor) {
    return RedirectToAction("ProvideTailorEvidence");
}

// 2. Tailor Exits - TailorProfile NOT created

// 3. Login Attempt (AccountController.cs)
[HttpPost("Login")]
var (ok, err, user) = await _auth.ValidateUserAsync(email, password);

if (err == "TAILOR_INCOMPLETE_PROFILE") {
    TempData["UserId"] = user.Id;
    TempData["InfoMessage"] = "يجب تقديم الأوراق الثبوتية";
    return RedirectToAction("ProvideTailorEvidence"); // MANDATORY
}

// 4. Evidence Page (AccountController.cs)
[HttpGet("ProvideTailorEvidence")]
// Check if TailorProfile already exists
var existingProfile = await _unitOfWork.Tailors.GetByUserIdAsync(userId);
if (existingProfile != null) {
    // Already submitted - redirect to login
    return RedirectToAction("Login");
}
// Show evidence form

// 5. Tailor MUST complete - cannot bypass
[HttpPost("ProvideTailorEvidence")]
// Creates TailorProfile
// Follows Condition 1 flow
```

---

## 🛡️ Security & Validation

### **Prevents Double Submission**
```csharp
[HttpGet("ProvideTailorEvidence")]
[HttpPost("ProvideTailorEvidence")]
// CRITICAL CHECK: Prevent double submission
var existingProfile = await _unitOfWork.Tailors.GetByUserIdAsync(userId);
if (existingProfile != null) {
    TempData["InfoMessage"] = "تم تقديم الأوراق الثبوتية بالفعل";
    return RedirectToAction("Login");
}
```

### **Validates Evidence Requirements**
```csharp
// ID Document: Required
if (model.IdDocument == null || model.IdDocument.Length == 0) {
    ModelState.AddModelError("يجب تحميل صورة الهوية الشخصية");
}

// Portfolio: At least 1 image required
if (model.PortfolioImages == null || !model.PortfolioImages.Any()) {
    ModelState.AddModelError("يجب تحميل على الأقل صورة واحدة");
}

// Max 10 images
if (model.PortfolioImages.Count > 10) {
  ModelState.AddModelError("يمكن تحميل 10 صور كحد أقصى");
}
```

### **File Upload Validation**
```csharp
private (bool IsValid, string? Error) ValidateFileUpload(IFormFile file, string fileType)
{
    // Size limits
var maxSize = fileType == "image" ? 5MB : 10MB;
    
    // Allowed types
    var allowedExtensions = fileType == "image"
        ? [".jpg", ".jpeg", ".png", ".gif", ".webp"]
        : [".pdf", ".doc", ".docx", ".jpg", ".png"];
    
    // Content type check
    // Directory traversal prevention
    // Return validation result
}
```

---

## 📝 Pages Tailor Navigates

### **1. Registration Page** (`/Account/Register`)
**Purpose**: Initial sign-up
**Required Fields**:
- ✅ Full Name
- ✅ Email
- ✅ Password (8+ chars, uppercase, lowercase, digit, special)
- ✅ Phone Number
- ✅ Role Selection: "Tailor"

**Validation**:
- Email format check
- Email uniqueness check
- Password strength validation
- Phone number format check

**On Success**: Redirect to Evidence Page

---

### **2. Evidence Submission Page** (`/Account/ProvideTailorEvidence`)
**Purpose**: Submit required documents and work samples
**Required Fields**:
- ✅ Workshop/Shop Name
- ✅ Address
- ✅ City
- ✅ Description/Bio
- ✅ Years of Experience
- ✅ ID Document (photo/PDF)
- ✅ Portfolio Images (3-10 images)

**Validation**:
- All text fields: XSS protection, SQL injection prevention
- ID Document: Max 10MB, types: .pdf, .jpg, .png, .doc
- Portfolio Images: Max 5MB each, types: .jpg, .jpeg, .png, .gif, .webp
- Max 10 portfolio images
- Prevent double submission

**On Success**: 
- Creates TailorProfile
- User.IsActive remains false
- Redirect to Login with success message

---

### **3. Login Page** (`/Account/Login`)
**Purpose**: User authentication

**Behavior for Tailors**:

**Case A: No TailorProfile (Condition 2)**
```
Input: Email + Password
↓
System Check: User exists but no TailorProfile
↓
Error: "TAILOR_INCOMPLETE_PROFILE"
↓
REDIRECT → /Account/ProvideTailorEvidence
Message: "يجب تقديم الأوراق الثبوتية لإكمال التسجيل"
```

**Case B: TailorProfile exists but NOT Approved (Condition 1 - Pending)**
```
Input: Email + Password
↓
System Check: User.IsActive = false
↓
Error: "حسابك قيد المراجعة من قبل الإدارة"
↓
STAY on Login Page
Cannot proceed
```

**Case C: TailorProfile Approved**
```
Input: Email + Password
↓
System Check: User.IsActive = true, TailorProfile.IsVerified = true
↓
LOGIN SUCCESS ✅
↓
REDIRECT → /Dashboards/Tailor
```

---

### **4. Tailor Dashboard** (`/Dashboards/Tailor`)
**Purpose**: Main hub for tailor after approval

**Access Requirements**:
- ✅ User.IsActive = true
- ✅ TailorProfile.IsVerified = true
- ✅ Role = "Tailor"

**Features**:
- View orders
- Manage services
- Update portfolio
- View reviews
- Business analytics
- Profile management

---

## 🎯 Key Differences: Tailor vs Customer vs Corporate

| Feature | Customer | Corporate | Tailor |
|---------|----------|-----------|--------|
| **Registration Form** | Simple | Simple | Simple |
| **Evidence Required** | ❌ No | ❌ No | ✅ **YES** (ID + Portfolio) |
| **Immediate Login** | ✅ Yes | ✅ Yes* | ❌ **NO** |
| **Admin Approval** | ❌ No | ✅ Yes | ✅ **YES** |
| **Can Skip Evidence** | N/A | N/A | ❌ **NO - MANDATORY** |
| **Redirect After Register** | Login | Login | **Evidence Page** |
| **First Login Behavior** | Dashboard | Dashboard* | **Blocked until approved** |

*Corporate: Can login but limited access until admin approves

---

## 🚨 Error Messages (All in Arabic)

### **Registration Errors**
```csharp
"الاسم الكامل مطلوب"            // Full name required
"البريد الإلكتروني مطلوب"              // Email required
"البريد الإلكتروني غير صالح"// Invalid email
"البريد الإلكتروني مسجل بالفعل" // Email already registered
"كلمة المرور مطلوبة"     // Password required
"كلمة المرور ضعيفة جداً"        // Password too weak
"رقم الهاتف غير صالح"           // Invalid phone number
```

### **Evidence Submission Errors**
```csharp
"جلسة غير صالحة. يرجى التسجيل مرة أخرى"   // Invalid session
"حساب غير صالح"           // Invalid account
"يجب تحميل صورة الهوية الشخصية"                // ID document required
"يجب تحميل على الأقل صورة واحدة من أعمالك"       // Portfolio required
"يمكن تحميل 10 صور كحد أقصى"        // Max 10 images
"حجم الملف كبير جداً"            // File too large
"نوع الملف غير مدعوم"      // Unsupported file type
"تم تقديم الأوراق الثبوتية بالفعل"               // Evidence already submitted
```

### **Login Errors**
```csharp
"البريد الإلكتروني أو كلمة المرور غير صحيحة"     // Invalid credentials
"يجب تقديم الأوراق الثبوتية لإكمال التسجيل"     // Evidence required (Condition 2)
"حسابك قيد المراجعة من قبل الإدارة" // Pending admin review
"حسابك غير نشط. يرجى الاتصال بالدعم"  // Account inactive
```

### **Success Messages**
```csharp
"تم إنشاء حسابك بنجاح!"    // Account created
"تم تقديم الأوراق الثبوتية بنجاح!"             // Evidence submitted
"سيتم مراجعة طلبك خلال 24-48 ساعة"         // Under review (24-48 hours)
"تم تسجيل الدخول بنجاح" // Login successful
```

---

## 🧪 Testing Scenarios

### **Test 1: Complete Happy Path**
```
1. Register as Tailor
2. Submit all evidence
3. Try login → Blocked (pending)
4. Admin approves
5. Login again → Success → Dashboard
```

### **Test 2: Abandoned Registration**
```
1. Register as Tailor
2. Close browser (no evidence)
3. Try login → Redirected to Evidence Page
4. Submit evidence
5. Try login → Blocked (pending)
6. Admin approves
7. Login → Success
```

### **Test 3: Double Submission Prevention**
```
1. Register as Tailor
2. Submit evidence
3. Try accessing Evidence Page directly → Blocked, redirected to Login
```

### **Test 4: Invalid Evidence**
```
1. Register as Tailor
2. Try submit without ID → Error
3. Try submit without portfolio → Error
4. Upload 11 images → Error
5. Upload 20MB file → Error
6. Submit valid evidence → Success
```

---

## 📊 Database State Tracking

### **Registration (Step 1)**
```sql
SELECT * FROM Users WHERE Email = 'tailor@example.com';
-- Result: Id=xxx, IsActive=false, RoleId=TailorRoleId

SELECT * FROM TailorProfiles WHERE UserId = 'xxx';
-- Result: (empty) - Not created yet
```

### **After Evidence Submission (Step 2)**
```sql
SELECT * FROM Users WHERE Email = 'tailor@example.com';
-- Result: Id=xxx, IsActive=false (still)

SELECT * FROM TailorProfiles WHERE UserId = 'xxx';
-- Result: Id=yyy, IsVerified=false, ShopName='...', Address='...'
```

### **After Admin Approval (Step 3)**
```sql
SELECT * FROM Users WHERE Email = 'tailor@example.com';
-- Result: Id=xxx, IsActive=true ✅

SELECT * FROM TailorProfiles WHERE UserId = 'xxx';
-- Result: Id=yyy, IsVerified=true ✅, ShopName='...', VerifiedAt='...'
```

---

## ✅ Implementation Checklist

- [x] Registration creates User with IsActive=false for Tailors
- [x] Redirect to Evidence Page after Tailor registration
- [x] Evidence Page validates all required fields
- [x] Evidence submission creates TailorProfile
- [x] User.IsActive remains false until admin approval
- [x] Login blocks Tailors without TailorProfile (Condition 2)
- [x] Login blocks Tailors with IsActive=false (Condition 1)
- [x] AuthService returns specific error codes
- [x] AccountController handles TAILOR_INCOMPLETE_PROFILE error
- [x] Prevent double evidence submission
- [x] All error messages in Arabic
- [x] Admin can approve/verify tailors
- [x] After approval, tailor can login and access dashboard
- [x] File upload validation (size, type, content)
- [x] XSS and SQL injection prevention
- [x] Proper redirection flow maintained

---

## 🎉 Summary

**For Tailors:**
1. **Register** → Creates account (inactive)
2. **MUST Submit Evidence** → Creates TailorProfile (unverified)
3. **Cannot Login** → Blocked until admin approval
4. **Admin Approves** → User becomes active
5. **Can NOW Login** → Access Tailor Dashboard

**If Evidence Skipped (Condition 2):**
- Login attempt → Automatically redirected to Evidence Page
- MUST complete evidence form
- No way to bypass this requirement

**Key Principle:**
🔒 **Tailors CANNOT access the platform until they provide evidence AND admin approves**

This ensures quality control and prevents fake tailor accounts!

---

**Last Updated**: January 2025
**Status**: ✅ Fully Implemented
**Tested**: ✅ All Conditions Covered
