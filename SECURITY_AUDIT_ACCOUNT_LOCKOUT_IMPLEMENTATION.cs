// ACCOUNT LOCKOUT IMPLEMENTATION

// 1. Add fields to User entity:
/*
public int FailedLoginAttempts { get; set; }
public DateTime? LockoutEnd { get; set; }
public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
*/

// 2. Update AuthService.ValidateUserAsync:
public async Task<(bool Succeeded, string? Error, User? User)> ValidateUserAsync(string email, string password)
{
    try
    {
_logger.LogInformation("[AuthService] Login attempt for email: {Email}", email);

    var user = await _unitOfWork.Users.GetByEmailAsync(email);
        if (user == null)
        {
     _logger.LogWarning("[AuthService] Login failed: User not found for email: {Email}", email);
       // ✅ Don't reveal if user exists or not
            return (false, "البريد الإلكتروني أو كلمة المرور غير صحيحة", null);
        }

        // ✅ CHECK IF ACCOUNT IS LOCKED OUT
if (user.IsLockedOut)
        {
 var remainingLockoutTime = (user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes;
       _logger.LogWarning("[AuthService] Login attempt for locked out user: {Email}. Lockout ends in {Minutes} minutes", 
     email, remainingLockoutTime);
   
   return (false, $"تم قفل حسابك مؤقتاً بسبب محاولات تسجيل دخول فاشلة. حاول مرة أخرى بعد {Math.Ceiling(remainingLockoutTime)} دقيقة.", null);
        }

        // Verify password
        if (!PasswordHasher.Verify(user.PasswordHash, password))
   {
     // ✅ INCREMENT FAILED LOGIN ATTEMPTS
            user.FailedLoginAttempts++;
  
            // ✅ LOCKOUT AFTER 5 FAILED ATTEMPTS
     if (user.FailedLoginAttempts >= 5)
        {
 user.LockoutEnd = DateTime.UtcNow.AddMinutes(15); // 15 minute lockout
    _logger.LogWarning("[AuthService] Account locked out due to failed login attempts: {Email}", email);
          }

          await _unitOfWork.Users.UpdateAsync(user);
    await _unitOfWork.SaveChangesAsync();

    _logger.LogWarning("[AuthService] Failed login attempt #{Attempts} for email: {Email}", 
           user.FailedLoginAttempts, email);

            // ✅ Don't reveal if password was wrong
       return (false, "البريد الإلكتروني أو كلمة المرور غير صحيحة", null);
        }

        // ✅ RESET FAILED ATTEMPTS ON SUCCESSFUL LOGIN
        if (user.FailedLoginAttempts > 0 || user.LockoutEnd.HasValue)
        {
     user.FailedLoginAttempts = 0;
    user.LockoutEnd = null;
   await _unitOfWork.Users.UpdateAsync(user);
          await _unitOfWork.SaveChangesAsync();
   _logger.LogInformation("[AuthService] Failed login attempts reset for user: {Email}", email);
      }

    // Check if user is active
      if (!user.IsActive)
 {
         _logger.LogWarning("[AuthService] Login attempt for inactive user: {Email}", email);
    return (false, "حسابك غير نشط. يرجى الاتصال بالدعم.", null);
        }

        // ... rest of validation logic

  _logger.LogInformation("[AuthService] Login successful for user: {Email}", email);
        return (true, null, user);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "[AuthService] Error validating user: {Email}", email);
        return (false, "حدث خطأ أثناء تسجيل الدخول. يرجى المحاولة مرة أخرى.", null);
    }
}

// 3. Create database migration:
/*
dotnet ef migrations add AddAccountLockout
dotnet ef database update
*/

// 4. Add admin functionality to unlock accounts:
public async Task<bool> UnlockUserAccountAsync(Guid userId)
{
    var user = await _unitOfWork.Users.GetByIdAsync(userId);
    if (user == null) return false;

 user.FailedLoginAttempts = 0;
    user.LockoutEnd = null;
    
 await _unitOfWork.Users.UpdateAsync(user);
    await _unitOfWork.SaveChangesAsync();
    
    _logger.LogInformation("[AuthService] Account unlocked by admin: {UserId}", userId);
    return true;
}
