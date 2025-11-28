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
    IDateTimeService dateTime,
    Microsoft.AspNetCore.Identity.UI.Services.IEmailSender emailSender) : Controller
{
    private readonly IAuthService _auth = auth;
    private readonly TafsilkPlatform.DataAccess.Repository.IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IFileUploadService _fileUploadService = fileUploadService;
    private readonly ILogger<AccountController> _logger = logger;
    private readonly IDateTimeService _dateTime = dateTime;
    private readonly Microsoft.AspNetCore.Identity.UI.Services.IEmailSender _emailSender = emailSender;

    #region Password Management

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userRepository.GetByEmailAsync(model.Email);
        if (user == null || user.IsDeleted)
        {
            ModelState.AddModelError(nameof(model.Email), "There is no account registered with this email address.");
            return View(model);
        }

        // Generate token
        var token = Guid.NewGuid().ToString("N");
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1); // 1 hour expiration
        await _unitOfWork.SaveChangesAsync();

        // Send email
        var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { token, email = user.Email }, Request.Scheme);
        
        var message = $@"
            <h3>Reset your password</h3>
            <p>Please reset your password by clicking here: <a href='{callbackUrl}'>Reset Password</a></p>
            <p>This link will expire in 1 hour.</p>";

        await _emailSender.SendEmailAsync(model.Email, "Reset Password", message);

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string token, string email)
    {
        if (token == null || email == null)
        {
            ModelState.AddModelError("", "Invalid password reset token");
        }
        return View(new ResetPasswordViewModel { Token = token, Email = email });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userRepository.GetByEmailAsync(model.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        // Verify token
        if (user.PasswordResetToken != model.Token || 
            !user.PasswordResetTokenExpires.HasValue || 
            user.PasswordResetTokenExpires.Value < DateTime.UtcNow)
        {
            ModelState.AddModelError("", "Invalid or expired password reset token");
            return View(model);
        }

        // Reset password
        var hash = PasswordHasher.Hash(model.NewPassword);
        user.PasswordHash = hash;
        
        // Clear token
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpires = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return RedirectToAction(nameof(ResetPasswordConfirmation));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    #endregion

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
            TempData["InfoMessage"] = "You're already logged in. Please log out first to create a new account.";
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
            TempData["ErrorMessage"] = "You're already logged in. Please log out first.";
            return RedirectToRoleDashboard(roleName);
        }

        // Sanitize inputs
        name = SanitizeInput(name ?? string.Empty, 100);
        email = SanitizeInput(email ?? string.Empty, 254)?.ToLowerInvariant() ?? string.Empty;

        // Validate name
        if (string.IsNullOrWhiteSpace(name))
            ModelState.AddModelError(nameof(name), "Please enter your full name.");
        else if (name.Length < 2)
            ModelState.AddModelError(nameof(name), "Name must be at least 2 characters long.");
        else if (name.Length > 100)
            ModelState.AddModelError(nameof(name), "Name is too long (maximum 100 characters).");

        // Validate email
        if (string.IsNullOrWhiteSpace(email))
            ModelState.AddModelError(nameof(email), "Please enter your email address.");
        else if (!IsValidEmail(email))
            ModelState.AddModelError(nameof(email), "Please enter a valid email address.");

        // Validate password
        if (string.IsNullOrWhiteSpace(password))
            ModelState.AddModelError(nameof(password), "Please enter a password.");
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
            ModelState.AddModelError(string.Empty, err ?? "Registration failed. Please try again.");
            return View();
        }

        _logger.LogInformation("[AccountController] User registered successfully: {Email}, Role: {Role}", email, role);

        // For Tailors: Redirect to CompleteTailorProfile (better UX wizard)
        if (role == RegistrationRole.Tailor)
        {
            TempData["UserId"] = user.Id.ToString();
            TempData["UserEmail"] = email;
            TempData["UserName"] = name;
            TempData["InfoMessage"] = "Account created! Please complete your profile to get started.";
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

            TempData["SuccessMessage"] = "Welcome! Your account has been created.";
            return RedirectToAction("Index", "Store", new { area = "Customer" });
        }

        // Default fallback (should not reach here)
        TempData["SuccessMessage"] = "Account created successfully!";
        return RedirectToAction("Login");
    }

    /// <summary>
    /// Displays the login page.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // If user is already authenticated, redirect to their dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            var roleName = User.FindFirstValue(ClaimTypes.Role);
            _logger.LogInformation("[AccountController] Authenticated user {Email} attempted to access Login. Redirecting to dashboard.",
                User.FindFirstValue(ClaimTypes.Email));
            TempData["InfoMessage"] = "You're already logged in.";
            return RedirectToRoleDashboard(roleName);
        }

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
            ModelState.AddModelError(nameof(email), "Please enter your email address.");
        else if (!IsValidEmail(email))
            ModelState.AddModelError(nameof(email), "Please enter a valid email address.");

        if (string.IsNullOrWhiteSpace(password))
            ModelState.AddModelError(nameof(password), "Please enter your password.");

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
            TempData["InfoMessage"] = "Please complete your profile to continue.";

            return RedirectToAction(nameof(CompleteTailorProfile), new { userId = user.Id });
        }

        if (!ok || user is null)
        {
            ModelState.AddModelError(string.Empty, err ?? "Incorrect email or password.");
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
                TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
                return RedirectToAction(nameof(Login));
            }

            _logger.LogInformation("[AccountController] Using UserId from claims: {UserId}", userGuid);
        }
        else
        {
            // No user ID and not authenticated - redirect to registration
            _logger.LogWarning("[AccountController] No UserId available from any source");
            TempData["InfoMessage"] = "Please register to create a tailor account.";
            return RedirectToAction(nameof(Register));
        }

        // ✅ FIX: Use GetUserWithProfileAsync which includes Role navigation property
        user = await _userRepository.GetUserWithProfileAsync(userGuid);

        // ✅ FIX: More detailed validation and better error messages
        if (user == null)
        {
            _logger.LogWarning("[AccountController] User not found: {UserId}", userGuid);
            TempData["ErrorMessage"] = "We couldn't find your account. Please register.";
            return RedirectToAction(nameof(Register));
        }

        if (user.Role == null)
        {
            _logger.LogError("[AccountController] User {UserId} has no role assigned. Email: {Email}", userGuid, user.Email);
            TempData["ErrorMessage"] = "There was a problem with your account. Please contact support.";
            return RedirectToAction(nameof(Register));
        }

        if (user.Role.Name?.ToLower() != "tailor")
        {
            _logger.LogWarning("[AccountController] User {UserId} is not a tailor. Role: {Role}", userGuid, user.Role.Name);
            TempData["ErrorMessage"] = $"This account is registered as a {user.Role.Name}. Please log in.";
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
            TempData["SuccessMessage"] = "Your profile is already complete. Welcome back!";

            // ✅ FIX: If profile exists, auto-login and redirect to dashboard
            if (User.Identity?.IsAuthenticated != true)
            {
                // Build claims for authentication
                var claims = new List<Claim>
  {
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
          new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
      new Claim(ClaimTypes.Name, existingProfile.FullName ?? user.Email ?? "Tailor"),
         new Claim("FullName", existingProfile.FullName ?? user.Email ?? "Tailor"),
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
            FullName = user.Email ?? "New Tailor"
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
                ModelState.AddModelError(string.Empty, "We couldn't find your user account.");
                return View(model);
            }

            // ✅ FIX: Load role navigation property
            await _unitOfWork.Context.Entry(user).Reference(u => u.Role).LoadAsync();

            if (user.Role == null || user.Role.Name?.ToLower() != "tailor")
            {
                _logger.LogWarning("[AccountController] Invalid user or not a tailor: {UserId}, Role: {Role}",
                  model.UserId, user.Role?.Name ?? "NULL");
                ModelState.AddModelError(string.Empty, "Invalid account type.");
                return View(model);
            }

            // CRITICAL: Check if profile already exists - BLOCK double submission
            var existingProfile = await _unitOfWork.Tailors.GetByUserIdAsync(model.UserId);
            if (existingProfile != null)
            {
                _logger.LogWarning("[AccountController] Tailor {UserId} attempted to submit profile but already has one. Blocking submission.", model.UserId);
                TempData["InfoMessage"] = "Your profile is already complete.";
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
                    ModelState.AddModelError(string.Empty, "You can only upload up to 10 images.");
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
                Specialization = SanitizeInput(model.WorkshopType, 50) ?? "General Tailoring",
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
            // ✅ FIX: Auto-login the tailor and redirect to their dashboard
            // Build claims for authentication using AuthService to ensure consistency
            var claims = await _auth.GetUserClaimsAsync(user);

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true });

            _logger.LogInformation("[AccountController] Tailor {UserId} auto-logged in after profile completion.", model.UserId);

            TempData["SuccessMessage"] = "Profile completed! Welcome to Tafsilk.";
            return RedirectToAction("Tailor", "Dashboards");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AccountController] Error completing tailor profile for user: {UserId}", model.UserId);
            ModelState.AddModelError(string.Empty, "We couldn't save your profile. Please try again.");
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

            // Try Tailor profile
            var tailorProfile = await _unitOfWork.Tailors.GetByUserIdAsync(id);
            if (tailorProfile?.ProfilePictureData != null)
            {
                imageData = tailorProfile.ProfilePictureData;
                contentType = tailorProfile.ProfilePictureContentType ?? "image/jpeg";
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
            ModelState.AddModelError(nameof(model.CurrentPassword), "The current password you entered is incorrect.");
            return View(model);
        }

        // Update to new password
        user.PasswordHash = PasswordHasher.Hash(model.NewPassword);
        user.UpdatedAt = _dateTime.Now;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "Your password has been changed.";

        return RedirectToAction(nameof(ChangePassword));
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
            TempData["ErrorMessage"] = "This verification link is invalid or has expired.";
            return RedirectToAction("Login");
        }

        var (success, error) = await _auth.VerifyEmailAsync(token);

        if (success)
            TempData["RegisterSuccess"] = "Email verified! You can now log in.";
        else
            TempData["ErrorMessage"] = error ?? "Email verification failed. Please try again.";

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
            ModelState.AddModelError(nameof(email), "Please enter your email address.");
            return View();
        }

        var (success, error) = await _auth.ResendVerificationEmailAsync(email);

        if (success)
            TempData["RegisterSuccess"] = "Verification email sent! Please check your inbox.";
        else
            TempData["ErrorMessage"] = error ?? "We couldn't send the verification email. Please try again.";

        return View();
    }

    #endregion

    #region OAuth (Google)

    /// <summary>
    /// Initiates Google OAuth login.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin(string? returnUrl = null)
    {
        // Check if Google authentication handler is registered
        var schemes = await HttpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>().GetAllSchemesAsync();
        var googleScheme = schemes.FirstOrDefault(s => s.Name == "Google");

        if (googleScheme == null)
        {
            TempData["ErrorMessage"] = "Google login is unavailable. Please use your email and password.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

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
                var failureMessage = authenticateResult.Failure?.Message;
                _logger.LogWarning("OAuth authentication failed for provider {Provider}. Error: {Error}", provider, failureMessage);

                if (failureMessage != null && (failureMessage.Contains("Access was denied") || failureMessage.Contains("canceled")))
                {
                    TempData["InfoMessage"] = "Login cancelled.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"We couldn't log you in with {provider}.";
                }

                return RedirectToAction(nameof(Login), new { returnUrl });
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
                TempData["ErrorMessage"] = $"We couldn't get your email from {provider}";
                return RedirectToAction(nameof(Login));
            }
            // Check if user exists
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(email);

            if (existingUser != null)
            {
                // User exists, sign them in
                string fullName = existingUser.Email ?? "User";
                var roleName = existingUser.Role?.Name ?? string.Empty;

                // ✅ CHECK: If user has no role, force them to complete registration
                if (string.IsNullOrEmpty(roleName))
                {
                    TempData["OAuthProvider"] = provider;
                    TempData["OAuthEmail"] = email;
                    TempData["OAuthName"] = name ?? string.Empty;
                    TempData["OAuthPicture"] = picture ?? string.Empty;
                    TempData["OAuthId"] = providerId ?? string.Empty;

                    return RedirectToAction(nameof(CompleteSocialRegistration));
                }

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
            TempData["ErrorMessage"] = "We encountered an error while logging you in. Please try again.";
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

            User user = null;
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(email);

            if (existingUser != null)
            {
                // Update existing user
                var roleNameStr = role == RegistrationRole.Tailor ? "Tailor" : "Customer";
                var roleEntity = await _unitOfWork.Context.Roles.FirstOrDefaultAsync(r => r.Name == roleNameStr);

                if (roleEntity != null)
                {
                    existingUser.Role = roleEntity;
                    existingUser.PhoneNumber = model.PhoneNumber;
                    existingUser.UpdatedAt = _dateTime.Now;

                    await _unitOfWork.Users.UpdateAsync(existingUser);

                    // Update/Create Profile
                    if (role == RegistrationRole.Customer)
                    {
                        var customer = await _unitOfWork.Customers.GetByUserIdAsync(existingUser.Id);
                        if (customer == null)
                        {
                            customer = new CustomerProfile
                            {
                                UserId = existingUser.Id,
                                FullName = model.FullName,
                                CreatedAt = _dateTime.Now
                            };
                            await _unitOfWork.Customers.AddAsync(customer);
                        }
                        else
                        {
                            customer.FullName = model.FullName;
                            await _unitOfWork.Customers.UpdateAsync(customer);
                        }
                    }
                    else if (role == RegistrationRole.Tailor)
                    {
                        var tailor = await _unitOfWork.Tailors.GetByUserIdAsync(existingUser.Id);
                        if (tailor != null)
                        {
                            tailor.FullName = model.FullName;
                            await _unitOfWork.Tailors.UpdateAsync(tailor);
                        }
                        else
                        {
                            // Defer profile creation to CompleteTailorProfile page
                            // Ensure user is inactive until they complete the profile
                            existingUser.IsActive = false;
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                }

                user = existingUser;
            }
            else
            {
                // Create registration request
                var registerRequest = new RegisterRequest
                {
                    Email = email,
                    Password = Guid.NewGuid().ToString(), // Generate random password for OAuth users
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Role = role
                };

                var (ok, err, newUser) = await _auth.RegisterAsync(registerRequest);

                if (!ok || newUser == null)
                {
                    ModelState.AddModelError(string.Empty, err ?? "Failed to create account");
                    ViewData["Provider"] = provider;
                    return View("CompleteGoogleRegistration", model);
                }
                user = newUser;
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

            // Redirect based on role
            if (role == RegistrationRole.Tailor)
            {
                // Tailors need to complete their profile
                TempData["InfoMessage"] = "Please complete your profile to start using the platform.";
                _logger.LogInformation("[AccountController] New tailor {UserId} from Google OAuth redirected to complete profile", user.Id);

                return RedirectToAction("CompleteTailorProfile", new { userId = user.Id });
            }
            else
            {
                // Customers can go directly to their dashboard
                TempData["SuccessMessage"] = "Welcome! Your account has been created via Google.";
                _logger.LogInformation("[AccountController] New customer {UserId} from Google OAuth redirected to dashboard", user.Id);

                return RedirectToAction("Customer", "Dashboards");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
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
            ModelState.AddModelError(string.Empty, "You cannot switch from a Tailor account to a Customer account directly. Please contact support.");
            return View(model);
        }

        // Allow Customer -> Tailor conversion
        if (model.TargetRole == "Tailor" && currentRole == "Customer")
        {
            if (string.IsNullOrWhiteSpace(model.ShopName) || string.IsNullOrWhiteSpace(model.Address))
            {
                ModelState.AddModelError(string.Empty, "Please provide your shop name and address.");
                return View(model);
            }

            var tailorRole = await _unitOfWork.Context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Tailor");
            if (tailorRole == null)
            {
                ModelState.AddModelError(string.Empty, "System error: Tailor role not found.");
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
            TempData["RegisterSuccess"] = "Your account has been upgraded to a Tailor account! Please log in again.";
            return RedirectToAction("Login");
        }

        TempData["ErrorMessage"] = "Role changes are not currently supported.";
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
            "customer" => RedirectToAction("Index", "Store", new { area = "Customer" }),
            _ => RedirectToAction("Index", "Store", new { area = "Customer" })
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
            return (false, "Password is required");

        if (password.Length < 8)
            return (false, "Password must be at least 8 characters");

        if (password.Length > 128)
            return (false, "Password is too long");

        if (!password.Any(char.IsUpper))
            return (false, "Password must contain at least one uppercase letter");

        if (!password.Any(char.IsLower))
            return (false, "Password must contain at least one lowercase letter");

        if (!password.Any(char.IsDigit))
            return (false, "Password must contain at least one number");

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            return (false, "Password must contain at least one special character");

        string[] weakPasswords = ["password1!", "qwerty123!", "admin123!", "welcome1!", "Password1!", "Qwerty123!"];
        if (weakPasswords.Any(weak => password.Equals(weak, StringComparison.OrdinalIgnoreCase)))
            return (false, "Password is too weak. Please choose a stronger password");

        return (true, null);
    }

    /// <summary>
    /// Validates the format and strength of a phone number.
    /// </summary>
    private static (bool IsValid, string? Error) ValidatePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return (false, "Phone number is required");

        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

        if (digitsOnly.Length < 9)
            return (false, "Phone number must contain at least 9 digits");

        string[] invalidPrefixes = ["000", "111", "222", "333", "444", "555", "666", "777", "888", "999"];
        if (invalidPrefixes.Any(digitsOnly.StartsWith) ||
           digitsOnly == new string('0', digitsOnly.Length) ||
               digitsOnly == new string('1', digitsOnly.Length) ||
          digitsOnly == new string('2', digitsOnly.Length))
        {
            return (false, "Phone number is too weak. Please choose another number");
        }

        return (true, null);
    }

    /// <summary>
    /// Validates uploaded files for type and size.
    /// </summary>
    private static (bool IsValid, string? Error) ValidateFileUpload(IFormFile? file, string fileType = "image")
    {
        if (file is null || file.Length == 0)
            return (false, "File is required");

        var maxSize = fileType == "image" ? 5 * 1024 * 1024 : 10 * 1024 * 1024;
        if (file.Length > maxSize)
            return (false, $"File size is too large. Maximum {maxSize / (1024 * 1024)} MB");

        string[] allowedExtensions = fileType == "image"
      ? [".jpg", ".jpeg", ".png", ".gif", ".webp"]
      : [".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png"];

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return (false, $"File type not supported. Allowed types: {string.Join(", ", allowedExtensions)}");

        string[] allowedContentTypes = fileType == "image"
                  ? ["image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"]
                  : ["application/pdf", "application/msword",
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
      "image/jpeg", "image/png"];

        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return (false, "Invalid file content type");

        var fileName = Path.GetFileName(file.FileName);
        if (fileName.Contains("..") || fileName.Contains('/') || fileName.Contains('\\'))
            return (false, "Invalid file name");

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
