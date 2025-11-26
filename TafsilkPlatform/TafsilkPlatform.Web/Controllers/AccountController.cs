using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TafsilkPlatform.DataAccess.Repository;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels;
using TafsilkPlatform.Utility.Security;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Services;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// Manages user account operations such as registration, login, and profile management.
/// </summary>
[Authorize]
public class AccountController(
    IAuthService auth,
    TafsilkPlatform.DataAccess.Repository.IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IFileUploadService fileUploadService,
    ILogger<AccountController> logger,
    IDateTimeService dateTime) : Controller
{
    private readonly IAuthService _auth = auth;
    private readonly TafsilkPlatform.DataAccess.Repository.IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IFileUploadService _fileUploadService = fileUploadService;
    private readonly ILogger<AccountController> _logger = logger;
    private readonly IDateTimeService _dateTime = dateTime;

    #region Registration & Login

    /// <summary>
    /// Displays the registration page. Redirects authenticated users to their dashboard.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        // If user is already authenticated, redirect to their dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            var roleName = User.FindFirstValue(ClaimTypes.Role);
            _logger.LogInformation("[AccountController] Authenticated user {Email} attempted to access Register. Redirecting to dashboard.",
           User.FindFirstValue(ClaimTypes.Email));
            TempData["InfoMessage"] = "أنت مسجل دخول بالفعل. يرجى تسجيل الخروج أولاً إذا كنت تريد إنشاء حساب جديد.";
            return RedirectToRoleDashboard(roleName);
        }

        return View();
    }

    /// <summary>
    /// Handles user registration POST. Validates input, creates user, and redirects based on role.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string name, string email, string password, string userType, string? phoneNumber)
    {
        // If user is already authenticated, redirect to their dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            var roleName = User.FindFirstValue(ClaimTypes.Role);
            _logger.LogWarning("[AccountController] Authenticated user {Email} attempted to POST Register. Blocking.",
        User.FindFirstValue(ClaimTypes.Email));
            TempData["ErrorMessage"] = "أنت مسجل دخول بالفعل. لا يمكنك إنشاء حساب جديد أثناء تسجيل الدخول.";
            return RedirectToRoleDashboard(roleName);
        }

        // Sanitize inputs
        name = SanitizeInput(name ?? string.Empty, 100);
        email = SanitizeInput(email ?? string.Empty, 254)?.ToLowerInvariant() ?? string.Empty;

        // Validate name
        if (string.IsNullOrWhiteSpace(name))
            ModelState.AddModelError(nameof(name), "الاسم الكامل مطلوب");
        else if (name.Length < 2)
            ModelState.AddModelError(nameof(name), "الاسم يجب أن يكون حرفين على الأقل");
        else if (name.Length > 100)
            ModelState.AddModelError(nameof(name), "الاسم طويل جداً (100 حرف كحد أقصى)");

        // Validate email
        if (string.IsNullOrWhiteSpace(email))
            ModelState.AddModelError(nameof(email), "البريد الإلكتروني مطلوب");
        else if (!IsValidEmail(email))
            ModelState.AddModelError(nameof(email), "البريد الإلكتروني غير صالح. يرجى إدخال بريد إلكتروني صحيح");

        // Validate password
        if (string.IsNullOrWhiteSpace(password))
            ModelState.AddModelError(nameof(password), "كلمة المرور مطلوبة");
        else
        {
            var (isValidPassword, passwordError) = ValidatePasswordStrength(password);
            if (!isValidPassword)
                ModelState.AddModelError(nameof(password), passwordError!);
        }

        // Validate phone number
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            var (isValidPhone, phoneError) = ValidatePhoneNumber(phoneNumber);
            if (!isValidPhone)
                ModelState.AddModelError(nameof(phoneNumber), phoneError!);
        }

        if (!ModelState.IsValid)
            return View();

        var role = userType?.ToLowerInvariant() switch
        {
            "tailor" => RegistrationRole.Tailor,
            _ => RegistrationRole.Customer
        };

        var req = new RegisterRequest
        {
            Email = email,
            Password = password,
            FullName = name,
            PhoneNumber = phoneNumber,
            Role = role
        };

        var (ok, err, user) = await _auth.RegisterAsync(req);
        if (!ok || user is null)
        {
            ModelState.AddModelError(string.Empty, err ?? "فشل التسجيل. يرجى المحاولة مرة أخرى");
            return View();
        }

        _logger.LogInformation("[AccountController] User registered successfully: {Email}, Role: {Role}", email, role);

        // For Tailors: Redirect to CompleteTailorProfile (better UX wizard)
        if (role == RegistrationRole.Tailor)
        {
            TempData["UserId"] = user.Id.ToString();
            TempData["UserEmail"] = email;
            TempData["UserName"] = name;
            TempData["InfoMessage"] = "تم إنشاء حسابك بنجاح! يجب إكمال ملفك الشخصي وتقديم الأوراق الثبوتية";
            return RedirectToAction(nameof(CompleteTailorProfile));
        }

        // ✅ UPDATED: For Customers - Auto-login and redirect to their dashboard
        if (role == RegistrationRole.Customer)
        {
            // Build claims for authentication using collection expression
            List<Claim> claims =
                    [
               new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email ?? string.Empty),
             new(ClaimTypes.Name, name),
    new("FullName", name),
                new(ClaimTypes.Role, "Customer")
               ];

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
              new AuthenticationProperties { IsPersistent = true });

            _logger.LogInformation("[AccountController] Customer auto-logged in after registration: {Email}", email);

            TempData["SuccessMessage"] = "مرحباً بك! تم إنشاء حسابك بنجاح";
            return RedirectToAction("Customer", "Dashboards");
        }

        // Default fallback (should not reach here)
        TempData["SuccessMessage"] = "تم إنشاء الحساب بنجاح!";
        return RedirectToAction("Login");
    }

    /// <summary>
    /// Displays the login page.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// Handles user login POST. Validates credentials and signs in the user.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, bool rememberMe = false, string? returnUrl = null)
    {
        // Sanitize inputs
        email = SanitizeInput(email ?? string.Empty, 254)?.ToLowerInvariant() ?? string.Empty;

        // Validate
        if (string.IsNullOrWhiteSpace(email) && !ModelState.ContainsKey(nameof(email)))
            ModelState.AddModelError(nameof(email), "البريد الإلكتروني مطلوب");
        else if (!IsValidEmail(email))
            ModelState.AddModelError(nameof(email), "البريد الإلكتروني غير صالح");

        if (string.IsNullOrWhiteSpace(password))
            ModelState.AddModelError(nameof(password), "كلمة المرور مطلوبة");

        if (!ModelState.IsValid)
            return View();

        var (ok, err, user) = await _auth.ValidateUserAsync(email, password);

        // Handle tailor with incomplete profile
        if (!ok && err == "TAILOR_INCOMPLETE_PROFILE" && user is not null)
        {
            _logger.LogWarning("[AccountController] Tailor {Email} attempted login without complete profile. Redirecting to complete profile page.", email);

            TempData["UserId"] = user.Id.ToString();
            TempData["UserEmail"] = user.Email ?? email;
            TempData["UserName"] = user.Email ?? email;
            TempData["InfoMessage"] = "يجب إكمال ملفك الشخصي وتقديم الأوراق الثبوتية قبل تسجيل الدخول";

            return RedirectToAction(nameof(CompleteTailorProfile), new { userId = user.Id });
        }

        if (!ok || user is null)
        {
            ModelState.AddModelError(string.Empty, err ?? "البريد الإلكتروني أو كلمة المرور غير صحيحة");
            return View();
        }

        // ✅ FIX: Use AuthService to build claims (avoids concurrent DbContext usage)
        var claims = await _auth.GetUserClaimsAsync(user);

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
        new AuthenticationProperties { IsPersistent = rememberMe });

        var roleName = user.Role?.Name ?? string.Empty;

        // Redirect to dashboard or return URL
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToRoleDashboard(roleName);
    }

    /// <summary>
    /// Logs out the current user and redirects to the home page.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    #endregion

    #region Tailor Profile Completion

    /// <summary>
    /// GET: Complete Tailor Profile - Handles both authenticated and unauthenticated tailors
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> CompleteTailorProfile(Guid? userId)
    {
        _logger.LogInformation("[AccountController] CompleteTailorProfile GET accessed. UserId param: {UserId}", userId);

        // Check if coming from registration (unauthenticated)
        var userIdStr = TempData["UserId"]?.ToString();

        // Keep the data for the POST
        TempData.Keep("UserId");
        TempData.Keep("UserEmail");
        TempData.Keep("UserName");
        TempData.Keep("InfoMessage");

        Guid userGuid;
        User? user = null;

        // Priority 1: Get from query string parameter (most reliable)
        if (userId.HasValue && userId.Value != Guid.Empty)
        {
            userGuid = userId.Value;
            _logger.LogInformation("[AccountController] Using UserId from query string: {UserId}", userGuid);
        }
        // Priority 2: Get from TempData (fallback)
        else if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out userGuid))
        {
            _logger.LogInformation("[AccountController] Using UserId from TempData: {UserId}", userGuid);
        }
        // Priority 3: Check if authenticated tailor (editing profile after login)
        else if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out userGuid))
            {
                _logger.LogWarning("[AccountController] Authenticated user has no valid UserId claim");
                TempData["ErrorMessage"] = "جلسة غير صالحة. يرجى تسجيل الدخول مرة أخرى";
                return RedirectToAction(nameof(Login));
            }

            _logger.LogInformation("[AccountController] Using UserId from claims: {UserId}", userGuid);
        }
        else
        {
            // No user ID and not authenticated - redirect to registration
            _logger.LogWarning("[AccountController] No UserId available from any source");
            TempData["InfoMessage"] = "يرجى التسجيل أولاً لإنشاء حساب خياط";
            return RedirectToAction(nameof(Register));
        }

        // ✅ FIX: Use GetUserWithProfileAsync which includes Role navigation property
        user = await _userRepository.GetUserWithProfileAsync(userGuid);

        // ✅ FIX: More detailed validation and better error messages
        if (user == null)
        {
            _logger.LogWarning("[AccountController] User not found: {UserId}", userGuid);
            TempData["ErrorMessage"] = "المستخدم غير موجود. يرجى التسجيل مرة أخرى";
            return RedirectToAction(nameof(Register));
        }

        if (user.Role == null)
        {
            _logger.LogError("[AccountController] User {UserId} has no role assigned. Email: {Email}", userGuid, user.Email);
            TempData["ErrorMessage"] = "خطأ في البيانات: الدور غير محدد. يرجى الاتصال بالدعم";
            return RedirectToAction(nameof(Register));
        }

        if (user.Role.Name?.ToLower() != "tailor")
        {
            _logger.LogWarning("[AccountController] User {UserId} is not a tailor. Role: {Role}", userGuid, user.Role.Name);
            TempData["ErrorMessage"] = $"هذا الحساب مسجل كـ {user.Role.Name}. يرجى تسجيل الدخول بدلاً من ذلك";
            return RedirectToAction(nameof(Login));
        }

        _logger.LogInformation("[AccountController] User found: {UserId}, Email: {Email}, Role: {Role}",
                    user.Id, user.Email, user.Role.Name);

        // CRITICAL: Check if profile already exists (evidence already provided)
        // This ensures ONE-TIME submission only
        var existingProfile = await _unitOfWork.Tailors.GetByUserIdAsync(user.Id);
        if (existingProfile != null)
        {
            _logger.LogWarning("[AccountController] Tailor {UserId} attempted to access complete profile page but already has profile. Redirecting to dashboard.", user.Id);
            TempData["SuccessMessage"] = "تم إكمال ملفك الشخصي بالفعل. مرحباً بك!";

            // ✅ FIX: If profile exists, auto-login and redirect to dashboard
            if (User.Identity?.IsAuthenticated != true)
            {
                // Build claims for authentication
                var claims = new List<Claim>
  {
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
          new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
      new Claim(ClaimTypes.Name, existingProfile.FullName ?? user.Email ?? "خياط"),
         new Claim("FullName", existingProfile.FullName ?? user.Email ?? "خياط"),
    new Claim(ClaimTypes.Role, "Tailor")
  };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = true });
            }

            return RedirectToAction("Tailor", "Dashboards");
        }

        var model = new CompleteTailorProfileRequest
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.Email ?? "خياط جديد"
        };

        return View(model);
    }

    /// <summary>
    /// POST: Complete Tailor Profile - Creates profile with evidence submission
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteTailorProfile(CompleteTailorProfileRequest model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            // Get the user - model.UserId comes from hidden field
            var user = await _unitOfWork.Users.GetByIdAsync(model.UserId);
            if (user == null)
            {
                _logger.LogWarning("[AccountController] User not found during profile completion: {UserId}", model.UserId);
                ModelState.AddModelError(string.Empty, "المستخدم غير موجود");
                return View(model);
            }

            // ✅ FIX: Load role navigation property
            await _unitOfWork.Context.Entry(user).Reference(u => u.Role).LoadAsync();

            if (user.Role == null || user.Role.Name?.ToLower() != "tailor")
            {
                _logger.LogWarning("[AccountController] Invalid user or not a tailor: {UserId}, Role: {Role}",
                  model.UserId, user.Role?.Name ?? "NULL");
                ModelState.AddModelError(string.Empty, "حساب غير صالح");
                return View(model);
            }

            // CRITICAL: Check if profile already exists - BLOCK double submission
            var existingProfile = await _unitOfWork.Tailors.GetByUserIdAsync(model.UserId);
            if (existingProfile != null)
            {
                _logger.LogWarning("[AccountController] Tailor {UserId} attempted to submit profile but already has one. Blocking submission.", model.UserId);
                TempData["InfoMessage"] = "تم إكمال ملفك الشخصي بالفعل. لا يمكن التقديم مرة أخرى.";
                return RedirectToAction("Tailor", "Dashboards");
            }

            // ✅ FIX: Documents are now OPTIONAL (removed from UI)
            // Only validate if provided
            if (model.IdDocumentFront != null && model.IdDocumentFront.Length > 0)
            {
                var (isValidId, idError) = ValidateFileUpload(model.IdDocumentFront, "document");
                if (!isValidId)
                {
                    ModelState.AddModelError(nameof(model.IdDocumentFront), idError!);
                    return View(model);
                }
            }

            // Portfolio images are also optional now
            if (model.PortfolioImages != null && model.PortfolioImages.Any())
            {
                var portfolioFiles = model.PortfolioImages;

                if (portfolioFiles.Count > 10)
                {
                    ModelState.AddModelError(string.Empty, "يمكن تحميل 10 صور كحد أقصى");
                    return View(model);
                }

                foreach (var image in portfolioFiles)
                {
                    var (isValidImage, imageError) = ValidateFileUpload(image, "image");
                    if (!isValidImage)
                    {
                        ModelState.AddModelError(nameof(model.PortfolioImages), imageError!);
                        return View(model);
                    }
                }
            }

            // Sanitize text inputs
            var sanitizedFullName = SanitizeInput(model.FullName, 100);
            var sanitizedWorkshopName = SanitizeInput(model.WorkshopName, 100);
            var sanitizedAddress = SanitizeInput(model.Address, 200);
            var sanitizedCity = SanitizeInput(model.City, 50);
            var sanitizedDescription = SanitizeInput(model.Description, 1000);

            // Create tailor profile NOW (this is the ONE AND ONLY TIME)
            var tailorProfile = new TailorProfile
            {
                Id = Guid.NewGuid(),
                UserId = model.UserId,
                FullName = sanitizedFullName,
                ShopName = sanitizedWorkshopName,
                Address = sanitizedAddress,
                City = sanitizedCity,
                Bio = sanitizedDescription,
                Specialization = SanitizeInput(model.WorkshopType, 50) ?? "خياطة عامة",
                ExperienceYears = model.ExperienceYears,
                IsVerified = false, // Awaiting admin approval
                CreatedAt = _dateTime.Now
            };

            await _unitOfWork.Tailors.AddAsync(tailorProfile);

            await _unitOfWork.SaveChangesAsync();
            // Keep user ACTIVE so they can use the platform immediately
            // Admin verification (IsVerified) is checked separately for sensitive features
            user.IsActive = true;
            user.UpdatedAt = _dateTime.Now;

            // Update phone number if provided
            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                user.PhoneNumber = model.PhoneNumber;
            }

            // ✅ FIX: Generate email verification token safely
            var verificationToken = GenerateSecureToken();

            user.EmailVerificationToken = verificationToken;
            user.EmailVerificationTokenExpires = _dateTime.Now.AddHours(24);

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("[AccountController] Tailor {UserId} completed profile. User is ACTIVE and can use platform.", model.UserId);

            // ✅ FIX: Auto-login the tailor and redirect to their dashboard
            // Build claims for authentication
            var claims = new List<Claim>
       {
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
        new Claim(ClaimTypes.Name, sanitizedFullName ?? user.Email ?? "خياط"),
     new Claim("FullName", sanitizedFullName ?? user.Email ?? "خياط"),
       new Claim(ClaimTypes.Role, "Tailor")
  };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true });

            _logger.LogInformation("[AccountController] Tailor {UserId} auto-logged in after profile completion.", model.UserId);

            TempData["SuccessMessage"] = "تم إكمال ملفك الشخصي بنجاح! مرحباً بك في منصة تفصيلك.";
            return RedirectToAction("Tailor", "Dashboards");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AccountController] Error completing tailor profile for user: {UserId}", model.UserId);
            ModelState.AddModelError(string.Empty, "حدث خطأ أثناء حفظ البيانات. يرجى المحاولة مرة أخرى.");
            return View(model);
        }
    }

    #endregion

    #region Profile & Settings

    /// <summary>
    /// Returns the profile picture for a user by ID, or a404 if not found.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ProfilePicture(Guid id)
    {
        try
        {
            byte[]? imageData = null;
            string? contentType = null;

            // Try to get profile picture from Customer profile
            var customerProfile = await _unitOfWork.Customers.GetByUserIdAsync(id);
            if (customerProfile?.ProfilePictureData != null)
            {
                imageData = customerProfile.ProfilePictureData;
                contentType = customerProfile.ProfilePictureContentType ?? "image/jpeg";
            }

            // Try Tailor profile if not found
            if (imageData == null)
            {
                var tailorProfile = await _unitOfWork.Tailors.GetByUserIdAsync(id);
                if (tailorProfile?.ProfilePictureData != null)
                {
                    imageData = tailorProfile.ProfilePictureData;
                    contentType = tailorProfile.ProfilePictureContentType ?? "image/jpeg";
                }
            }

            // Return image or placeholder
            if (imageData != null)
                return File(imageData, contentType ?? "image/jpeg");

            return NotFound();
        }
        catch
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Displays the change password page.
    /// </summary>
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    /// <summary>
    /// Handles POST for changing the user's password.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var user = await _unitOfWork.Users.GetByIdAsync(userGuid);
        if (user == null)
            return NotFound();

        // Verify current password
        if (!PasswordHasher.Verify(user.PasswordHash, model.CurrentPassword))
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "كلمة المرور الحالية غير صحيحة");
            return View(model);
        }

        // Update to new password
        user.PasswordHash = PasswordHasher.Hash(model.NewPassword);
        user.UpdatedAt = _dateTime.Now;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "تم تغيير كلمة المرور بنجاح!";

        var roleName = User.FindFirstValue(ClaimTypes.Role);
        return RedirectToRoleDashboard(roleName);
    }

    /// <summary>
    /// Redirects to the user's dashboard based on their role.
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

    #region Email Verification

    /// <summary>
    /// Verifies a user's email using a token from the verification link.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            TempData["ErrorMessage"] = "رابط التحقق غير صالح";
            return RedirectToAction("Login");
        }

        var (success, error) = await _auth.VerifyEmailAsync(token);

        if (success)
            TempData["RegisterSuccess"] = "تم تأكيد بريدك الإلكتروني بنجاح! يمكنك الآن تسجيل الدخول";
        else
            TempData["ErrorMessage"] = error ?? "فشل تأكيد البريد الإلكتروني";

        return RedirectToAction("Login");
    }

    /// <summary>
    /// Displays the resend verification email page.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResendVerificationEmail()
    {
        return View();
    }

    /// <summary>
    /// Handles POST for resending the verification email.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendVerificationEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(nameof(email), "البريد الإلكتروني مطلوب");
            return View();
        }

        var (success, error) = await _auth.ResendVerificationEmailAsync(email);

        if (success)
            TempData["RegisterSuccess"] = "تم إرسال رسالة التحقق بنجاح! يرجى التحقق من بريدك الإلكتروني";
        else
            TempData["ErrorMessage"] = error ?? "فشل إرسال رسالة التحقق";

        return View();
    }

    #endregion

    #region OAuth (Google)

    /// <summary>
    /// Initiates Google OAuth login.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GoogleLogin(string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(GoogleResponse), "Account", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, "Google");
    }

    /// <summary>
    /// Handles Google OAuth response.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleResponse(string? returnUrl = null)
    {
        return await HandleOAuthResponse("Google", returnUrl);
    }

    /// <summary>
    /// Handles OAuth response for Google.
    /// </summary>
    private async Task<IActionResult> HandleOAuthResponse(string provider, string? returnUrl = null)
    {
        try
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(provider);

            if (!authenticateResult.Succeeded)
            {
                TempData["ErrorMessage"] = $"فشل تسجيل الدخول عبر {provider}";
                return RedirectToAction(nameof(Login));
            }

            var claims = authenticateResult.Principal?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var providerId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            // Get profile picture from Google
            string? picture = claims?.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value
      ?? claims?.FirstOrDefault(c => c.Type == "picture")?.Value;

            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = $"لم نتمكن من الحصول على بريدك الإلكتروني من {provider}";
                return RedirectToAction(nameof(Login));
            }

            // Check if user exists
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(email);

            if (existingUser != null)
            {
                // User exists, sign them in
                string fullName = existingUser.Email ?? "مستخدم";
                var roleName = existingUser.Role?.Name ?? string.Empty;

                if (!string.IsNullOrEmpty(roleName))
                {
                    switch (roleName.ToLower())
                    {
                        case "customer":
                            var customer = await _unitOfWork.Customers.GetByUserIdAsync(existingUser.Id);
                            if (customer != null && !string.IsNullOrEmpty(customer.FullName))
                                fullName = customer.FullName;
                            break;
                        case "tailor":
                            var tailor = await _unitOfWork.Tailors.GetByUserIdAsync(existingUser.Id);
                            if (tailor != null && !string.IsNullOrEmpty(tailor.FullName))
                                fullName = tailor.FullName;
                            break;
                    }
                }

                var userClaims = new List<Claim>
          {
           new Claim(ClaimTypes.NameIdentifier, existingUser.Id.ToString()),
     new Claim(ClaimTypes.Email, existingUser.Email ?? string.Empty),
  new Claim(ClaimTypes.Name, fullName),
      new Claim("FullName", fullName)
   };

                if (!string.IsNullOrEmpty(roleName))
                    userClaims.Add(new Claim(ClaimTypes.Role, roleName));

                var identity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
              new AuthenticationProperties { IsPersistent = true });

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToRoleDashboard(roleName);
            }
            else
            {
                // New user - store data in session and redirect to complete registration
                TempData["OAuthProvider"] = provider;
                TempData["OAuthEmail"] = email;
                TempData["OAuthName"] = name ?? string.Empty;
                TempData["OAuthPicture"] = picture ?? string.Empty;
                TempData["OAuthId"] = providerId ?? string.Empty;

                return RedirectToAction(nameof(CompleteSocialRegistration));
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"حدث خطأ أثناء تسجيل الدخول: {ex.Message}";
            return RedirectToAction(nameof(Login));
        }
    }

    /// <summary>
    /// Displays the complete Google registration page for new OAuth users.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult CompleteGoogleRegistration()
    {
        return CompleteSocialRegistration();
    }

    /// <summary>
    /// Displays the complete social registration page for new OAuth users.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult CompleteSocialRegistration()
    {
        var provider = TempData["OAuthProvider"]?.ToString() ?? "Google";
        var email = TempData["OAuthEmail"]?.ToString();
        var name = TempData["OAuthName"]?.ToString();
        var picture = TempData["OAuthPicture"]?.ToString();

        if (string.IsNullOrEmpty(email))
            return RedirectToAction(nameof(Register));

        // Keep data for the form
        TempData.Keep("OAuthProvider");
        TempData.Keep("OAuthEmail");
        TempData.Keep("OAuthName");
        TempData.Keep("OAuthPicture");
        TempData.Keep("OAuthId");

        var model = new CompleteGoogleRegistrationViewModel
        {
            Email = email,
            FullName = name ?? string.Empty,
            ProfilePictureUrl = picture
        };

        ViewData["Provider"] = provider;
        return View("CompleteGoogleRegistration", model);
    }

    /// <summary>
    /// Handles POST for completing Google registration for OAuth users.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteGoogleRegistration(CompleteGoogleRegistrationViewModel model)
    {
        return await CompleteSocialRegistrationPost(model);
    }

    /// <summary>
    /// Handles POST for completing social registration for OAuth users.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteSocialRegistration(CompleteGoogleRegistrationViewModel model)
    {
        return await CompleteSocialRegistrationPost(model);
    }

    /// <summary>
    /// Handles the logic for completing social registration for OAuth users.
    /// </summary>
    private async Task<IActionResult> CompleteSocialRegistrationPost(CompleteGoogleRegistrationViewModel model)
    {
        if (!ModelState.IsValid)
            return View("CompleteGoogleRegistration", model);

        var provider = TempData["OAuthProvider"]?.ToString() ?? "Google";
        var email = TempData["OAuthEmail"]?.ToString();
        var oauthId = TempData["OAuthId"]?.ToString();
        var picture = TempData["OAuthPicture"]?.ToString();

        if (string.IsNullOrEmpty(email))
            return RedirectToAction(nameof(Register));

        try
        {
            // Determine role
            var role = model.UserType?.ToLowerInvariant() switch
            {
                "tailor" => RegistrationRole.Tailor,
                _ => RegistrationRole.Customer
            };

            // Create registration request
            var registerRequest = new RegisterRequest
            {
                Email = email,
                Password = Guid.NewGuid().ToString(), // Generate random password for OAuth users
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Role = role
            };

            var (ok, err, user) = await _auth.RegisterAsync(registerRequest);

            if (!ok || user == null)
            {
                ModelState.AddModelError(string.Empty, err ?? "فشل إنشاء الحساب");
                ViewData["Provider"] = provider;
                return View("CompleteGoogleRegistration", model);
            }

            // Update profile picture if available from OAuth provider
            if (!string.IsNullOrEmpty(picture))
            {
                if (role == RegistrationRole.Customer)
                {
                    var customerProfile = await _unitOfWork.Customers.GetByUserIdAsync(user.Id);
                    if (customerProfile != null)
                    {
                        // TODO: Download and store the OAuth profile picture
                        await _unitOfWork.Customers.UpdateAsync(customerProfile);
                    }
                }
                else if (role == RegistrationRole.Tailor)
                {
                    var tailorProfile = await _unitOfWork.Tailors.GetByUserIdAsync(user.Id);
                    if (tailorProfile != null)
                    {
                        // TODO: Download and store the OAuth profile picture
                        await _unitOfWork.Tailors.UpdateAsync(tailorProfile);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }

            // Sign in the user
            string fullName = model.FullName;
            var roleName = user.Role?.Name ?? string.Empty;

            if (!string.IsNullOrEmpty(roleName) && !string.IsNullOrEmpty(fullName))
            {
                switch (roleName.ToLower())
                {
                    case "customer":
                        var customer = await _unitOfWork.Customers.GetByUserIdAsync(user.Id);
                        if (customer != null && !string.IsNullOrEmpty(customer.FullName))
                            fullName = customer.FullName;
                        break;
                    case "tailor":
                        var tailor = await _unitOfWork.Tailors.GetByUserIdAsync(user.Id);
                        if (tailor != null && !string.IsNullOrEmpty(tailor.FullName))
                            fullName = tailor.FullName;
                        break;
                }
            }

            var userClaims = new List<Claim>
        {
       new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
  new Claim(ClaimTypes.Name, fullName),
      new Claim("FullName", fullName)
            };

            if (!string.IsNullOrEmpty(roleName))
                userClaims.Add(new Claim(ClaimTypes.Role, roleName));

            var identity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
     new AuthenticationProperties { IsPersistent = true });

            return RedirectToRoleDashboard(roleName);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"حدث خطأ: {ex.Message}");
            ViewData["Provider"] = provider;
            return View("CompleteGoogleRegistration", model);
        }
    }

    #endregion

    #region Role Management

    /// <summary>
    /// Displays the request role change page.
    /// </summary>
    [HttpGet]
    public IActionResult RequestRoleChange()
    {
        return View();
    }

    /// <summary>
    /// Handles POST for requesting a role change.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestRoleChange(RoleChangeRequestViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var user = await _unitOfWork.Users.GetByIdAsync(userGuid);
        if (user == null)
            return NotFound();

        var currentRole = User.FindFirstValue(ClaimTypes.Role);

        // Validate role change
        if (currentRole == "Tailor" && model.TargetRole == "Customer")
        {
            ModelState.AddModelError(string.Empty, "لا يمكن التحويل من خياط إلى عميل بشكل مباشر. يرجى الاتصال بالدعم.");
            return View(model);
        }

        // Allow Customer -> Tailor conversion
        if (model.TargetRole == "Tailor" && currentRole == "Customer")
        {
            if (string.IsNullOrWhiteSpace(model.ShopName) || string.IsNullOrWhiteSpace(model.Address))
            {
                ModelState.AddModelError(string.Empty, "اسم المتجر والعنوان مطلوبان للخياطين");
                return View(model);
            }

            var tailorRole = await _unitOfWork.Context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Tailor");
            if (tailorRole == null)
            {
                ModelState.AddModelError(string.Empty, "دور الخياط غير موجود في النظام");
                return View(model);
            }

            user.RoleId = tailorRole.Id;
            user.UpdatedAt = _dateTime.Now;
            await _unitOfWork.Users.UpdateAsync(user);

            var tailorProfile = new TailorProfile
            {
                Id = Guid.NewGuid(),
                UserId = userGuid,
                FullName = user.CustomerProfile?.FullName ?? string.Empty,
                ShopName = model.ShopName,
                Address = model.Address,
                ExperienceYears = model.ExperienceYears,
                CreatedAt = _dateTime.Now
            };

            await _unitOfWork.Tailors.AddAsync(tailorProfile);
            await _unitOfWork.SaveChangesAsync();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["RegisterSuccess"] = "تم تحويل حسابك إلى خياط بنجاح! يرجى تسجيل الدخول مرة أخرى.";
            return RedirectToAction("Login");
        }

        TempData["ErrorMessage"] = "طلب تغيير الدور غير مدعوم حالياً";
        var roleName = User.FindFirstValue(ClaimTypes.Role);
        return RedirectToRoleDashboard(roleName);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Redirects the user to their dashboard based on their role.
    /// </summary>
    private IActionResult RedirectToRoleDashboard(string? roleName) =>
        roleName?.ToLowerInvariant() switch
        {
            "tailor" => RedirectToAction("Tailor", "Dashboards"),
            "admin" => RedirectToAction("Index", "AdminDashboard", new { area = "Admin" }),
            _ => RedirectToAction("Customer", "Dashboards")
        };

    /// <summary>
    /// Validates the format of an email address.
    /// </summary>
    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email && email.Contains('@') && email.Length <= 254;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates password strength and returns a tuple indicating validity and error message.
    /// </summary>
    private static (bool IsValid, string? Error) ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "كلمة المرور مطلوبة");

        if (password.Length < 8)
            return (false, "كلمة المرور يجب أن تكون 8 أحرف على الأقل");

        if (password.Length > 128)
            return (false, "كلمة المرور طويلة جداً");

        if (!password.Any(char.IsUpper))
            return (false, "كلمة المرور يجب أن تحتوي على حرف كبير واحد على الأقل");

        if (!password.Any(char.IsLower))
            return (false, "كلمة المرور يجب أن تحتوي على حرف صغير واحد على الأقل");

        if (!password.Any(char.IsDigit))
            return (false, "كلمة المرور يجب أن تحتوي على رقم واحد على الأقل");

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            return (false, "كلمة المرور يجب أن تحتوي على رمز خاص واحد على الأقل");

        string[] weakPasswords = ["password1!", "qwerty123!", "admin123!", "welcome1!", "Password1!", "Qwerty123!"];
        if (weakPasswords.Any(weak => password.Equals(weak, StringComparison.OrdinalIgnoreCase)))
            return (false, "كلمة المرور ضعيفة جداً. يرجى اختيار كلمة مرور أقوى");

        return (true, null);
    }

    /// <summary>
    /// Validates the format and strength of a phone number.
    /// </summary>
    private static (bool IsValid, string? Error) ValidatePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return (false, "رقم الهاتف مطلوب");

        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

        if (digitsOnly.Length < 9)
            return (false, "رقم الهاتف يجب أن يحتوي على 9 أرقام على الأقل");

        string[] invalidPrefixes = ["000", "111", "222", "333", "444", "555", "666", "777", "888", "999"];
        if (invalidPrefixes.Any(digitsOnly.StartsWith) ||
           digitsOnly == new string('0', digitsOnly.Length) ||
               digitsOnly == new string('1', digitsOnly.Length) ||
          digitsOnly == new string('2', digitsOnly.Length))
        {
            return (false, "رقم الهاتف ضعيف جداً. يرجى اختيار رقم آخر");
        }

        return (true, null);
    }

    /// <summary>
    /// Validates uploaded files for type and size.
    /// </summary>
    private static (bool IsValid, string? Error) ValidateFileUpload(IFormFile? file, string fileType = "image")
    {
        if (file is null || file.Length == 0)
            return (false, "الملف مطلوب");

        var maxSize = fileType == "image" ? 5 * 1024 * 1024 : 10 * 1024 * 1024;
        if (file.Length > maxSize)
            return (false, $"حجم الملف كبير جداً. الحد الأقصى {maxSize / (1024 * 1024)} ميجابايت");

        string[] allowedExtensions = fileType == "image"
      ? [".jpg", ".jpeg", ".png", ".gif", ".webp"]
      : [".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png"];

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return (false, $"نوع الملف غير مدعوم. الأنواع المسموحة: {string.Join(", ", allowedExtensions)}");

        string[] allowedContentTypes = fileType == "image"
                  ? ["image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"]
                  : ["application/pdf", "application/msword",
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
      "image/jpeg", "image/png"];

        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return (false, "نوع محتوى الملف غير صحيح");

        var fileName = Path.GetFileName(file.FileName);
        if (fileName.Contains("..") || fileName.Contains('/') || fileName.Contains('\\'))
            return (false, "اسم الملف غير صالح");

        return (true, null);
    }

    /// <summary>
    /// Sanitizes user input to prevent XSS and SQL injection.
    /// </summary>
    private static string SanitizeInput(string? input, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        input = input.Trim();
        input = System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);

        string[] sqlPatterns = ["--", ";--", "';", "')", "' OR '", "' AND '", "DROP ", "INSERT ", "DELETE ", "UPDATE ", "EXEC "];
        foreach (var pattern in sqlPatterns)
            input = input.Replace(pattern, "", StringComparison.OrdinalIgnoreCase);

        if (input.Length > maxLength)
            input = input[..maxLength];

        return input;
    }

    /// <summary>
    /// Generates a secure random token for email verification or other purposes.
    /// </summary>
    private static string GenerateSecureToken()
    {
        // Generate a 32-byte random token
        var randomBytes = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Convert to Base64 and make it URL-safe
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    #endregion
}
