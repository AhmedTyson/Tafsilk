using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.ViewModels;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Security;
using System.Security.Claims;

namespace TafsilkPlatform.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AuthService> _logger;
        private readonly IDateTimeService _dateTime;
        private readonly IEmailService _emailService;

        public AuthService(
            AppDbContext db,
            ILogger<AuthService> logger,
            IDateTimeService dateTime,
            IEmailService emailService)
        {
            _db = db;
            _logger = logger;
            _dateTime = dateTime;
            _emailService = emailService;
        }

        public async Task<(bool Succeeded, string? Error, User? User)> RegisterAsync(RegisterRequest request)
        {
            // Use transaction to ensure atomicity
            using var transaction = await _db.Database.BeginTransactionAsync();
            
            try
            {
                _logger.LogInformation("[AuthService] Registration attempt for email: {Email}, role: {Role}",
                    request.Email, request.Role);

                // Validate email format
                if (!IsValidEmail(request.Email))
                {
                    return (false, "البريد الإلكتروني غير صالح", null);
                }

                // Validate email uniqueness
                if (await _db.Users.AnyAsync(u => u.Email == request.Email))
                {
                    _logger.LogWarning("[AuthService] Registration failed - Email already exists: {Email}", request.Email);
                    return (false, "البريد الإلكتروني مستخدم بالفعل", null);
                }

                // Validate phone number format and uniqueness if provided
                if (!string.IsNullOrEmpty(request.PhoneNumber))
                {
                    var cleanPhone = request.PhoneNumber.Replace(" ", "").Replace("-", "");
                    
                    // Check if phone already exists
                    if (await _db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber))
                    {
                        _logger.LogWarning("[AuthService] Registration failed - Phone number already exists: {Phone}", request.PhoneNumber);
                        return (false, "رقم الهاتف مستخدم بالفعل", null);
                    }
                }

                // Validate password strength
                var (isValidPassword, passwordError) = ValidatePassword(request.Password);
                if (!isValidPassword)
                {
                    return (false, passwordError, null);
                }

                // Create user
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    PasswordHash = PasswordHasher.Hash(request.Password),
                    CreatedAt = _dateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    EmailNotifications = true, // Default to true
                    SmsNotifications = false,
                    PromotionalNotifications = false,
                    RoleId = await EnsureRoleAsync(request.Role)
                };

                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync();

                _logger.LogInformation("[AuthService] User created successfully: {UserId}, Email: {Email}",
                    user.Id, user.Email);

                // Create role-specific profile
                await CreateProfileAsync(user.Id, request);

                // Generate email verification token
                var verificationToken = GenerateEmailVerificationToken();
                user.EmailVerificationToken = verificationToken;
                user.EmailVerificationTokenExpires = _dateTime.Now.AddHours(24);
                await _db.SaveChangesAsync();

                // Send verification email (don't fail registration if email fails)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendEmailVerificationAsync(
                            user.Email,
                            request.FullName ?? "مستخدم",
                            verificationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
                    }
                });

                // Commit transaction
                await transaction.CommitAsync();

                _logger.LogInformation("[AuthService] Registration completed successfully for user: {UserId}", user.Id);

                return (true, null, user);
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await transaction.RollbackAsync();
     
                _logger.LogError(ex, "[AuthService] Registration failed for email: {Email}", request.Email);
                return (false, "حدث خطأ أثناء التسجيل. يرجى المحاولة مرة أخرى.", null);
            }
        }

        /// <summary>
        /// Validates email format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates password strength
        /// </summary>
        private (bool IsValid, string? Error) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return (false, "كلمة المرور مطلوبة");
            }

            if (password.Length < 6)
            {
                return (false, "كلمة المرور يجب أن تكون 6 أحرف على الأقل");
            }

            // Optional: Check for common weak passwords
            var weakPasswords = new[] { "123456", "password", "qwerty", "111111", "123123" };
            if (weakPasswords.Contains(password.ToLower()))
            {
                return (false, "كلمة المرور ضعيفة جداً. يرجى اختيار كلمة مرور أقوى");
            }

            return (true, null);
        }

        public async Task<(bool Succeeded, string? Error, User? User)> ValidateUserAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("[AuthService] Login attempt for email: {Email}", email);

                // Load Role for role-based claims/redirects
                var user = await _db.Users
                    .AsNoTracking()
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user is null)
                {
                    _logger.LogWarning("[AuthService] Login failed - User not found: {Email}", email);
                    return (false, "البريد الإلكتروني أو كلمة المرور غير صحيحة", null);
                }

                if (!PasswordHasher.Verify(user.PasswordHash, password))
                {
                    _logger.LogWarning("[AuthService] Login failed - Invalid password for user: {Email}", email);
                    return (false, "البريد الإلكتروني أو كلمة المرور غير صحيحة", null);
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("[AuthService] Login failed - User is inactive: {Email}", email);
                    return (false, "حسابك غير نشط. يرجى التواصل مع الدعم.", null);
                }

                if (user.IsDeleted)
                {
                    _logger.LogWarning("[AuthService] Login failed - User is deleted: {Email}", email);
                    return (false, "حسابك غير موجود. يرجى التواصل مع الدعم.", null);
                }

                // Update last login timestamp
                await UpdateLastLoginAsync(user.Id);

                _logger.LogInformation("[AuthService] Login successful for user: {UserId}, Email: {Email}",
                    user.Id, user.Email);

                return (true, null, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Login error for email: {Email}", email);
                return (false, "حدث خطأ أثناء تسجيل الدخول. يرجى المحاولة مرة أخرى.", null);
            }
        }

        /// <summary>
        /// Creates role-specific profile after user registration.
        /// </summary>
        private async Task CreateProfileAsync(Guid userId, RegisterRequest request)
        {
            switch (request.Role)
            {
                case RegistrationRole.Customer:
                    var customer = new CustomerProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        FullName = request.FullName ?? string.Empty,
                        CreatedAt = _dateTime.Now
                    };
                    await _db.CustomerProfiles.AddAsync(customer);
                    _logger.LogInformation("[AuthService] Customer profile created for user: {UserId}", userId);
                    break;

                case RegistrationRole.Tailor:
                    var tailor = new TailorProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        FullName = request.FullName,
                        // Use provided values or defaults for required fields
                        ShopName = !string.IsNullOrEmpty(request.ShopName) ? request.ShopName : "متجر " + request.FullName,
                        Address = !string.IsNullOrEmpty(request.Address) ? request.Address : "لم يتم التحديد",
                        City = request.City ?? string.Empty,
                        CreatedAt = _dateTime.Now,
                        IsVerified = false
                    };
                    await _db.TailorProfiles.AddAsync(tailor);
                    _logger.LogInformation("[AuthService] Tailor profile created (unverified) for user: {UserId}", userId);
                    break;

                case RegistrationRole.Corporate:
                    var corporate = new CorporateAccount
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        CompanyName = request.CompanyName ?? string.Empty,
                        ContactPerson = request.ContactPerson ?? request.FullName ?? string.Empty,
                        CreatedAt = _dateTime.Now,
                        IsApproved = false
                    };
                    await _db.CorporateAccounts.AddAsync(corporate);
                    _logger.LogInformation("[AuthService] Corporate account created (unapproved) for user: {UserId}", userId);
                    break;
            }

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Ensures the specified role exists in the database.
        /// </summary>
        private async Task<Guid> EnsureRoleAsync(RegistrationRole desired)
        {
            var name = desired switch
            {
                RegistrationRole.Customer => "Customer",
                RegistrationRole.Tailor => "Tailor",
                RegistrationRole.Corporate => "Corporate",
                _ => "Customer"
            };

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == name);
            if (role is not null)
            {
                _logger.LogDebug("[AuthService] Role already exists: {RoleName}", name);
                return role.Id;
            }

            role = new Role
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = name switch
                {
                    "Customer" => "عميل - يمكنه طلب الخدمات من الخياطين",
                    "Tailor" => "خياط - يقدم خدمات الخياطة",
                    "Corporate" => "شركة - حساب مؤسسي للطلبات الجماعية",
                    _ => null
                },
                CreatedAt = _dateTime.Now
            };

            await _db.Roles.AddAsync(role);
            await _db.SaveChangesAsync();

            _logger.LogInformation("[AuthService] New role created: {RoleName} ({RoleId})", name, role.Id);

            return role.Id;
        }

        /// <summary>
        /// Updates the last login timestamp for the user.
        /// </summary>
        private async Task UpdateLastLoginAsync(Guid userId)
        {
            try
            {
                var user = await _db.Users.FindAsync(userId);
                if (user != null)
                {
                    user.LastLoginAt = _dateTime.Now;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Don't fail login if this fails
                _logger.LogWarning(ex, "[AuthService] Failed to update last login for user: {UserId}", userId);
            }
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            try
            {
                return await _db.Users
                    .Include(u => u.Role)
                    .Include(u => u.CustomerProfile)
                    .Include(u => u.TailorProfile)
                    .Include(u => u.CorporateAccount)
                    .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error getting user by ID: {UserId}", userId);
                return null;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _db.Users
                    .Include(u => u.Role)
                    .Include(u => u.CustomerProfile)
                    .Include(u => u.TailorProfile)
                    .Include(u => u.CorporateAccount)
                    .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error getting user by email: {Email}", email);
                return null;
            }
        }

        public async Task<(bool Succeeded, string? Error)> ChangePasswordAsync(
            Guid userId,
            string currentPassword,
            string newPassword)
        {
            try
            {
                var user = await _db.Users.FindAsync(userId);
                if (user == null)
                {
                    return (false, "المستخدم غير موجود");
                }

                if (!PasswordHasher.Verify(user.PasswordHash, currentPassword))
                {
                    return (false, "كلمة المرور الحالية غير صحيحة");
                }

                user.PasswordHash = PasswordHasher.Hash(newPassword);
                user.UpdatedAt = _dateTime.Now;

                await _db.SaveChangesAsync();

                _logger.LogInformation("[AuthService] Password changed successfully for user: {UserId}", userId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error changing password for user: {UserId}", userId);
                return (false, "حدث خطأ أثناء تغيير كلمة المرور");
            }
        }

        public async Task<bool> IsInRoleAsync(Guid userId, string roleName)
        {
            try
            {
                var user = await _db.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                return user?.Role?.Name?.Equals(roleName, StringComparison.OrdinalIgnoreCase) ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error checking role for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<List<Claim>> GetUserClaimsAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("UserId", user.Id.ToString())
            };

            // Add role claim
            if (user.Role != null && !string.IsNullOrEmpty(user.Role.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
            }

            // Add full name from profile
            string fullName = user.Email ?? "مستخدم";

            try
            {
                switch (user.Role?.Name?.ToLower())
                {
                    case "customer":
                        var customer = await _db.CustomerProfiles
                            .FirstOrDefaultAsync(c => c.UserId == user.Id);
                        if (customer != null && !string.IsNullOrEmpty(customer.FullName))
                        {
                            fullName = customer.FullName;
                        }
                        break;

                    case "tailor":
                        var tailor = await _db.TailorProfiles
                            .FirstOrDefaultAsync(t => t.UserId == user.Id);
                        if (tailor != null && !string.IsNullOrEmpty(tailor.FullName))
                        {
                            fullName = tailor.FullName;
                        }
                        // Add verification status
                        if (tailor != null)
                        {
                            claims.Add(new Claim("IsVerified", tailor.IsVerified.ToString()));
                        }
                        break;

                    case "corporate":
                        var corporate = await _db.CorporateAccounts
                            .FirstOrDefaultAsync(c => c.UserId == user.Id);
                        if (corporate != null)
                        {
                            fullName = corporate.ContactPerson ?? corporate.CompanyName ?? fullName;
                            claims.Add(new Claim("CompanyName", corporate.CompanyName ?? string.Empty));
                            claims.Add(new Claim("IsApproved", corporate.IsApproved.ToString()));
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AuthService] Error loading profile for claims: {UserId}", user.Id);
            }

            claims.Add(new Claim(ClaimTypes.Name, fullName));
            claims.Add(new Claim("FullName", fullName));

            return claims;
        }

        public async Task<(bool Succeeded, string? Error)> SetUserActiveStatusAsync(Guid userId, bool isActive)
        {
            try
            {
                var user = await _db.Users.FindAsync(userId);
                if (user == null)
                {
                    return (false, "المستخدم غير موجود");
                }

                user.IsActive = isActive;
                user.UpdatedAt = _dateTime.Now;

                await _db.SaveChangesAsync();

                _logger.LogInformation("[AuthService] User active status changed: {UserId}, IsActive: {IsActive}",
                    userId, isActive);

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error setting user active status: {UserId}", userId);
                return (false, "حدث خطأ أثناء تحديث حالة المستخدم");
            }
        }

        public async Task<(bool Succeeded, string? Error)> VerifyTailorAsync(Guid tailorId, bool isVerified)
        {
            try
            {
                var tailor = await _db.TailorProfiles.FindAsync(tailorId);
                if (tailor == null)
                {
                    return (false, "الخياط غير موجود");
                }

                tailor.IsVerified = isVerified;
                tailor.UpdatedAt = _dateTime.Now;

                await _db.SaveChangesAsync();

                _logger.LogInformation("[AuthService] Tailor verification status changed: {TailorId}, IsVerified: {IsVerified}",
                    tailorId, isVerified);

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error verifying tailor: {TailorId}", tailorId);
                return (false, "حدث خطأ أثناء تحديث حالة التحقق");
            }
        }

        public async Task<(bool Succeeded, string? Error)> ApproveCorporateAsync(Guid corporateId, bool isApproved)
        {
            try
            {
                var corporate = await _db.CorporateAccounts.FindAsync(corporateId);
                if (corporate == null)
                {
                    return (false, "الحساب المؤسسي غير موجود");
                }

                corporate.IsApproved = isApproved;
                corporate.UpdatedAt = _dateTime.Now;

                await _db.SaveChangesAsync();

                _logger.LogInformation("[AuthService] Corporate approval status changed: {CorporateId}, IsApproved: {IsApproved}",
                    corporateId, isApproved);

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error approving corporate: {CorporateId}", corporateId);
                return (false, "حدث خطأ أثناء تحديث حالة الموافقة");
            }
        }

        /// <summary>
        /// Generates a secure email verification token
        /// </summary>
        private string GenerateEmailVerificationToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "")
                .Substring(0, 32);
        }

        public async Task<(bool Succeeded, string? Error)> VerifyEmailAsync(string token)
        {
            try
            {
                var user = await _db.Users
     .FirstOrDefaultAsync(u => u.EmailVerificationToken == token 
      && !u.IsDeleted);

            if (user == null)
   {
   _logger.LogWarning("[AuthService] Email verification failed - Invalid token");
           return (false, "رابط التحقق غير صالح");
      }

         if (user.EmailVerified)
   {
          _logger.LogInformation("[AuthService] Email already verified for user: {UserId}", user.Id);
    return (true, null); // Already verified, return success
       }

if (user.EmailVerificationTokenExpires < _dateTime.Now)
       {
      _logger.LogWarning("[AuthService] Email verification failed - Token expired for user: {UserId}", user.Id);
 return (false, "انتهت صلاحية رابط التحقق. يرجى طلب رابط جديد");
      }

         user.EmailVerified = true;
      user.EmailVerifiedAt = _dateTime.Now;
         user.EmailVerificationToken = null;
user.EmailVerificationTokenExpires = null;
 user.UpdatedAt = _dateTime.Now;

            await _db.SaveChangesAsync();

      _logger.LogInformation("[AuthService] Email verified successfully for user: {UserId}", user.Id);

            // Send welcome email (don't fail if this fails)
     _ = Task.Run(async () =>
  {
   try
        {
        var role = await _db.Roles.FindAsync(user.RoleId);
            var fullName = await GetUserFullNameAsync(user.Id);
        await _emailService.SendWelcomeEmailAsync(
     user.Email,
   fullName,
      role?.Name ?? "Customer");
        }
    catch (Exception ex)
     {
     _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
 }
         });

        return (true, null);
        }
        catch (Exception ex)
  {
       _logger.LogError(ex, "[AuthService] Error verifying email for token: {Token}", token);
            return (false, "حدث خطأ أثناء تأكيد البريد الإلكتروني");
        }
    }

    public async Task<(bool Succeeded, string? Error)> ResendVerificationEmailAsync(string email)
    {
        try
        {
        var user = await _db.Users
      .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

            if (user == null)
        {
           return (false, "المستخدم غير موجود");
       }

    if (user.EmailVerified)
     {
          return (false, "البريد الإلكتروني مؤكد بالفعل");
         }

          // Generate new token
       var verificationToken = GenerateEmailVerificationToken();
            user.EmailVerificationToken = verificationToken;
   user.EmailVerificationTokenExpires = _dateTime.Now.AddHours(24);
         user.UpdatedAt = _dateTime.Now;

       await _db.SaveChangesAsync();

       var fullName = await GetUserFullNameAsync(user.Id);
            await _emailService.SendEmailVerificationAsync(
          user.Email,
    fullName,
      verificationToken);

       _logger.LogInformation("[AuthService] Verification email resent to user: {UserId}", user.Id);

       return (true, null);
        }
      catch (Exception ex)
        {
            _logger.LogError(ex, "[AuthService] Error resending verification email for: {Email}", email);
    return (false, "حدث خطأ أثناء إعادة إرسال رسالة التحقق");
        }
    }

    private async Task<string> GetUserFullNameAsync(Guid userId)
    {
        try
   {
   var user = await _db.Users
     .Include(u => u.Role)
                .Include(u => u.CustomerProfile)
         .Include(u => u.TailorProfile)
   .Include(u => u.CorporateAccount)
        .FirstOrDefaultAsync(u => u.Id == userId);

         if (user == null) return "مستخدم";

         return user.Role?.Name?.ToLower() switch
            {
  "customer" => user.CustomerProfile?.FullName ?? user.Email ?? "مستخدم",
       "tailor" => user.TailorProfile?.FullName ?? user.Email ?? "مستخدم",
      "corporate" => user.CorporateAccount?.ContactPerson ?? user.CorporateAccount?.CompanyName ?? user.Email ?? "مستخدم",
_ => user.Email ?? "مستخدم"
            };
        }
    catch
        {
  return "مستخدم";
     }
    }
}
    }
