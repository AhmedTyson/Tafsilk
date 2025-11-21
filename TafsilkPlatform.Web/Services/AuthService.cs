using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Security;
using TafsilkPlatform.Web.ViewModels;

namespace TafsilkPlatform.Web.Services
{
    /// <summary>
    /// Handles user authentication logic
    /// - Registration
    /// - Login validation
    /// - Email verification
    /// - Password management
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AuthService> _logger;
        private readonly IDateTimeService _dateTime;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        // Weak passwords to reject during registration
        private static readonly string[] WeakPasswords = { "123456", "password", "qwerty", "111111", "123123" };

        // Compiled query for login validation - only loads what we need
        private static readonly Func<AppDbContext, string, Task<User?>> _getUserForLoginQuery =
       EF.CompileAsyncQuery((AppDbContext db, string email) =>
 db.Users
     .AsNoTracking()
              .Include(u => u.Role)
                    .FirstOrDefault(u => u.Email == email));

        // Compiled query for checking tailor profile existence
        private static readonly Func<AppDbContext, Guid, Task<bool>> _hasTailorProfileQuery =
            EF.CompileAsyncQuery((AppDbContext db, Guid userId) =>
              db.TailorProfiles.Any(t => t.UserId == userId));

        public AuthService(
         AppDbContext db,
 ILogger<AuthService> logger,
   IDateTimeService dateTime,
   IEmailService emailService,
    IMemoryCache cache,
    IServiceScopeFactory serviceScopeFactory)
        {
            _db = db;
            _logger = logger;
            _dateTime = dateTime;
            _emailService = emailService;
            _cache = cache;
            _serviceScopeFactory = serviceScopeFactory;
        }

        #region Registration

        /// <summary>
        /// Registers a new user with role-specific profile
        /// For Tailors: Profile created AFTER evidence submission (not here)
        /// For Customers/Corporates: Profile created immediately
        /// </summary>
        public async Task<(bool Succeeded, string? Error, User? User)> RegisterAsync(RegisterRequest request)
        {
            // No manual transaction needed - EF Core's SaveChangesAsync handles it
            try
            {
                _logger.LogInformation("[AuthService] Registration attempt: {Email}, Role: {Role}",
                          request.Email, request.Role);

                // Validate user input
                var validationError = ValidateRegistrationRequest(request);
                if (validationError != null)
                {
                    return (false, validationError, null);
                }

                // Check if email or phone already exists
                if (await IsEmailTakenAsync(request.Email))
                {
                    _logger.LogWarning("[AuthService] Registration failed - Email already exists: {Email}", request.Email);
                    return (false, "البريد الإلكتروني مستخدم بالفعل", null);
                }

                if (!string.IsNullOrEmpty(request.PhoneNumber) && await IsPhoneTakenAsync(request.PhoneNumber))
                {
                    _logger.LogWarning("[AuthService] Registration failed - Phone already exists: {Phone}", request.PhoneNumber);
                    return (false, "رقم الهاتف مستخدم بالفعل", null);
                }

                // Create user account
                var user = CreateUserEntity(request);
                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync();

                _logger.LogInformation("[AuthService] User created: {UserId}, Email: {Email}, Role: {Role}, IsActive: {IsActive}",
                        user.Id, user.Email, request.Role, user.IsActive);

                // Create role-specific profile (except Tailors)
                if (request.Role != RegistrationRole.Tailor)
                {
                    await CreateProfileAsync(user.Id, request);

                    // ✅ UPDATED: Auto-verify customers - they can login immediately
                    // REMOVED: Corporate email verification
                    if (request.Role == RegistrationRole.Customer)
                    {
                        // Auto-verify customers - they can login immediately
                        user.EmailVerified = true;
                        user.EmailVerifiedAt = _dateTime.Now;
                        await _db.SaveChangesAsync();

                        _logger.LogInformation("[AuthService] Customer email auto-verified: {UserId}", user.Id);

                        // Send welcome email in background (optional)
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _emailService.SendWelcomeEmailAsync(user.Email, request.FullName ?? "عميل", "Customer");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
                            }
                        });
                    }
                }
                else
                {
                    _logger.LogInformation("[AuthService] Tailor profile creation deferred - awaiting evidence: {UserId}", user.Id);
                }

                // SaveChangesAsync already committed everything atomically
                _logger.LogInformation("[AuthService] Registration completed successfully: {UserId}", user.Id);

                return (true, null, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Registration failed for email: {Email}", request.Email);
                return (false, "حدث خطأ أثناء التسجيل. يرجى المحاولة مرة أخرى.", null);
            }
        }
        // Called by: [POST] /Account/Login, /ApiAuth/Login
        public async Task<(bool Succeeded, string? Error, User? User)> ValidateUserAsync(string email, string password)
        {

            try

            {

                _logger.LogInformation("[AuthService] Login attempt for: {Email}", email);



                // Get user with role information using compiled query

                var user = await _getUserForLoginQuery(_db, email);



                if (user == null)

                {

                    _logger.LogWarning("[AuthService] Login failed - User not found: {Email}", email);

                    return (false, "البريد الإلكتروني أو كلمة المرور غير صحيحة", null);

                }



                // Verify password

                if (!PasswordHasher.Verify(user.PasswordHash, password))

                {

                    _logger.LogWarning("[AuthService] Login failed - Invalid password: {Email}", email);

                    return (false, "البريد الإلكتروني أو كلمة المرور غير صحيحة", null);

                }



                // CRITICAL: Check if tailor has submitted evidence - use compiled query

                if (user.Role?.Name?.ToLower() == "tailor")

                {

                    var hasTailorProfile = await _hasTailorProfileQuery(_db, user.Id);

                    if (!hasTailorProfile)

                    {

                        _logger.LogWarning("[AuthService] Login attempt - Tailor has not provided evidence yet: {Email}", email);

                        // Always allow login to complete profile if tailor profile is missing

                        _logger.LogInformation("[AuthService] Redirecting new tailor to evidence submission: {Email}", email);

                        return (false, "TAILOR_INCOMPLETE_PROFILE", user);

                    }

                }



                // Check account status

                if (!user.IsActive)

                {

                    _logger.LogWarning("[AuthService] Login failed - User is inactive: {Email}", email);



                    // ✅ IMPROVED: More specific messages based on role and state

                    string message;



                    if (user.Role?.Name?.ToLower() == "tailor")

                    {

                        var hasTailorProfile = await _hasTailorProfileQuery(_db, user.Id);



                        if (!hasTailorProfile)

                        {

                            // Evidence not submitted yet (should not reach here anymore)

                            message = "يجب تقديم الأوراق الثبوتية أولاً لإكمال التسجيل. يرجى إكمال التسجيل عبر رابط التسجيل.";

                            _logger.LogInformation("[AuthService] Tailor has not submitted evidence yet: {Email}", email);

                        }

                        else

                        {

                            // Evidence submitted, waiting for admin approval or banned

                            message = "حسابك قيد المراجعة من قبل الإدارة أو تم حظرك. سيتم تفعيله خلال24-48 ساعة عمل أو يرجى التواصل مع الدعم.";

                            _logger.LogInformation("[AuthService] Tailor awaiting admin approval or banned: {Email}", email);

                        }

                    }

                    else

                    {

                        message = "حسابك غير نشط. يرجى التواصل مع الدعم.";

                    }



                    return (false, message, null);

                }



                if (user.IsDeleted)

                {

                    _logger.LogWarning("[AuthService] Login failed - User is deleted: {Email}", email);

                    return (false, "حسابك غير موجود. يرجى التواصل مع الدعم.", null);

                }



                // Update last login timestamp (don't fail login if this fails)
                // ✅ FIXED: Use IServiceScopeFactory to create new scope for background task
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _serviceScopeFactory.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        await db.Users
                            .Where(u => u.Id == user.Id)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(u => u.LastLoginAt, _dateTime.Now));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[AuthService] Failed to update last login: {UserId}", user.Id);
                    }
                });



                _logger.LogInformation("[AuthService] Login successful: {UserId}, Email: {Email}", user.Id, user.Email);

                return (true, null, user);

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "[AuthService] Login error for: {Email}", email);

                return (false, "حدث خطأ أثناء تسجيل الدخول. يرجى المحاولة مرة أخرى.", null);

            }

        }
        #endregion
        #region Email Verification

        /// <summary>
        /// </summary>
        public async Task<(bool Succeeded, string? Error)> VerifyEmailAsync(string token)
        {
            try
            {
                var user = await _db.Users
                              .FirstOrDefaultAsync(u => u.EmailVerificationToken == token && !u.IsDeleted);

                if (user == null)
                {
                    _logger.LogWarning("[AuthService] Email verification failed - Invalid token");
                    return (false, "رابط التحقق غير صالح");
                }

                if (user.EmailVerified)
                {
                    _logger.LogInformation("[AuthService] Email already verified: {UserId}", user.Id);
                    return (true, null); // Already verified
                }

                if (user.EmailVerificationTokenExpires < _dateTime.Now)
                {
                    _logger.LogWarning("[AuthService] Email verification failed - Token expired: {UserId}", user.Id);
                    return (false, "انتهت صلاحية رابط التحقق. يرجى طلب رابط جديد");
                }

                // Mark email as verified
                user.EmailVerified = true;
                user.EmailVerifiedAt = _dateTime.Now;
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpires = null;
                user.UpdatedAt = _dateTime.Now;

                await _db.SaveChangesAsync();

                _logger.LogInformation("[AuthService] Email verified successfully: {UserId}", user.Id);

                // Send welcome email in background
                _ = Task.Run(async () =>
       {
           try
           {
               var role = await _db.Roles.FindAsync(user.RoleId);
               var fullName = await GetUserFullNameAsync(user.Id);
               await _emailService.SendWelcomeEmailAsync(user.Email, fullName, role?.Name ?? "Customer");
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

        /// <summary>
        /// Resends email verification link
        /// </summary>
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

                // Generate new verification token
                var verificationToken = GenerateEmailVerificationToken();
                user.EmailVerificationToken = verificationToken;
                user.EmailVerificationTokenExpires = _dateTime.Now.AddHours(24);
                user.UpdatedAt = _dateTime.Now;

                await _db.SaveChangesAsync();

                var fullName = await GetUserFullNameAsync(user.Id);
                await _emailService.SendEmailVerificationAsync(user.Email, fullName, verificationToken);

                _logger.LogInformation("[AuthService] Verification email resent: {UserId}", user.Id);

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error resending verification email: {Email}", email);
                return (false, "حدث خطأ أثناء إعادة إرسال رسالة التحقق");
            }
        }

        #endregion

        #region Password Management

        /// <summary>
        /// Changes user password after verifying current password
        /// </summary>
        public async Task<(bool Succeeded, string? Error)> ChangePasswordAsync(
            Guid userId, string currentPassword, string newPassword)
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

                _logger.LogInformation("[AuthService] Password changed successfully: {UserId}", userId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error changing password: {UserId}", userId);
                return (false, "حدث خطأ أثناء تغيير كلمة المرور");
            }
        }

        #endregion

        #region User Queries

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            try
            {
                // Use AsSplitQuery to avoid cartesian explosion
                return await _db.Users
                    .AsNoTracking()
                  .AsSplitQuery()
                         .Include(u => u.Role)
                      .Include(u => u.CustomerProfile)
                  .Include(u => u.TailorProfile)
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
                // Use AsSplitQuery to avoid cartesian explosion
                return await _db.Users
                  .AsNoTracking()
             .AsSplitQuery()
            .Include(u => u.Role)
          .Include(u => u.CustomerProfile)
             .Include(u => u.TailorProfile)
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error getting user by email: {Email}", email);
                return null;
            }
        }

        public async Task<bool> IsInRoleAsync(Guid userId, string roleName)
        {
            try
            {
                // Optimized: Only select the role name, don't load the entire user
                var userRole = await _db.Users
             .AsNoTracking()
            .Where(u => u.Id == userId)
                   .Select(u => u.Role.Name)
               .FirstOrDefaultAsync();

                return userRole?.Equals(roleName, StringComparison.OrdinalIgnoreCase) ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error checking role: {UserId}", userId);
                return false;
            }
        }

        #endregion

        #region Claims Building

        /// <summary>
        /// Builds authentication claims for a user
        /// Note: Consider using UserProfileHelper for consistency
        /// </summary>
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

            // ✅ FIXED: Get full name from already-loaded navigation properties
            // This avoids querying the database again and prevents concurrency issues
            string fullName = GetFullNameFromUser(user);
            claims.Add(new Claim(ClaimTypes.Name, fullName));
            claims.Add(new Claim("FullName", fullName));

            // ✅ FIXED: Add role-specific claims from already-loaded data
            AddRoleSpecificClaimsFromUser(claims, user);

            return await Task.FromResult(claims);
        }

        /// <summary>
        /// Gets full name from already-loaded user navigation properties
        /// No database query - uses in-memory data
        /// </summary>
        private string GetFullNameFromUser(User user)
        {
            switch (user.Role?.Name?.ToLower())
            {
                case "customer":
                    return user.CustomerProfile?.FullName ?? user.Email ?? "مستخدم";

                case "tailor":
                    return user.TailorProfile?.FullName ?? user.Email ?? "مستخدم";


                default:
                    return user.Email ?? "مستخدم";
            }
        }
        /// <summary>
        /// Adds role-specific claims from already-loaded user data
        /// No database query - uses in-memory data
        /// </summary>
        private void AddRoleSpecificClaimsFromUser(List<Claim> claims, User user)
        {
            try
            {
                switch (user.Role?.Name?.ToLower())
                {
                    case "tailor":
                        if (user.TailorProfile != null)
                        {
                            claims.Add(new Claim("IsVerified", user.TailorProfile.IsVerified.ToString()));
                        }
                        break;

                        // REMOVED: Corporate case
                        // case "corporate":
                        //  if (user.CorporateAccount != null)
                        // {
                        //   claims.Add(new Claim("CompanyName", user.CorporateAccount.CompanyName ?? string.Empty));
                        //     claims.Add(new Claim("IsApproved", user.CorporateAccount.IsApproved.ToString()));
                        //   }
                        //   break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AuthService] Error adding role-specific claims: {UserId}", user.Id);
            }
        }

        #endregion

        #region Admin Operations

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

                if (isVerified)
                {
                    tailor.VerifiedAt = _dateTime.Now;
                }

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

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Validates registration request fields
        /// </summary>
        private string? ValidateRegistrationRequest(RegisterRequest request)
        {
            // Validate email format
            if (!IsValidEmail(request.Email))
            {
                return "البريد الإلكتروني غير صالح";
            }

            // Validate password strength
            var (isValid, error) = ValidatePassword(request.Password);
            if (!isValid)
            {
                return error;
            }

            return null;
        }

        private async Task<bool> IsEmailTakenAsync(string email)
        {
            return await _db.Users.AnyAsync(u => u.Email == email);
        }

        private async Task<bool> IsPhoneTakenAsync(string phoneNumber)
        {
            return await _db.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
        }

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

            // Check for weak passwords
            if (WeakPasswords.Contains(password.ToLower()))
            {
                return (false, "كلمة المرور ضعيفة جداً. يرجى اختيار كلمة مرور أقوى");
            }

            return (true, null);
        }

        private User CreateUserEntity(RegisterRequest request)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
     PhoneNumber = request.PhoneNumber,
     PasswordHash = PasswordHasher.Hash(request.Password),
    CreatedAt = _dateTime.Now,
                // Tailors: inactive until evidence provided
                IsActive = request.Role == RegistrationRole.Tailor ? false : true,
                IsDeleted = false,
                // Notification preferences removed - system simplified
 EmailVerified = false,
                RoleId = EnsureRoleAsync(request.Role).Result
            };
        }

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
                    _logger.LogInformation("[AuthService] Customer profile created: {UserId}", userId);
                    break;

                case RegistrationRole.Tailor:
                    // DO NOT CREATE - must provide evidence first
                    _logger.LogInformation("[AuthService] Tailor profile creation deferred: {UserId}", userId);
                    break;
            }

            await _db.SaveChangesAsync();
        }

        private async Task SendEmailVerificationAsync(User user, string? fullName)
        {
            var verificationToken = GenerateEmailVerificationToken();
            user.EmailVerificationToken = verificationToken;
            user.EmailVerificationTokenExpires = _dateTime.Now.AddHours(24);
            await _db.SaveChangesAsync();

            // Send email in background (don't fail registration if email fails)
            _ = Task.Run(async () =>
                   {
                       try
                       {
                           await _emailService.SendEmailVerificationAsync(
                   user.Email,
                          fullName ?? "مستخدم",
                     verificationToken);
                       }
                       catch (Exception ex)
                       {
                           _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
                       }
                   });
        }

        /// <summary>
        /// Ensures the specified role exists in database
        /// OPTIMIZED: Uses memory cache to avoid repeated database lookups
        /// </summary>
        private async Task<Guid> EnsureRoleAsync(RegistrationRole desired)
        {
            var name = desired switch
            {
                RegistrationRole.Customer => "Customer",
                RegistrationRole.Tailor => "Tailor",
                // RegistrationRole.Corporate => "Corporate", // REMOVED: Corporate feature
                _ => "Customer"
            };

            // Try to get from cache first
            var cacheKey = $"Role_{name}";
            if (_cache.TryGetValue(cacheKey, out Guid cachedRoleId))
            {
                _logger.LogDebug("[AuthService] Role retrieved from cache: {RoleName}", name);
                return cachedRoleId;
            }

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == name);
            if (role != null)
            {
                // Cache the role ID for 1 hour
                _cache.Set(cacheKey, role.Id, TimeSpan.FromHours(1));
                _logger.LogDebug("[AuthService] Role exists and cached: {RoleName}", name);
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
                    // "Corporate" => "شركة - حساب مؤسسي للطلبات الجماعية", // REMOVED: Corporate feature
                    _ => null
                },
                CreatedAt = _dateTime.Now
            };

            await _db.Roles.AddAsync(role);
            await _db.SaveChangesAsync();

            // Cache the new role ID
            _cache.Set(cacheKey, role.Id, TimeSpan.FromHours(1));

            _logger.LogInformation("[AuthService] New role created and cached: {RoleName} ({RoleId})", name, role.Id);

            return role.Id;
        }

        // ✅ REMOVED: UpdateLastLoginAsync - now handled inline with proper scope

        /// <summary>
        /// Gets user full name from appropriate profile
        /// OPTIMIZED: Only loads necessary data
        /// </summary>
        private async Task<string> GetUserFullNameAsync(Guid userId)
        {
            try
            {
                // Optimized: Use projection to only load what we need
                var userInfo = await _db.Users
               .AsNoTracking()
               .Where(u => u.Id == userId)
                     .Select(u => new
                     {
                         u.Email,
                         RoleName = u.Role.Name,
                         CustomerName = u.CustomerProfile != null ? u.CustomerProfile.FullName : null,
                         TailorName = u.TailorProfile != null ? u.TailorProfile.FullName : null
                         // REMOVED: Corporate fields
                         // CorporatePerson = u.CorporateAccount != null ? u.CorporateAccount.ContactPerson : null,
                         // CompanyName = u.CorporateAccount != null ? u.CorporateAccount.CompanyName : null
                     })
       .FirstOrDefaultAsync();

                if (userInfo == null) return "مستخدم";

                return userInfo.RoleName?.ToLower() switch
                {
                    "customer" => userInfo.CustomerName ?? userInfo.Email ?? "مستخدم",
                    "tailor" => userInfo.TailorName ?? userInfo.Email ?? "مستخدم",
                    // "corporate" => userInfo.CorporatePerson ?? userInfo.CompanyName ?? userInfo.Email ?? "مستخدم", // REMOVED
                    _ => userInfo.Email ?? "مستخدم"
                };
            }
            catch
            {
                return "مستخدم";
            }
        }

        private string GenerateEmailVerificationToken()
        {
            // Generate a URL-safe random token
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
          .Replace("+", "")
           .Replace("/", "")
          .Replace("=", "");

            // Ensure we have at least 32 characters, pad with another GUID if needed
            while (token.Length < 32)
            {
                token += Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    .Replace("+", "")
              .Replace("/", "")
              .Replace("=", "");
            }

            return token.Substring(0, 32);
        }

        #endregion
    }
}
