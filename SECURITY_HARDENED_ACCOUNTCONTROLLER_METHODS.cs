// 🔒 SECURITY-HARDENED ACCOUNTCONTROLLER METHODS
// Replace existing methods with these implementations

using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Encodings.Web;

namespace TafsilkPlatform.Web.Controllers;

public partial class AccountController
{
    // ✅ ADD THESE DEPENDENCIES TO CONSTRUCTOR
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IFileValidationService _fileValidation;
    private readonly IServiceProvider _serviceProvider;
    private readonly HtmlEncoder _htmlEncoder;

    // ✅ SECURE TOKEN GENERATION METHOD
    /// <summary>
    /// Generates a cryptographically secure token for email verification
    /// </summary>
    /// <returns>URL-safe Base64 encoded token</returns>
    private string GenerateSecureVerificationToken()
    {
     var tokenBytes = new byte[32]; // 256 bits
    using (var rng = RandomNumberGenerator.Create())
        {
       rng.GetBytes(tokenBytes);
        }
        return WebEncoders.Base64UrlEncode(tokenBytes);
    }

    // ✅ SECURE LOGIN WITH AUTHENTICATION CHECK AND RATE LIMITING
    /// <summary>
  /// User login endpoint with rate limiting and authentication check
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
   // Check if user is already authenticated
        if (User.Identity?.IsAuthenticated == true)
      {
            var roleName = User.FindFirstValue(ClaimTypes.Role);
 _logger.LogInformation(
    "[AccountController] Authenticated user {Email} attempted to access Login. Redirecting to dashboard.",
     User.FindFirstValue(ClaimTypes.Email));
            TempData["InfoMessage"] = "أنت مسجل دخول بالفعل.";
            return RedirectToRoleDashboard(roleName);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")] // ✅ RATE LIMITING
    public async Task<IActionResult> Login(
    string email,
        string password,
        bool rememberMe = false,
  string? returnUrl = null)
    {
        // Check if user is already authenticated
        if (User.Identity?.IsAuthenticated == true)
        {
            var currentRole = User.FindFirstValue(ClaimTypes.Role);
    _logger.LogWarning(
 "[AccountController] Authenticated user {Email} attempted to POST Login. Blocking.",
User.FindFirstValue(ClaimTypes.Email));
            TempData["ErrorMessage"] = "أنت مسجل دخول بالفعل. يرجى تسجيل الخروج أولاً إذا كنت تريد تسجيل الدخول بحساب آخر.";
            return RedirectToRoleDashboard(currentRole);
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
{
          ModelState.AddModelError(string.Empty, "يرجى إدخال البريد وكلمة المرور");
         return View();
   }

   // ✅ Validate credentials (includes account lockout check in AuthService)
        var (ok, err, user) = await _auth.ValidateUserAsync(email, password);
     if (!ok || user is null)
        {
 ModelState.AddModelError(string.Empty, err ?? "بيانات اعتماد غير صحيحة");
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

  // Build claims with role and full name
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
  new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
      new Claim(ClaimTypes.Name, fullName),
            new Claim("FullName", fullName)
        };

      if (!string.IsNullOrEmpty(roleName))
        {
         claims.Add(new Claim(ClaimTypes.Role, roleName));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
   var principal = new ClaimsPrincipal(identity);

 await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
principal,
         new AuthenticationProperties
          {
         IsPersistent = rememberMe
         });

        _logger.LogInformation("[AccountController] User {Email} logged in successfully", email);

        // Redirect to role-specific dashboard or return URL
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
}

        return RedirectToRoleDashboard(roleName);
    }

    // ✅ SECURE REGISTER WITH AUTHENTICATION CHECK AND RATE LIMITING
    [HttpGet]
    [AllowAnonymous]
  public IActionResult Register()
    {
        // If user is already authenticated, redirect to their dashboard
        if (User.Identity?.IsAuthenticated == true)
    {
        var roleName = User.FindFirstValue(ClaimTypes.Role);
      _logger.LogInformation(
  "[AccountController] Authenticated user {Email} attempted to access Register. Redirecting to dashboard.",
           User.FindFirstValue(ClaimTypes.Email));
 TempData["InfoMessage"] = "أنت مسجل دخول بالفعل. يرجى تسجيل الخروج أولاً إذا كنت تريد إنشاء حساب جديد.";
  return RedirectToRoleDashboard(roleName);
        }

        return View();
    }

    [HttpPost]
 [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")] // ✅ RATE LIMITING
    public async Task<IActionResult> Register(
    string name,
        string email,
      string password,
    string userType,
   string? phoneNumber)
    {
     // If user is already authenticated, redirect to their dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
  var roleName = User.FindFirstValue(ClaimTypes.Role);
    _logger.LogWarning(
        "[AccountController] Authenticated user {Email} attempted to POST Register. Blocking.",
         User.FindFirstValue(ClaimTypes.Email));
 TempData["ErrorMessage"] = "أنت مسجل دخول بالفعل. لا يمكنك إنشاء حساب جديد أثناء تسجيل الدخول.";
     return RedirectToRoleDashboard(roleName);
        }

        // ✅ Sanitize inputs
name = _htmlEncoder.Encode(name ?? "");
      email = email?.Trim().ToLowerInvariant() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError(nameof(name), "الاسم الكامل مطلوب");
        }
if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
 {
 ModelState.AddModelError(string.Empty, "بيانات غير صالحة");
        }
      if (!ModelState.IsValid)
        {
  return View();
}

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
            ModelState.AddModelError(string.Empty, err ?? "فشل التسجيل");
     return View();
}

        // Special handling for Tailors: Must provide evidence BEFORE login
        if (role == RegistrationRole.Tailor)
        {
  TempData["UserId"] = user.Id.ToString();
      TempData["UserEmail"] = email;
     TempData["UserName"] = name;
    TempData["InfoMessage"] = "تم إنشاء حسابك بنجاح! يجب تقديم الأوراق الثبوتية لإكمال التسجيل";
            return RedirectToAction(nameof(ProvideTailorEvidence));
        }

        // For Customers and Corporates: Show success and redirect to login
        TempData["RegisterSuccess"] = "تم إنشاء الحساب بنجاح. يرجى التحقق من بريدك الإلكتروني وتسجيل الدخول";
        return RedirectToAction("Login");
    }

    // ✅ SECURE EVIDENCE SUBMISSION WITH FILE VALIDATION
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("fileupload")] // ✅ RATE LIMITING
    public async Task<IActionResult> ProvideTailorEvidence(CompleteTailorProfileRequest model)
    {
        if (!ModelState.IsValid)
      {
 return View(model);
        }

    try
        {
            // Get the user
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
                _logger.LogWarning(
       "[AccountController] Tailor {UserId} attempted to submit evidence but already has profile. Blocking submission.",
  model.UserId);
  TempData["InfoMessage"] = "تم تقديم الأوراق الثبوتية بالفعل. لا يمكن التقديم مرة أخرى.";
          return RedirectToAction(nameof(Login));
          }

     // ✅ VALIDATE ID DOCUMENT
 if (model.IdDocument == null || model.IdDocument.Length == 0)
            {
    ModelState.AddModelError(nameof(model.IdDocument), "يجب تحميل صورة الهوية الشخصية");
      return View(model);
       }

            var (isValidId, idError) = await _fileValidation.ValidateDocumentAsync(model.IdDocument);
    if (!isValidId)
   {
             _logger.LogWarning("[AccountController] Invalid ID document upload attempt by user {UserId}", model.UserId);
 ModelState.AddModelError(nameof(model.IdDocument), idError ?? "ملف الهوية غير صالح");
 return View(model);
   }

        // ✅ VALIDATE PORTFOLIO IMAGES
            if ((model.PortfolioImages == null || !model.PortfolioImages.Any()) &&
       (model.WorkSamples == null || !model.WorkSamples.Any()))
    {
       ModelState.AddModelError(string.Empty, "يجب تحميل على الأقل صورة واحدة من أعمالك السابقة");
       return View(model);
            }

 var portfolioImagesToProcess = model.PortfolioImages ?? model.WorkSamples ?? new List<IFormFile>();
    foreach (var image in portfolioImagesToProcess)
         {
     var (isValidImg, imgError) = await _fileValidation.ValidateImageAsync(image);
   if (!isValidImg)
          {
        _logger.LogWarning("[AccountController] Invalid portfolio image upload attempt by user {UserId}", model.UserId);
            ModelState.AddModelError(nameof(model.PortfolioImages), imgError ?? "صورة غير صالحة");
           return View(model);
      }
     }

   // ✅ Sanitize text inputs
            var sanitizedFullName = _htmlEncoder.Encode(model.FullName);
var sanitizedWorkshopName = _htmlEncoder.Encode(model.WorkshopName);
            var sanitizedAddress = _htmlEncoder.Encode(model.Address);
     var sanitizedCity = _htmlEncoder.Encode(model.City ?? "");
       var sanitizedDescription = _htmlEncoder.Encode(model.Description);

    // Create tailor profile NOW (after evidence is provided)
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
   using (var memoryStream = new MemoryStream())
            {
          await model.IdDocument.CopyToAsync(memoryStream);
    tailorProfile.ProfilePictureData = memoryStream.ToArray();
       tailorProfile.ProfilePictureContentType = model.IdDocument.ContentType;
      }

            await _unitOfWork.Tailors.AddAsync(tailorProfile);

    // ✅ Save portfolio images OUTSIDE wwwroot
            var portfolioFolderPath = Path.Combine(
         Directory.GetCurrentDirectory(),
        "App_Data", // ✅ Outside wwwroot
     "portfolios",
     tailorProfile.Id.ToString());
         Directory.CreateDirectory(portfolioFolderPath);

   int imageIndex = 0;
            foreach (var image in portfolioImagesToProcess.Take(10))
  {
    if (image.Length > 0)
      {
         // ✅ Generate secure filename
       var safeFileName = _fileValidation.GenerateSafeFileName(image.FileName);
 var filePath = Path.Combine(portfolioFolderPath, safeFileName);

         using (var stream = new FileStream(filePath, FileMode.Create))
      {
             await image.CopyToAsync(stream);
           }

         var relativeUrl = $"/api/portfolio/{tailorProfile.Id}/{safeFileName}";
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
    imageIndex++;
       }
       }

     // NOW activate the user and send email verification
        user.IsActive = true;
            user.UpdatedAt = _dateTime.Now;

  // ✅ Generate SECURE verification token
            var verificationToken = GenerateSecureVerificationToken();

            user.EmailVerificationToken = verificationToken;
            user.EmailVerificationTokenExpires = _dateTime.Now.AddHours(24);

   await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
      "[AccountController] Tailor {UserId} completed ONE-TIME evidence submission. Profile created, user activated.",
            model.UserId);

            // ✅ Send email verification via BACKGROUND QUEUE
        await _taskQueue.QueueBackgroundWorkItemAsync(async token =>
  {
  using var scope = _serviceProvider.CreateScope();
       var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
     var logger = scope.ServiceProvider.GetRequiredService<ILogger<AccountController>>();

    try
           {
   await emailService.SendEmailVerificationAsync(
        user.Email,
      sanitizedFullName,
      verificationToken);
    logger.LogInformation("[Background] Email verification sent to tailor: {Email}", user.Email);
      }
 catch (Exception ex)
        {
  logger.LogError(ex, "[Background] Failed to send verification email to {Email}", user.Email);
              }
      });

     TempData["RegisterSuccess"] = "تم إكمال التسجيل بنجاح! تم إرسال رابط تأكيد البريد الإلكتروني. يمكنك الآن تسجيل الدخول وستتم مراجعة طلبك خلال 24-48 ساعة.";
  return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AccountController] Error providing tailor evidence for user: {UserId}", model.UserId);
        ModelState.AddModelError(string.Empty, "حدث خطأ أثناء حفظ البيانات. يرجى المحاولة مرة أخرى.");
            return View(model);
        }
    }

    // ✅ RESEND EMAIL WITH RATE LIMITING
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("email")] // ✅ RATE LIMITING
    public async Task<IActionResult> ResendVerificationEmail(string email)
    {
if (string.IsNullOrWhiteSpace(email))
        {
    ModelState.AddModelError(nameof(email), "البريد الإلكتروني مطلوب");
     return View();
        }

        var (success, error) = await _auth.ResendVerificationEmailAsync(email);

        if (success)
{
            TempData["RegisterSuccess"] = "تم إرسال رسالة التحقق بنجاح! يرجى التحقق من بريدك الإلكتروني";
        }
        else
        {
 TempData["ErrorMessage"] = error ?? "فشل إرسال رسالة التحقق";
     }

        return View();
    }
}
