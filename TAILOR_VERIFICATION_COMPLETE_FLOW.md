# Tailor Verification Complete Flow

## 🎯 Overview

This document explains the **complete tailor verification flow** from registration to admin approval, clarifying the difference between:
- **Tailor Evidence Submission** (Tailor's responsibility)
- **Admin Verification** (Admin's responsibility)

---

## 📋 Complete Flow Diagram

### Step 1: Tailor Registration
```
User clicks "Register as Tailor"
  ↓
Fills registration form (name, email, password)
  ↓
System creates User account:
  - Role = "Tailor"
  - IsActive = false
  - NO TailorProfile yet ❌
  ↓
Redirects to: /Account/ProvideTailorEvidence
```

**Page:** `/Account/ProvideTailorEvidence`  
**Purpose:** Tailor provides evidence documents  
**Required:**
- Shop name, address, city
- Phone number
- Experience years
- ID document (image)
- Portfolio images (minimum 3)
- Terms acceptance

---

### Step 2: Tailor Evidence Submission
```
Tailor completes all form fields
  ↓
Uploads required documents
  ↓
Clicks "Submit Application"
  ↓
System creates TailorProfile:
  - UserId = tailor's user ID
  - IsVerified = false ❌ (awaiting admin review)
  - Stores documents
  ↓
Sets User.IsActive = true ✅ (can now login)
  ↓
Sends email verification
  ↓
Redirects to: /Account/Login
  ↓
Shows success message: "Application submitted. Check your email."
```

**Result:**  
✅ TailorProfile created  
✅ Can login  
❌ NOT verified yet (IsVerified = false)  
❌ Limited features only

---

### Step 3: Tailor First Login
```
Tailor logs in with email/password
  ↓
AuthService checks:
  - Email + password valid? ✅
  - TailorProfile exists? ✅
  - IsActive? ✅
  ↓
Login successful
  ↓
Redirects to: /Dashboards/Tailor
  ↓
Middleware checks:
  - TailorProfile exists? ✅
  - IsVerified? ❌ NO
  ↓
Sets HttpContext.Items["PendingApproval"] = true
  ↓
Dashboard shows YELLOW warning banner:
  "حسابك قيد المراجعة من قبل الإدارة. سيتم تفعيله خلال 2-3 أيام عمل."
```

**Tailor can:**  
✅ View dashboard (limited)  
✅ See pending approval message  
✅ Add more portfolio images  
✅ Update profile info  

**Tailor CANNOT:**  
❌ Receive orders  
❌ Accept payments  
❌ Full features access

---

### Step 4: Admin Review Process

#### 4.1: Admin Accesses Verification Page
```
Admin logs in
  ↓
Navigates to: /Admin/Tailors/Verification
  (or: /AdminDashboard/TailorVerification)
  ↓
Sees list of pending tailors:
  - Name, shop name, email
  - City, address
  - Registration date
  - Portfolio image count
  - "Pending" badge
```

**Page:** `/Admin/Tailors/Verification`  
**Controller:** `AdminDashboardController.TailorVerification()`  
**Purpose:** Show list of tailors awaiting verification  

---

#### 4.2: Admin Reviews Individual Tailor
```
Admin clicks "Review Application" on a tailor
  ↓
Redirects to: /Admin/Tailors/{id}/Review
  ↓
Shows detailed view:
  - Tailor personal info
- Shop information
  - Uploaded ID document
  - Portfolio images (gallery)
  - Services (if any)
  - Registration date
  ↓
Admin has 2 options:
  1. Approve ✅
  2. Reject ❌
```

**Page:** `/Admin/Tailors/{id}/Review`  
**Controller:** `AdminDashboardController.ReviewTailor(id)`  
**View:** `Views/AdminDashboard/ReviewTailor.cshtml`

---

#### 4.3a: Admin Approves Tailor
```
Admin clicks "Approve"
  ↓
POST to: /Admin/Tailors/{id}/Approve
  ↓
System updates:
  - TailorProfile.IsVerified = true ✅
  - TailorProfile.UpdatedAt = now
  ↓
Creates notification for tailor:
  Title: "تم التحقق من حسابك"
  Message: "تهانينا! تم التحقق من حسابك بنجاح..."
  Type: "Success"
  ↓
Logs admin action
  ↓
Saves to database
  ↓
Redirects back to: /Admin/Tailors/Verification
  ↓
Shows success message: "Tailor verified successfully"
```

**Result:**  
✅ TailorProfile.IsVerified = true  
✅ Tailor gets notification  
✅ Full access granted

---

#### 4.3b: Admin Rejects Tailor
```
Admin clicks "Reject"
  ↓
Enters reason for rejection
  ↓
POST to: /Admin/Tailors/{id}/Reject
  ↓
Creates notification for tailor:
  Title: "تم رفض طلب التحقق"
  Message: "عذراً، تم رفض طلب التحقق. السبب: {reason}"
  Type: "Warning"
  ↓
Logs admin action
  ↓
Saves to database
  ↓
Redirects back to: /Admin/Tailors/Verification
  ↓
Shows info message: "Tailor verification rejected"
```

**Result:**  
❌ TailorProfile.IsVerified remains false  
❌ Tailor gets rejection notification  
❌ Limited access continues

---

### Step 5: Tailor After Approval

#### 5.1: Tailor Logs In After Approval
```
Tailor logs in
  ↓
AuthService checks:
  - TailorProfile exists? ✅
  - IsVerified? ✅ YES (approved by admin)
  ↓
Login successful
  ↓
Redirects to: /Dashboards/Tailor
  ↓
Middleware checks:
  - TailorProfile exists? ✅
  - IsVerified? ✅ YES
  ↓
HttpContext.Items["PendingApproval"] = false
  ↓
Dashboard shows NO warning banner
  ↓
Full features enabled! 🎉
```

**Tailor can NOW:**  
✅ Full dashboard access  
✅ Receive orders  
✅ Accept payments  
✅ Manage services  
✅ Respond to RFQs  
✅ All tailor features

---

## 📊 Page/URL Summary

| Page Purpose | URL | Controller Method | View | User Role |
|--------------|-----|-------------------|------|-----------|
| **Tailor submits evidence** | `/Account/ProvideTailorEvidence` | `AccountController.ProvideTailorEvidence()` | `Account/ProvideTailorEvidence.cshtml` | Tailor (unauthenticated or authenticated incomplete) |
| **Admin views pending tailors** | `/Admin/Tailors/Verification` | `AdminDashboardController.TailorVerification()` | `AdminDashboard/TailorVerification.cshtml` | Admin |
| **Admin reviews individual tailor** | `/Admin/Tailors/{id}/Review` | `AdminDashboardController.ReviewTailor(id)` | `AdminDashboard/ReviewTailor.cshtml` | Admin |
| **Admin approves tailor** | `/Admin/Tailors/{id}/Approve` (POST) | `AdminDashboardController.ApproveTailor(id)` | N/A (redirect) | Admin |
| **Admin rejects tailor** | `/Admin/Tailors/{id}/Reject` (POST) | `AdminDashboardController.RejectTailor(id)` | N/A (redirect) | Admin |
| **Tailor dashboard (pending)** | `/Dashboards/Tailor` | `DashboardsController.Tailor()` | `Dashboards/Tailor.cshtml` | Tailor (IsVerified = false) |
| **Tailor dashboard (approved)** | `/Dashboards/Tailor` | `DashboardsController.Tailor()` | `Dashboards/Tailor.cshtml` | Tailor (IsVerified = true) |

---

## 🔐 Security Checks

### For Tailor Evidence Page (`/Account/ProvideTailorEvidence`)
- ✅ AllowAnonymous (can be accessed before login)
- ✅ Checks if authenticated user is a tailor
- ✅ Checks if TailorProfile already exists (prevent duplicates)
- ✅ Redirects to dashboard if already complete

### For Admin Verification Pages
- ✅ `[Authorize(Roles = "Admin")]` on controller
- ✅ Only admins can access
- ✅ Validates tailor ID exists
- ✅ Logs all admin actions

### For Tailor Dashboard
- ✅ `[Authorize(Roles = "Tailor")]` on action
- ✅ Checks TailorProfile exists
- ✅ Shows pending approval if not verified
- ✅ Redirects to evidence page if profile missing

---

## 🎯 State Transitions

### Tailor States
```
[Registered] 
  - User exists
- No TailorProfile
  - Cannot login
  ↓
[Evidence Submitted]
  - TailorProfile exists
  - IsVerified = false
  - Can login (limited)
  ↓
[Under Review] (same as Evidence Submitted)
  - Admin reviewing
  - Can login (limited)
  ↓
[Approved] ✅
  - IsVerified = true
  - Full access
OR
[Rejected] ❌
  - IsVerified = false
  - Notification sent
  - Can resubmit (future feature)
```

---

## 📝 Database Changes

### When Tailor Submits Evidence
```sql
-- Insert TailorProfile
INSERT INTO TailorProfiles (Id, UserId, ShopName, Address, City, Bio, ExperienceYears, IsVerified, CreatedAt)
VALUES (newid(), @UserId, @ShopName, @Address, @City, @Bio, @ExperienceYears, 0, GETDATE())

-- Update User
UPDATE Users
SET IsActive = 1, UpdatedAt = GETDATE()
WHERE Id = @UserId

-- Insert PortfolioImages
INSERT INTO PortfolioImages (PortfolioImageId, TailorId, ImageUrl, UploadedAt, IsDeleted)
VALUES (@ImageId, @TailorId, @ImageUrl, GETDATE(), 0)
```

### When Admin Approves
```sql
-- Update TailorProfile
UPDATE TailorProfiles
SET IsVerified = 1, UpdatedAt = GETDATE()
WHERE Id = @TailorId

-- Insert Notification
INSERT INTO Notifications (UserId, Title, Message, Type, SentAt)
VALUES (@UserId, 'تم التحقق من حسابك', '...', 'Success', GETDATE())
```

---

## 🧪 Testing Scenarios

### Test Case 1: Complete Happy Path ✅
1. Register as tailor
2. Complete evidence form
3. Submit application
4. Login (see pending approval)
5. Admin reviews and approves
6. Login again (full access)

### Test Case 2: Incomplete Evidence ❌
1. Register as tailor
2. Close evidence page without submitting
3. Try to login → BLOCKED by AuthService
4. Try to access features → Redirected by middleware

### Test Case 3: Admin Rejection ❌
1. Tailor submits evidence
2. Admin rejects with reason
3. Tailor sees rejection notification
4. Tailor still has limited access
5. (Future: Can resubmit)

### Test Case 4: Middleware Protection ✅
1. Approved tailor tries to access evidence page
2. Redirected to dashboard (already complete)

---

## 🚨 Important Notes

1. **Two Separate Pages:**
   - `/Account/ProvideTailorEvidence` = For **TAILORS** to submit documents
   - `/Admin/Tailors/Verification` = For **ADMINS** to review applications

2. **IsVerified Flag:**
   - `false` = Pending approval (limited access)
   - `true` = Approved by admin (full access)

3. **Cannot Skip:**
   - Tailors MUST submit evidence to login
   - Tailors MUST wait for admin approval for full access

4. **Admin Workflow:**
 - Admin navigates to verification page
   - Reviews each tailor individually
 - Approves or rejects with reason
   - System sends notification automatically

---

## 📞 Quick Reference

### For Tailors:
- **Evidence Submission:** `/Account/ProvideTailorEvidence`
- **Dashboard (pending):** `/Dashboards/Tailor` (with warning banner)
- **Dashboard (approved):** `/Dashboards/Tailor` (full features)

### For Admins:
- **Verification List:** `/Admin/Tailors/Verification`
- **Review Individual:** `/Admin/Tailors/{id}/Review`
- **Approve:** POST `/Admin/Tailors/{id}/Approve`
- **Reject:** POST `/Admin/Tailors/{id}/Reject`

---

**Summary:** The current implementation is correct. Tailors submit evidence on THEIR page, admins verify on ADMIN page. The fix I made ensures authenticated tailors see the evidence form instead of being redirected to Register. ✅
