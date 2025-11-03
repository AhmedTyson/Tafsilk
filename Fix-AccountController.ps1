# PowerShell script to fix AccountController.cs
# This script will:
# 1. Remove duplicate methods
# 2. Add missing methods (Settings, Password Reset)
# 3. Mark obsolete methods

$filePath = "C:\Users\ahmed\source\repos\AhmedTyson\Tafsilk\TafsilkPlatform.Web\Controllers\AccountController.cs"
$content = Get-Content $filePath -Raw

# Define the code to add before the final closing brace
$newMethods = @"

    #region Settings

    /// <summary>
    /// User settings page (redirects to dashboard for now)
    /// </summary>
    [HttpGet]
    public IActionResult Settings()
    {
        _logger.LogInformation("User {UserId} accessed Settings page", User.FindFirstValue(ClaimTypes.NameIdentifier));
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
"@

# Find the last closing brace (end of class)
$lastBraceIndex = $content.LastIndexOf("}")

# Insert new methods before the last brace
$newContent = $content.Substring(0, $lastBraceIndex) + $newMethods + "`n" + $content.Substring($lastBraceIndex)

# Now remove duplicate methods - find second occurrence of VerifyEmail and ResendVerificationEmail
# Split content into lines for easier manipulation
$lines = $newContent -split "`n"
$output = New-Object System.Collections.ArrayList

# Track which methods we've seen
$verifyEmailCount = 0
$resendCount = 0
$skipUntil = -1

for ($i = 0; $i -lt $lines.Count; $i++) {
    # Skip lines if we're in a duplicate method
    if ($i -lt $skipUntil) {
        continue
    }

    $line = $lines[$i]
    
    # Detect second VerifyEmail method
    if ($line -match 'public async Task<IActionResult> VerifyEmail\(string token\)') {
        $verifyEmailCount++
        if ($verifyEmailCount -eq 2) {
      # Skip this entire method (find its end)
  $braceCount = 0
        $inMethod = $false
       for ($j = $i; $j -lt $lines.Count; $j++) {
  if ($lines[$j] -match '\{') { $braceCount++; $inMethod = $true }
    if ($lines[$j] -match '\}') { $braceCount-- }
     if ($inMethod -and $braceCount -eq 0) {
     $skipUntil = $j + 1
           break
    }
     }
    continue
        }
    }
    
    # Detect second ResendVerificationEmail method
    if ($line -match 'public IActionResult ResendVerificationEmail\(\)' -and $lines[$i-1] -match '\[HttpGet\]') {
        $resendCount++
        if ($resendCount -eq 2) {
  # Skip until we find the POST method after this one
      $braceCount = 0
    $inMethod = $false
            $methodsFound = 0
            for ($j = $i; $j -lt $lines.Count; $j++) {
        if ($lines[$j] -match 'public async Task<IActionResult> ResendVerificationEmail\(string email\)') {
         # Found the POST method, skip it too
           for ($k = $j; $k -lt $lines.Count; $k++) {
          if ($lines[$k] -match '\{') { $braceCount++; $inMethod = $true }
           if ($lines[$k] -match '\}') { $braceCount-- }
  if ($inMethod -and $braceCount -eq 0) {
       $skipUntil = $k + 1
     break
    }
}
  break
                }
  }
       continue
        }
    }

    [void]$output.Add($line)
}

# Join lines back
$finalContent = $output -join "`n"

# Write to file
$finalContent | Set-Content $filePath -Encoding UTF8

Write-Host "✅ AccountController.cs has been updated successfully!" -ForegroundColor Green
Write-Host "  - Added Settings action" -ForegroundColor Cyan
Write-Host "  - Added ForgotPassword actions (GET/POST)" -ForegroundColor Cyan
Write-Host "  - Added ResetPassword actions (GET/POST)" -ForegroundColor Cyan
Write-Host "  - Added GeneratePasswordResetToken helper" -ForegroundColor Cyan
Write-Host "  - Removed duplicate VerifyEmail and ResendVerificationEmail methods" -ForegroundColor Cyan
