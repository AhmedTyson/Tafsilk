# 🔧 Account Controller Quick Fix Instructions

## ⚠️ Current Issue
The AccountController has duplicate method definitions that need to be cleaned up and new methods need to be added properly.

## 📋 Step-by-Step Fix Instructions

### Step 1: Remove Duplicate Methods

The following methods appear twice in the file and need one instance removed:

1. **VerifyEmail** (around line 1138 and line 1174) - Keep only ONE
2. **ResendVerificationEmail** GET (around line 1148 and line 1184) - Keep only ONE  
3. **ResendVerificationEmail** POST (around line 1157 and line 1193) - Keep only ONE
4. **CompleteTailorProfile** GET with [Authorize(Policy = "TailorPolicy")] (around line 1271) - This is different from the [Authorize] one, decide which to keep

### Step 2: Add Missing Methods

Add these new methods to the AccountController (recommended location: before the final closing brace):

```csharp
#region Settings

/// <summary>
/// User settings page (redirects to dashboard for now)
/// </summary>
[HttpGet]
public IActionResult Settings()
{
_logger.LogInformation("User {UserId} accessed Settings page", 
        User.FindFirstValue(ClaimTypes.NameIdentifier));
    var roleName = User.FindFirstValue(ClaimTypes.Role);
    return RedirectToRoleDashboard(roleName);
}

#endregion

#region Password Reset

/// <summary>
/// Forgot password page - request password reset email
/// </summary>
[HttpGet]
[AllowAnonymous]
public IActionResult ForgotPassword()
{
    return View();
}

/// <summary>
/// Send password reset email
/// </summary>
[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ForgotPassword(string email)
{
    if (string.IsNullOrWhiteSpace(email))
  {
        ModelState.AddModelError(nameof(email), "البريد الإلكتروني مطلوب");
      return View();
    }

    var user = await _unitOfWork.Users.GetByEmailAsync(email);
    
    // Security: Always show success message even if user doesn't exist
    if (user == null)
    {
        _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
        TempData["SuccessMessage"] = "إذا كان البريد الإلكتروني موجوداً في نظامنا، ستتلقى رسالة لإعادة تعيين كلمة المرور خلال بضع دقائق.";
   return View();
    }

    // Generate password reset token
    var resetToken = GeneratePasswordResetToken();
    user.PasswordResetToken = resetToken;
    user.PasswordResetTokenExpires = _dateTime.Now.AddHours(1);
    user.UpdatedAt = _dateTime.Now;

    await _unitOfWork.Users.UpdateAsync(user);
    await _unitOfWork.SaveChangesAsync();

    // TODO: Send email with reset link
    var resetLink = Url.Action(nameof(ResetPassword), "Account", 
        new { token = resetToken }, Request.Scheme);
    _logger.LogInformation("Password reset link generated for {Email}: {Link}", email, resetLink);

    TempData["SuccessMessage"] = "إذا كان البريد الإلكتروني موجوداً في نظامنا، ستتلقى رسالة لإعادة تعيين كلمة المرور خلال بضع دقائق.";
    return View();
}

/// <summary>
/// Reset password form with token
/// </summary>
[HttpGet]
[AllowAnonymous]
public IActionResult ResetPassword(string token)
{
    if (string.IsNullOrEmpty(token))
    {
 TempData["ErrorMessage"] = "رابط إعادة تعيين كلمة المرور غير صالح";
        return RedirectToAction(nameof(Login));
    }

    var model = new ResetPasswordViewModel { Token = token };
    return View(model);
}

/// <summary>
/// Process password reset
/// </summary>
[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
{
    if (!ModelState.IsValid)
    {
 return View(model);
    }

    var user = await _unitOfWork.Context.Set<User>()
        .FirstOrDefaultAsync(u => u.PasswordResetToken == model.Token);

    if (user == null)
    {
        ModelState.AddModelError(string.Empty, "رابط إعادة تعيين كلمة المرور غير صالح");
        return View(model);
    }

    // Check token expiry
    if (user.PasswordResetTokenExpires == null || user.PasswordResetTokenExpires < _dateTime.Now)
    {
        ModelState.AddModelError(string.Empty, "انتهت صلاحية رابط إعادة تعيين كلمة المرور. يرجى طلب رابط جديد.");
  return View(model);
    }

    // Update password
    user.PasswordHash = PasswordHasher.Hash(model.NewPassword);
  user.PasswordResetToken = null;
    user.PasswordResetTokenExpires = null;
    user.UpdatedAt = _dateTime.Now;

    await _unitOfWork.Users.UpdateAsync(user);
    await _unitOfWork.SaveChangesAsync();

    _logger.LogInformation("Password reset successful for user: {Email}", user.Email);

    TempData["RegisterSuccess"] = "تم إعادة تعيين كلمة المرور بنجاح! يمكنك الآن تسجيل الدخول بكلمة المرور الجديدة.";
    return RedirectToAction(nameof(Login));
}

/// <summary>
/// Generates a secure password reset token
/// </summary>
private string GeneratePasswordResetToken()
{
    return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
        .Replace("+", "")
  .Replace("/", "")
        .Replace("=", "")
        .Substring(0, 32);
}

#endregion
```

### Step 3: Mark Obsolete Methods

Find the `ProvideTailorEvidence` methods and add `[Obsolete]` attributes:

```csharp
/// <summary>
/// DEPRECATED: Use CompleteTailorProfile instead
/// </summary>
[HttpGet]
[AllowAnonymous]
[Obsolete("This method is deprecated. Tailor evidence is now handled through a different flow.")]
public async Task<IActionResult> ProvideTailorEvidence()
{
    // Keep existing implementation or redirect to new flow
}

[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
[Obsolete("This method is deprecated. Tailor evidence is now handled through a different flow.")]
public async Task<IActionResult> ProvideTailorEvidence(CompleteTailorProfileRequest model)
{
    // Keep existing implementation or redirect to new flow
}
```

### Step 4: Update User Model

Add to `TafsilkPlatform.Web/Models/User.cs` after `EmailVerifiedAt` property:

```csharp
// Password reset tokens
[MaxLength(64)]
public string? PasswordResetToken { get; set; }

[DataType(DataType.DateTime)]
public DateTime? PasswordResetTokenExpires { get; set; }
```

### Step 5: Update Login View

In `TafsilkPlatform.Web/Views/Account/Login.cshtml`, find the line:

```html
<a href="#" class="forgot-password">نسيت كلمة المرور؟</a>
```

Replace with:

```html
<a href="@Url.Action("ForgotPassword", "Account")" class="forgot-password">نسيت كلمة المرور؟</a>
```

### Step 6: Create Required Files

1. **ResetPasswordViewModel.cs** - Already created at `ViewModels/ResetPasswordViewModel.cs` ✅
2. **ForgotPassword.cshtml** - Already created at `Views/Account/ForgotPassword.cshtml` ✅
3. **ResetPassword.cshtml** - Already created at `Views/Account/ResetPassword.cshtml` ✅

### Step 7: Database Migration

Run these commands:

```bash
# Create migration
dotnet ef migrations add AddPasswordResetFieldsToUsers --project TafsilkPlatform.Web

# Apply migration
dotnet ef database update --project TafsilkPlatform.Web
```

Or run this SQL manually:

```sql
ALTER TABLE Users
ADD PasswordResetToken NVARCHAR(64) NULL,
    PasswordResetTokenExpires DATETIME2 NULL;

CREATE INDEX IX_Users_PasswordResetToken 
ON Users(PasswordResetToken) 
WHERE PasswordResetToken IS NOT NULL;
```

## ✅ Testing Checklist

After making these changes:

- [ ] Build solution - should have no errors
- [ ] Test Settings link from ChangePassword page
- [ ] Test Settings link from RequestRoleChange page
- [ ] Test "Forgot Password" link from Login page
- [ ] Test password reset flow end-to-end
- [ ] Verify database migration applied

## 📝 Summary of Changes

1. ✅ Remove duplicate methods (3 duplicates)
2. ✅ Add Settings action
3. ✅ Add password reset actions (4 new methods)
4. ✅ Add password reset token generator helper
5. ✅ Mark old methods as obsolete
6. ✅ Update User model with 2 new fields
7. ✅ Fix Login view forgot password link
8. ✅ Create 3 new views/viewmodels
9. ✅ Run database migration

## ⚠️ Important Notes

- The current file has some logic issues with duplicate tailor registration flows
- The ProvideTailorEvidence methods should eventually be removed or consolidated
- Consider refactoring the tailor registration flow in a future update
- Ensure all file upload paths are properly secured

## 🆘 If You Get Stuck

If compilation errors persist:

1. Check for any remaining duplicate methods
2. Ensure all `using` statements are present at top of file
3. Verify all referenced ViewModels exist
4. Run `dotnet clean` then `dotnet build`
5. Restart Visual Studio/IDE

---

**This is a manual fix guide. Apply changes carefully to avoid breaking existing functionality.**
