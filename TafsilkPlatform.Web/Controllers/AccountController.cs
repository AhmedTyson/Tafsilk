using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Security;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.ViewModels;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// Manages user account operations such as registration, login, and profile management.
/// </summary>
[Authorize]
public class AccountController : Controller
{
    private readonly IAuthService _auth;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<AccountController> _logger;
    private readonly IDateTimeService _dateTime;

    public AccountController(
        IAuthService auth,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IFileUploadService fileUploadService,
      ILogger<AccountController> logger,
   IDateTimeService dateTime)
    {
        _auth = auth;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _fileUploadService = fileUploadService;
        _logger = logger;
        _dateTime = dateTime;
    }

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
            "corporate" => RegistrationRole.Corporate,
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
            // Build claims for authentication
            var claims = new List<Claim>
            {
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
 new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
new Claim(ClaimTypes.Name, name),
          new Claim("FullName", name),
           new Claim(ClaimTypes.Role, "Customer")
  };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
          new AuthenticationProperties { IsPersistent = true });

            _logger.LogInformation("[AccountController] Customer auto-logged in after registration: {Email}", email);

            TempData["SuccessMessage"] = "مرحباً بك! تم إنشاء حسابك بنجاح";
            return RedirectToAction("Customer", "Dashboards");
        }

        // For Corporates: Show success and redirect to login (requires email verification)
        TempData["RegisterSuccess"] = "تم إنشاء الحساب بنجاح. يرجى التحقق من بريدك الإلكتروني وتسجيل الدخول";
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
        if (string.IsNullOrWhiteSpace(email))
            ModelState.AddModelError(nameof(email), "البريد الإلكتروني مطلوب");
        else if (!IsValidEmail(email))
            ModelState.AddModelError(nameof(email), "البريد الإلكتروني غير صالح");

        if (string.IsNullOrWhiteSpace(password))
            ModelState.AddModelError(nameof(password), "كلمة المرور مطلوبة");

        if (!ModelState.IsValid)
            return View();

        var (ok, err, user) = await _auth.ValidateUserAsync(email, password);

        // Handle tailor with incomplete profile
        if (!ok && err == "TAILOR_INCOMPLETE_PROFILE" && user != null)
        {
            _logger.LogWarning("[AccountController] Tailor {Email} attempted login without complete profile. Redirecting to complete profile page.", email);

            TempData["UserId"] = user.Id.ToString();
            TempData["UserEmail"] = user.Email ?? email;
            TempData["UserName"] = user.Email ?? email;
            TempData["InfoMessage"] = "يجب إكمال ملفك الشخصي وتقديم الأوراق الثبوتية antes de تسجيل الدخول";

            return RedirectToAction(nameof(CompleteTailorProfile));
        }

        if (!ok || user is null)
        {
            ModelState.AddModelError(string.Empty, err ?? "البريد الإلكتروني أو كلمة المرور غير صحيحة");
            return View();
        }

        // Get full name from profile based on role
        string fullName = user.Email ?? "مستخدم";
        var roleName = user.Role?.Name ?? string.Empty;

        if (!string.IsNullOrEmpty(roleName))
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
                case "corporate":
                    var corporate = await _unitOfWork.Corporates.GetByUserIdAsync(user.Id);
                    if (corporate != null)
                        fullName = corporate.ContactPerson ?? corporate.CompanyName ?? user.Email ?? "مستخدم";
                    break;
            }
        }

        // Build claims
        var claims = new List<Claim>
        {
   new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
       new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, fullName),
            new Claim("FullName", fullName)
        };

        if (!string.IsNullOrEmpty(roleName))
            claims.Add(new Claim(ClaimTypes.Role, roleName));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
    new AuthenticationProperties { IsPersistent = rememberMe });

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
    public async Task<IActionResult> CompleteTailorProfile()
    {
        // Check if coming from registration (unauthenticated)
        var userIdStr = TempData["UserId"]?.ToString();

        // Keep the data for the POST
        TempData.Keep("UserId");
        TempData.Keep("UserEmail");
        TempData.Keep("UserName");

        Guid userGuid;
        User? user = null;

        // Scenario 1: Unauthenticated tailor (just registered or login redirect)
        if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out userGuid))
        {
            user = await _unitOfWork.Users.GetByIdAsync(userGuid);
            if (user == null || user.Role?.Name?.ToLower() != "tailor")
            {
                TempData["ErrorMessage"] = "حساب غير صالح";
                return RedirectToAction(nameof(Register));
            }
        }
        // Scenario 2: Authenticated tailor (editing profile after login)
        else if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out userGuid))
                return Unauthorized();

            user = await _unitOfWork.Users.GetByIdAsync(userGuid);
            if (user == null)
                return NotFound();

            // Check if user is a tailor
            var roleName = User.FindFirstValue(ClaimTypes.Role);
            if (roleName?.ToLower() != "tailor")
            {
                TempData["ErrorMessage"] = "هذه الصفحة مخصصة للخياطين فقط";
                return RedirectToAction("Index", "Home");
            }
        }
        else
        {
            // No user ID and not authenticated - redirect to registration
            TempData["ErrorMessage"] = "جلسة غير صالحة. يرجى التسجيل مرة أخرى";
            return RedirectToAction(nameof(Register));
        }

        // CRITICAL: Check if profile already exists (evidence already provided)
        // This ensures ONE-TIME submission only
        var existingProfile = await _unitOfWork.Tailors.GetByUserIdAsync(user.Id);
        if (existingProfile != null)
        {
            _logger.LogWarning("[AccountController] Tailor {UserId} attempted to access complete profile page but already has profile. Redirecting to login.", user.Id);
            TempData["InfoMessage"] = "تم إكمال ملفك الشخصي بالفعل. يمكنك تسجيل الدخول الآن";
            return RedirectToAction(nameof(Login));
        }

        var model = new CompleteTailorProfileRequest
        {
            UserId = user.Id,
            Email = TempData["UserEmail"]?.ToString() ?? user.Email,
            FullName = TempData["UserName"]?.ToString() ?? user.Email
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
            if (user == null || user.Role?.Name?.ToLower() != "tailor")
            {
                ModelState.AddModelError(string.Empty, "حساب غير صالح");
                return View(model);
            }

            // CRITICAL: Check if profile already exists - BLOCK double submission
            var existingProfile = await _unitOfWork.Tailors.GetByUserIdAsync(model.UserId);
            if (existingProfile != null)
            {
                _logger.LogWarning("[AccountController] Tailor {UserId} attempted to submit profile but already has one. Blocking submission.", model.UserId);
                TempData["InfoMessage"] = "تم إكمال ملفك الشخصي بالفعل. لا يمكن التقديم مرة أخرى.";
                return RedirectToAction(nameof(Login));
            }

            // Validate that required evidence documents are provided
            if (model.IdDocument == null || model.IdDocument.Length == 0)
            {
                ModelState.AddModelError(nameof(model.IdDocument), "يجب تحميل صورة الهوية الشخصية");
                return View(model);
            }
            else
            {
                var (isValidId, idError) = ValidateFileUpload(model.IdDocument, "document");
                if (!isValidId)
                {
                    ModelState.AddModelError(nameof(model.IdDocument), idError!);
                    return View(model);
                }
            }

            // Validate portfolio images (at least 3)
            if ((model.PortfolioImages == null || !model.PortfolioImages.Any()) &&
           (model.WorkSamples == null || !model.WorkSamples.Any()))
            {
                ModelState.AddModelError(string.Empty, "يجب تحميل على الأقل 3 صور من أعمالك السابقة");
                return View(model);
            }
            else
            {
                var portfolioFiles = model.PortfolioImages ?? model.WorkSamples ?? new List<IFormFile>();
                if (portfolioFiles.Count < 3)
                {
                    ModelState.AddModelError(string.Empty, "يجب تحميل على الأقل 3 صور من معرض الأعمال");
                    return View(model);
                }

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
                ExperienceYears = model.ExperienceYears,
                IsVerified = false, // Awaiting admin approval
                CreatedAt = _dateTime.Now
            };

            // Store ID document as evidence
            if (model.IdDocument != null && model.IdDocument.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await model.IdDocument.CopyToAsync(memoryStream);
                tailorProfile.ProfilePictureData = memoryStream.ToArray();
                tailorProfile.ProfilePictureContentType = model.IdDocument.ContentType;
            }

            await _unitOfWork.Tailors.AddAsync(tailorProfile);

            // Save portfolio images
            var portfolioFilesToSave = model.PortfolioImages ?? model.WorkSamples ?? new List<IFormFile>();
            if (portfolioFilesToSave.Any())
            {
                var portfolioFolderPath = Path.Combine("wwwroot", "uploads", "portfolio", tailorProfile.Id.ToString());

                // Create directory if it doesn't exist (async-compatible approach)
                if (!Directory.Exists(portfolioFolderPath))
                {
                    Directory.CreateDirectory(portfolioFolderPath);
                }

                int imageIndex = 0;
                foreach (var image in portfolioFilesToSave.Take(10))
                {
                    if (image.Length > 0)
                    {
                        var fileName = $"portfolio_{_dateTime.Now.Ticks}_{imageIndex++}{Path.GetExtension(image.FileName)}";
                        var filePath = Path.Combine(portfolioFolderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                        {
                            await image.CopyToAsync(stream);
                        }

                        var relativeUrl = $"/uploads/portfolio/{tailorProfile.Id}/{fileName}";
                        var portfolioImage = new PortfolioImage
                        {
                            PortfolioImageId = Guid.NewGuid(),
                            TailorId = tailorProfile.Id,
                            ImageUrl = relativeUrl,
                            IsBeforeAfter = false,
                            UploadedAt = _dateTime.Now,
                            IsDeleted = false
                        };

                        await _unitOfWork.Context.Set<PortfolioImage>().AddAsync(portfolioImage);
                    }
                }
            }

            // Keep user INACTIVE until admin approves
            user.IsActive = false;
            user.UpdatedAt = _dateTime.Now;

            // Generate email verification token
            var verificationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
 .Replace("+", "").Replace("/", "").Replace("=", "").Substring(0,32);

            user.EmailVerificationToken = verificationToken;
            user.EmailVerificationTokenExpires = _dateTime.Now.AddHours(24);

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("[AccountController] Tailor {UserId} completed ONE-TIME profile submission. Awaiting admin review (IsActive=false).", model.UserId);

            TempData["RegisterSuccess"] = "تم إكمال ملفك الشخصي بنجاح! سيتم مراجعة طلبك من قبل الإدارة خلال24-48 ساعة. سنرسل لك إشعاراً عند الموافقة على حسابك.";
            return RedirectToAction(nameof(Login));
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

            // Try Corporate profile if not found
            if (imageData == null)
            {
                var corporateProfile = await _unitOfWork.Corporates.GetByUserIdAsync(id);
                if (corporateProfile?.ProfilePictureData != null)
                {
                    imageData = corporateProfile.ProfilePictureData;
                    contentType = corporateProfile.ProfilePictureContentType ?? "image/jpeg";
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

    #region Password Reset

    /// <summary>
    /// Displays the forgot password page.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    /// <summary>
    /// Handles POST for forgotten password. Sends a reset link if the email exists.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgottenPassword(string email)
    {
        email = SanitizeInput(email ?? string.Empty, 254)?.ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(nameof(email), "البريد الإلكتروني مطلوب");
            return View();
        }

        if (!IsValidEmail(email))
        {
            ModelState.AddModelError(nameof(email), "البريد الإلكتروني غير صالح");
            return View();
        }

        var user = await _unitOfWork.Users.GetByEmailAsync(email);

        // Security: Always show success message (don't reveal if email exists)
        if (user == null)
        {
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
            TempData["SuccessMessage"] = "إذا كان البريد الإلكتروني موجوداً في نظامنا، ستتلقى رسالة لإعادة تعيين كلمة المرور خلال بضع دقائق.";
            return View();
        }

        var resetToken = GeneratePasswordResetToken();
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpires = _dateTime.Now.AddHours(1);
        user.UpdatedAt = _dateTime.Now;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var resetLink = Url.Action(nameof(ResetPassword), "Account",
     new { token = resetToken }, Request.Scheme);
        _logger.LogInformation("Password reset link generated for {Email}: {Link}", email, resetLink);

        TempData["SuccessMessage"] = "إذا كان البريد الإلكتروني موجوداً في نظامنا، ستتلقى رسالة لإعادة تعيين كلمة المرور خلال بضع دقائق.";
        return View();
    }

    /// <summary>
    /// Displays the reset password page for a given token.
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
    /// Handles POST for resetting the user's password.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _unitOfWork.Context.Set<User>()
        .FirstOrDefaultAsync(u => u.PasswordResetToken == model.Token);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "رابط إعادة تعيين كلمة المرور غير صالح");
            return View(model);
        }

        if (user.PasswordResetTokenExpires == null || user.PasswordResetTokenExpires < _dateTime.Now)
        {
            ModelState.AddModelError(string.Empty, "انتهت صلاحية رابط إعادة تعيين كلمة المرور. يرجى طلب رابط جديد.");
            return View(model);
        }

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

    #endregion

    #region OAuth (Google/Facebook)

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
    /// Initiates Facebook OAuth login.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult FacebookLogin(string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(FacebookResponse), "Account", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, "Facebook");
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
    /// Handles Facebook OAuth response.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> FacebookResponse(string? returnUrl = null)
    {
        return await HandleOAuthResponse("Facebook", returnUrl);
    }

    /// <summary>
    /// Handles OAuth response for Google or Facebook.
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

            // Try to get profile picture from different claim types
            string? picture = null;

            if (provider == "Facebook")
            {
                picture = claims?.FirstOrDefault(c => c.Type == "urn:facebook:picture:url")?.Value
                ?? claims?.FirstOrDefault(c => c.Type == "urn:facebook:picture")?.Value
                    ?? claims?.FirstOrDefault(c => c.Type == "picture")?.Value;

                if (string.IsNullOrEmpty(picture) && !string.IsNullOrEmpty(providerId))
                    picture = $"https://graph.facebook.com/{providerId}/picture?type=large";
            }
            else if (provider == "Google")
            {
                picture = claims?.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value
                   ?? claims?.FirstOrDefault(c => c.Type == "picture")?.Value;
            }

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
                        case "corporate":
                            var corporate = await _unitOfWork.Corporates.GetByUserIdAsync(existingUser.Id);
                            if (corporate != null)
                                fullName = corporate.ContactPerson ?? corporate.CompanyName ?? existingUser.Email ?? "مستخدم";
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
                "corporate" => RegistrationRole.Corporate,
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
                    case "corporate":
                        var corporate = await _unitOfWork.Corporates.GetByUserIdAsync(user.Id);
                        if (corporate != null)
                            fullName = corporate.ContactPerson ?? corporate.CompanyName ?? fullName;
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
    private IActionResult RedirectToRoleDashboard(string? roleName)
    {
        return (roleName?.ToLowerInvariant()) switch
        {
            "tailor" => RedirectToAction("Tailor", "Dashboards"),
            "corporate" => RedirectToAction("Corporate", "Dashboards"),
            "admin" => RedirectToAction("Index", "Admin"),
            _ => RedirectToAction("Customer", "Dashboards")
        };
    }

    /// <summary>
    /// Validates the format of an email address.
    /// </summary>
    private bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email && email.Contains("@") && email.Length <= 254;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates password strength and returns a tuple indicating validity and error message.
    /// </summary>
    private (bool IsValid, string? Error) ValidatePasswordStrength(string password)
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

        var weakPasswords = new[] { "password1!", "qwerty123!", "admin123!", "welcome1!", "Password1!", "Qwerty123!" };
        if (weakPasswords.Any(weak => password.Equals(weak, StringComparison.OrdinalIgnoreCase)))
            return (false, "كلمة المرور ضعيفة جداً. يرجى اختيار كلمة مرور أقوى");

        return (true, null);
    }

    /// <summary>
    /// Validates the format and strength of a phone number.
    /// </summary>
    private (bool IsValid, string? Error) ValidatePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return (false, "رقم الهاتف مطلوب");

        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

        if (digitsOnly.Length < 9)
            return (false, "رقم الهاتف يجب أن يحتوي على 9 أرقام على الأقل");

        if (digitsOnly.StartsWith("000") || digitsOnly.StartsWith("111") || digitsOnly.StartsWith("222") ||
              digitsOnly.StartsWith("333") || digitsOnly.StartsWith("444") || digitsOnly.StartsWith("555") ||
               digitsOnly.StartsWith("666") || digitsOnly.StartsWith("777") || digitsOnly.StartsWith("888") ||
                  digitsOnly.StartsWith("999") || digitsOnly == new string('0', digitsOnly.Length) ||
                  digitsOnly == new string('1', digitsOnly.Length) || digitsOnly == new string('2', digitsOnly.Length))
        {
            return (false, "رقم الهاتف ضعيف جداً. يرجى اختيار رقم آخر");
        }

        return (true, null);
    }

    /// <summary>
    /// Validates uploaded files for type and size.
    /// </summary>
    private (bool IsValid, string? Error) ValidateFileUpload(IFormFile? file, string fileType = "image")
    {
        if (file == null || file.Length == 0)
            return (false, "الملف مطلوب");

        var maxSize = fileType == "image" ? 5 * 1024 * 1024 : 10 * 1024 * 1024;
        if (file.Length > maxSize)
            return (false, $"حجم الملف كبير جداً. الحد الأقصى {maxSize / (1024 * 1024)} ميجابايت");

        var allowedExtensions = fileType == "image"
      ? new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }
      : new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return (false, $"نوع الملف غير مدعوم. الأنواع المسموحة: {string.Join(", ", allowedExtensions)}");

        var allowedContentTypes = fileType == "image"
            ? new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" }
     : new[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    "image/jpeg", "image/png" };

        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return (false, "نوع محتوى الملف غير صحيح");

        var fileName = Path.GetFileName(file.FileName);
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
            return (false, "اسم الملف غير صالح");

        return (true, null);
    }

    /// <summary>
    /// Sanitizes user input to prevent XSS and SQL injection.
    /// </summary>
    private string SanitizeInput(string? input, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        input = input.Trim();
        input = System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);

        var sqlPatterns = new[] { "--", ";--", "';", "')", "' OR '", "' AND '", "DROP ", "INSERT ", "DELETE ", "UPDATE ", "EXEC " };
        foreach (var pattern in sqlPatterns)
            input = input.Replace(pattern, "", StringComparison.OrdinalIgnoreCase);

        if (input.Length > maxLength)
            input = input.Substring(0, maxLength);

        return input;
    }

    /// <summary>
    /// Generates a secure password reset token.
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
}
