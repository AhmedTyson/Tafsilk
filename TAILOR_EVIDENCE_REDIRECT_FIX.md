# Tailor Evidence Page Redirect Fix

## Overview
Fixed the login flow for tailors who haven't submitted their evidence yet. Instead of blocking their login with an error message, the system now signs them in and redirects them to the evidence submission page.

## Changes Made

### 1. AuthService.cs - Modified `ValidateUserAsync` Method
**Location:** `TafsilkPlatform.Web/Services/AuthService.cs` (Lines ~93-105)

**Change:**
```csharp
// BEFORE: Blocked login with error message
if (!hasTailorProfile)
{
    _logger.LogWarning("[AuthService] Login blocked - Tailor has not provided evidence: {Email}", email);
  return (false, 
        "يجب إكمال عملية التحقق وتقديم الأوراق الثبوتية قبل تسجيل الدخول. " +
  "هذه الخطوة إلزامية ولا يمكن تخطيها. يرجى التسجيل مرة أخرى وإكمال جميع الخطوات المطلوبة.",
        null);
}

// AFTER: Return special code to trigger redirect
if (!hasTailorProfile)
{
    _logger.LogWarning("[AuthService] Login attempt - Tailor has not provided evidence yet: {Email}", email);
    // Return success but with a special error code to trigger redirect
    return (false, "TAILOR_INCOMPLETE_PROFILE", user);
}
```

**Key Points:**
- Returns the `user` object instead of `null` when tailor profile is missing
- Uses special error code `"TAILOR_INCOMPLETE_PROFILE"` as a signal
- Controller can now detect this case and handle the redirect

### 2. AccountController.cs - Modified `Login` Method
**Location:** `TafsilkPlatform.Web/Controllers/AccountController.cs` (Lines ~132-170)

**Added Special Case Handling:**
```csharp
// Special case: Tailor without profile - redirect to evidence page
if (!success && error == "TAILOR_INCOMPLETE_PROFILE" && user != null)
{
    _logger.LogInformation("Redirecting tailor {UserId} to complete evidence submission", user.Id);
    
    // Sign in the tailor first so they can access the evidence page as authenticated user
  var tailorClaims = await _profileHelper.BuildUserClaimsAsync(user);
    var tailorIdentity = new ClaimsIdentity(tailorClaims, CookieAuthenticationDefaults.AuthenticationScheme);
    var tailorPrincipal = new ClaimsPrincipal(tailorIdentity);

    await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme, 
        tailorPrincipal, 
        new AuthenticationProperties { IsPersistent = rememberMe });
    
    TempData["WarningMessage"] = "يجب إكمال عملية التحقق وتقديم الأوراق الثبوتية قبل الوصول إلى ميزات الخياط. " +
        "هذه الخطوة إلزامية ولا يمكن تخطيها.";
    return RedirectToAction(nameof(ProvideTailorEvidence), new { incomplete = true });
}
```

**Key Points:**
- Detects the special `TAILOR_INCOMPLETE_PROFILE` error code
- Signs in the tailor user (creates authentication cookie)
- Shows a warning message explaining why they're being redirected
- Redirects to `ProvideTailorEvidence` page with `incomplete=true` flag

## User Flow

### Before Fix:
1. Tailor registers → Account created (inactive)
2. Tailor tries to login → ❌ **BLOCKED** with error message
3. User stuck - cannot proceed

### After Fix:
1. Tailor registers → Account created (inactive)
2. Tailor tries to login → ✅ **Signs in successfully**
3. System redirects to evidence submission page
4. Tailor can complete their profile
5. After submission → Account becomes active

## Benefits

1. **Better UX:** No dead-end errors, clear path forward
2. **Authenticated Access:** Tailor is signed in, so they can access the evidence page properly
3. **Middleware Compatibility:** Works seamlessly with `UserStatusMiddleware.cs` which also redirects incomplete tailors
4. **Clear Messaging:** Warning message explains why they're being redirected
5. **Single Flow:** Both login and middleware redirect to the same page

## Testing Checklist

- [ ] Tailor registers new account
- [ ] Tailor tries to login without submitting evidence
- [ ] System signs them in and redirects to evidence page
- [ ] Warning message is displayed
- [ ] Tailor can submit evidence on the page
- [ ] After submission, tailor can access dashboard
- [ ] Regular users (Customer/Corporate) login normally

## Related Files

- `TafsilkPlatform.Web/Services/AuthService.cs` - Login validation logic
- `TafsilkPlatform.Web/Controllers/AccountController.cs` - Login controller
- `TafsilkPlatform.Web/Middleware/UserStatusMiddleware.cs` - Redirect middleware
- `TafsilkPlatform.Web/Views/Account/ProvideTailorEvidence.cshtml` - Evidence form

## Notes

- The `incomplete=true` query parameter in the redirect is used by the evidence page to show appropriate messaging
- The tailor is signed in BEFORE redirect, so they have proper authentication context
- This fix ensures the login flow matches the middleware behavior (both redirect to same page)
