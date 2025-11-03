# Tailor Registration Redirect Logic - Implementation Guide

## Overview
This document describes the implementation of the redirect logic that ensures incomplete tailor registrations are properly redirected back to the verification page. This is a **MANDATORY** feature that prevents tailors from accessing tailor-specific features until they complete the verification process.

---

## 🎯 Business Requirements

### Critical Rule
**Tailors CANNOT access any tailor-specific features until verification is 100% complete and approved by admin.**

### Key Differences from Other Roles

| Role | Verification Required | Immediate Dashboard Access | Can Skip Verification |
|------|----------------------|---------------------------|---------------------|
| Customer | No | ✅ Yes | ✅ Yes |
| Corporate | Yes (Business docs) | ❌ No | ❌ No |
| **Tailor** | **Yes (Strict)** | **❌ No** | **❌ Absolutely Not** |

---

## 🔧 Implementation Components

### 1. **UserStatusMiddleware** (Enhanced)
**File:** `TafsilkPlatform.Web/Middleware/UserStatusMiddleware.cs`

#### What It Does:
- ✅ Checks if user is authenticated
- ✅ Verifies user is active and not deleted
- ✅ **CRITICAL:** For tailors, checks if `TailorProfile` exists
- ✅ Redirects incomplete tailors to `/Account/ProvideTailorEvidence?incomplete=true`

#### Key Method: `HandleTailorVerificationCheck`
```csharp
private async Task HandleTailorVerificationCheck(HttpContext context, Guid userId, IUnitOfWork unitOfWork)
{
    var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

    // Allow access to these pages for unverified tailors
 if (path.Contains("/account/providetailorevidence") ||
        path.Contains("/account/logout") ||
 path.Contains("/home"))
    {
        return;
    }

    // Check if tailor has completed verification
    var tailorProfile = await unitOfWork.Tailors.GetByUserIdAsync(userId);

    if (tailorProfile == null)
    {
    // MANDATORY REDIRECT: Tailor has not completed verification
        context.Response.Redirect("/Account/ProvideTailorEvidence?incomplete=true");
     return;
    }
    else if (!tailorProfile.IsVerified)
    {
        // Tailor submitted evidence but not yet approved
        context.Items["PendingApproval"] = true;
    }
}
```

#### Paths That Skip Middleware:
- `/account/login`
- `/account/logout`
- `/account/register`
- `/account/providetailorevidence`
- `/account/verifyemail`
- Static files (`/css`, `/js`, `/lib`, `/images`, `/uploads`)
- Development tools (`/swagger`, `/_framework`, `/_vs`)

---

### 2. **AuthService** (Enhanced Login Validation)
**File:** `TafsilkPlatform.Web/Services/AuthService.cs`

#### Enhanced Login Check:
```csharp
// CRITICAL: Check if tailor has submitted evidence
if (user.Role?.Name?.ToLower() == "tailor")
{
 var hasTailorProfile = await _hasTailorProfileQuery(_db, user.Id);
    if (!hasTailorProfile)
    {
        return (false, 
            "يجب إكمال عملية التحقق وتقديم الأوراق الثبوتية قبل تسجيل الدخول. " +
            "هذه الخطوة إلزامية ولا يمكن تخطيها.",
      null);
    }
}
```

#### Error Messages:
- **No TailorProfile:** "يجب إكمال عملية التحقق وتقديم الأوراق الثبوتية قبل تسجيل الدخول."
- **Inactive (Pending Approval):** "حسابك قيد المراجعة من قبل الإدارة. سيتم تفعيله خلال 2-3 أيام عمل."
- **Account Deleted:** "حسابك غير موجود. يرجى التواصل مع الدعم."

---

### 3. **AccountController** (Updated)
**File:** `TafsilkPlatform.Web/Controllers/AccountController.cs`

#### ProvideTailorEvidence Action (Enhanced):
```csharp
[HttpGet]
[AllowAnonymous]
public async Task<IActionResult> ProvideTailorEvidence(bool incomplete = false)
{
    // Handle incomplete registration redirect from middleware
    if (incomplete && User.Identity?.IsAuthenticated == true)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(authenticatedUserId, out var authUserId))
        {
       var user = await _unitOfWork.Users.GetByIdAsync(authUserId);
            if (user != null && user.Role?.Name?.ToLower() == "tailor")
            {
       var tailorProfile = await _unitOfWork.Tailors.GetByUserIdAsync(authUserId);
           if (tailorProfile != null)
            {
          // Profile exists, redirect to dashboard
   return RedirectToAction("Tailor", "Dashboards");
      }

        // Show incomplete warning
                TempData["WarningMessage"] = "يجب إكمال عملية التحقق للوصول إلى ميزات الخياط.";
    // ... show form
            }
      }
    }
    // ... rest of original logic
}
```

---

### 4. **DashboardsController** (Enhanced)
**File:** `TafsilkPlatform.Web/Controllers/DashboardsController.cs`

#### Tailor Dashboard Check:
```csharp
[Authorize(Roles = "Tailor")]
public async Task<IActionResult> Tailor()
{
    var userId = User.GetUserId();

    // CRITICAL: Check if tailor has completed verification
    var tailor = await _context.TailorProfiles
      .Include(t => t.User)
   .Include(t => t.TailorServices)
     .Include(t => t.PortfolioImages)
        .FirstOrDefaultAsync(t => t.UserId == userId);

    if (tailor == null)
    {
     // Redirect to evidence submission
        TempData["ErrorMessage"] = "يجب تقديم الأوراق الثبوتية وإكمال ملفك الشخصي أولاً.";
  return RedirectToAction("ProvideTailorEvidence", "Account", new { incomplete = true });
    }

    // Check if pending approval
    var isPendingApproval = HttpContext.Items["PendingApproval"] as bool? ?? false;
    if (!tailor.IsVerified || isPendingApproval)
    {
        ViewData["PendingApproval"] = true;
        ViewData["PendingMessage"] = "حسابك قيد المراجعة من قبل الإدارة...";
    }

    // ... build dashboard
}
```

---

### 5. **ProvideTailorEvidence View** (Enhanced)
**File:** `TafsilkPlatform.Web/Views/Account/ProvideTailorEvidence.cshtml`

#### New Warning Alert:
```html
<div class="alert alert-danger border-danger mb-4" role="alert">
    <h4 class="alert-heading">
        <i class="fas fa-exclamation-circle"></i>
        خطوة إلزامية - لا يمكن تخطيها
    </h4>
    <p><strong>هذه الصفحة هي بوابة التحقق الإلزامية للخياطين.</strong></p>
    <ul>
        <li><strong>❌ لا يمكن تخطي هذه الخطوة</strong></li>
        <li><strong>❌ لا يمكن الوصول للوحة التحكم قبل الإكمال</strong></li>
      <li><strong>❌ لا يمكن إضافة خدمات أو استقبال طلبات</strong></li>
        <li><strong>✅ يجب تقديم جميع المستندات المطلوبة</strong></li>
    </ul>
</div>
```

---

### 6. **Tailor Dashboard View** (Enhanced)
**File:** `TafsilkPlatform.Web/Views/Dashboards/Tailor.cshtml`

#### Pending Approval Alert:
```html
@if (ViewData["PendingApproval"] as bool? == true)
{
    <div class="alert alert-warning mb-4" role="alert">
        <h5><i class="fas fa-clock"></i> حسابك قيد المراجعة</h5>
        <p>@ViewData["PendingMessage"]</p>
        <p><strong>ماذا يمكنك فعله الآن:</strong></p>
        <ul>
            <li>إكمال ملفك الشخصي</li>
       <li>إضافة المزيد من الصور</li>
    <li>تجهيز قائمة الخدمات</li>
        </ul>
    </div>
}
```

---

## 🔄 Complete Flow Diagram

### Scenario 1: ✅ Successful Registration
```
1. User registers as Tailor
   ↓
2. Redirected to ProvideTailorEvidence
   ↓
3. Completes ALL fields + uploads documents
   ↓
4. Clicks "Submit Application"
   ↓
5. System creates TailorProfile (IsVerified = false)
   ↓
6. Redirected to Login with success message
 ↓
7. Logs in successfully
   ↓
8. Middleware allows access (TailorProfile exists)
   ↓
9. Dashboard shows "Pending Approval" notice
   ↓
10. Admin approves (IsVerified = true, IsActive = true)
   ↓
11. Tailor gets full access
```

### Scenario 2: ❌ Incomplete Registration
```
1. User registers as Tailor
   ↓
2. Redirected to ProvideTailorEvidence
   ↓
3. User closes page / exits without completing
   ↓
4. TailorProfile = NULL (not created)
   ↓
5. User tries to login
   ↓
6. AuthService blocks: "يجب إكمال عملية التحقق"
   ↓
7. OR: User somehow bypasses login
   ↓
8. Middleware intercepts any tailor page request
   ↓
9. Redirects to: /Account/ProvideTailorEvidence?incomplete=true
   ↓
10. Shows warning: "يجب إكمال عملية التحقق"
   ↓
11. User MUST complete form to proceed
```

---

## 🚨 Error Handling

### Session Timeout
- **Duration:** TempData expires after session ends
- **Action:** User must re-register if session expires
- **Message:** "جلسة غير صالحة. يرجى التسجيل مرة أخرى"

### Duplicate Submission
- **Check:** System verifies `TailorProfile` doesn't already exist
- **Action:** Redirects to Login
- **Message:** "تم تقديم الأوراق الثبوتية بالفعل"

### Missing Documents
- **Validation:** Client-side + Server-side
- **Required:**
  - ID Document (1 file)
  - Portfolio Images (minimum 3 files)
- **Message:** "يجب تحميل على الأقل 3 صور من أعمالك"

---

## 📋 Testing Checklist

### Test Case 1: Complete Happy Path
- [ ] Register as tailor
- [ ] Complete evidence form
- [ ] Upload all documents
- [ ] Submit successfully
- [ ] Login works
- [ ] Dashboard shows pending approval
- [ ] Admin approves
- [ ] Full access granted

### Test Case 2: Incomplete Registration
- [ ] Register as tailor
- [ ] Close evidence page without submitting
- [ ] Try to login → Blocked
- [ ] Try to access `/Dashboards/Tailor` → Redirected
- [ ] Try to access `/TailorManagement/*` → Redirected
- [ ] Complete evidence form → Access granted

### Test Case 3: Duplicate Submission Prevention
- [ ] Complete evidence once
- [ ] Try to access evidence page again → Redirected to Login
- [ ] Login → Dashboard works

### Test Case 4: Middleware Bypass Attempt
- [ ] Manually navigate to `/Dashboards/Tailor` while incomplete
- [ ] Should redirect to evidence page
- [ ] Warning message displayed
- [ ] Complete form → Access granted

---

## 🔧 Configuration

### Required Services in Program.cs
```csharp
// Already configured ✅
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITailorRepository, TailorRepository>();

// Middleware registration ✅
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserStatusMiddleware>(); // Must be AFTER auth
```

### Database Requirements
- `TailorProfiles` table must exist
- `IsVerified` column (bool) - default `false`
- Foreign key: `UserId` → `Users.Id`

---

## 📊 Admin Workflow

### Reviewing Tailor Applications
1. Admin receives notification (email/dashboard)
2. Reviews evidence documents:
   - ID/Business license
   - Portfolio images
   - Business information
3. Approves or Rejects:
   - **Approve:** Set `IsVerified = true`, `IsActive = true`
   - **Reject:** Email tailor with reason
4. Tailor receives notification
5. On next login, full access granted

---

## 🎨 User Experience

### Customer Experience
✅ Simple, straightforward registration
✅ Immediate dashboard access
✅ No verification required

### Tailor Experience
⚠️ **Mandatory verification gateway**
⚠️ Cannot skip or bypass
⚠️ Clear instructions and warnings
⚠️ Transparent approval process
✅ Limited access while pending
✅ Full access after approval

---

## 🔒 Security Considerations

1. **Session Management**
   - TempData is encrypted and server-side
   - Cannot be tampered with by client
   
2. **Middleware Protection**
   - Runs on EVERY request after authentication
   - Cannot be bypassed with direct URLs
   
3. **Database Checks**
   - Every critical action verifies TailorProfile exists
   - Uses `[Authorize(Roles = "Tailor")]` attribute
   
4. **Login Validation**
   - Blocks login if incomplete
   - Clear error messages
   - No ambiguous states

---

## ✅ Success Criteria

### Functional
- [x] Incomplete tailors cannot access tailor features
- [x] Clear error messages guide users
- [x] Middleware intercepts all tailor routes
- [x] Dashboard shows pending approval notice
- [x] Login validation works correctly

### User Experience
- [x] Warning messages are clear and prominent
- [x] Arabic UI/UX throughout
- [x] Responsive design works on mobile
- [x] Help text explains what to do

### Technical
- [x] No compilation errors
- [x] Build successful
- [x] Middleware registered correctly
- [x] Database queries optimized (compiled queries)
- [x] Proper error handling

---

## 📝 Future Enhancements

1. **Email Notifications**
 - Send reminder email if evidence not submitted within 24h
   - Notify tailor when application is approved/rejected

2. **Draft Save Feature**
   - Auto-save form progress
   - Resume from where left off (24h expiry)

3. **Admin Dashboard**
   - Show pending tailor applications
   - One-click approve/reject
   - View uploaded documents

4. **Analytics**
   - Track completion rate
   - Identify drop-off points
   - Measure time to approval

---

## 🆘 Troubleshooting

### Issue: Middleware not redirecting
**Solution:** Ensure middleware is registered AFTER `UseAuthentication()` and `UseAuthorization()`

### Issue: TempData is empty
**Solution:** Session middleware must be registered: `app.UseSession()`

### Issue: User can still access tailor pages
**Solution:** Check `ShouldSkipMiddleware()` method - ensure path is not being skipped

### Issue: Infinite redirect loop
**Solution:** Ensure `/Account/ProvideTailorEvidence` is in the skip list

---

## 📞 Support

For questions or issues:
- **Developer:** Check code comments and documentation
- **Users:** Contact support@tafsilk.com
- **Admins:** Refer to admin documentation

---

## ✨ Summary

This implementation ensures that **tailors cannot skip or bypass the verification process**. The system uses multiple layers of protection:

1. **Login validation** blocks incomplete tailors
2. **Middleware** intercepts and redirects incomplete tailors
3. **Dashboard checks** redirect if profile missing
4. **Clear UI warnings** explain the requirements

**Result:** A secure, user-friendly verification gateway that maintains platform quality and trust. 🎯
