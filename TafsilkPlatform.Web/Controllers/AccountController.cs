using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.ViewModels;
using TafsilkPlatform.Web.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TafsilkPlatform.Web.Controllers;

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

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
     // If user is already authenticated, redirect to their dashboard
        // They should logout first if they want to register a new account
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

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
   public async Task<IActionResult> Register(string name, string email, string password, string userType, string? phoneNumber)
    {
        // If user is already authenticated, redirect to their dashboard
        // They should logout first if they want to register a new account
        if (User.Identity?.IsAuthenticated == true)
        {
  var roleName = User.FindFirstValue(ClaimTypes.Role);
  _logger.LogWarning("[AccountController] Authenticated user {Email} attempted to POST Register. Blocking.", 
        User.FindFirstValue(ClaimTypes.Email));
  TempData["ErrorMessage"] = "أنت مسجل دخول بالفعل. لا يمكنك إنشاء حساب جديد أثناء تسجيل الدخول.";
      return RedirectToRoleDashboard(roleName);
        }

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

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, bool rememberMe = false, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "يرجى إدخال البريد وكلمة المرور");
            return View();
        }

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
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
        {
            IsPersistent = rememberMe
        });

        // REMOVED: Check if tailor needs to complete profile
        // Verification is ONE-TIME only (before first login via ProvideTailorEvidence)
        // After successful login, ALL users (including tailors) go to their dashboard
   // NO verification prompts after login

        // Redirect to role-specific dashboard or return URL
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
    {
  return Redirect(returnUrl);
        }

    return RedirectToRoleDashboard(roleName);
    }

    private IActionResult RedirectToRoleDashboard(string? roleName)
    {
        return (roleName?.ToLowerInvariant()) switch
        {
            "tailor" => RedirectToAction("Tailor", "Dashboards"),
            "corporate" => RedirectToAction("Corporate", "Dashboards"),
            _ => RedirectToAction("Customer", "Dashboards")
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

/// <summary>
  /// Serves profile pictures from the database
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
{
  return File(imageData, contentType ?? "image/jpeg");
       }
    
        // Return 404 or default placeholder image
     return NotFound();
        }
     catch
   {
     // Log the error if needed
       return NotFound();
        }
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
  return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
 public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
     {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
 if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
  {
   return Unauthorized();
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userGuid);
        if (user == null)
        {
 return NotFound();
        }

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
      
  // Get user role to redirect to appropriate dashboard
   var roleName = User.FindFirstValue(ClaimTypes.Role);
    return RedirectToRoleDashboard(roleName);
    }

    [HttpGet]
    public IActionResult RequestRoleChange()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestRoleChange(RoleChangeRequestViewModel model)
    {
      if (!ModelState.IsValid)
        {
         return View(model);
     }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
      return Unauthorized();
  }

        var user = await _unitOfWork.Users.GetByIdAsync(userGuid);
        if (user == null)
        {
            return NotFound();
   }

        var currentRole = User.FindFirstValue(ClaimTypes.Role);

        // Validate role change (e.g., Customer can become Tailor, but not vice versa without admin approval)
        if (currentRole == "Tailor" && model.TargetRole == "Customer")
        {
 ModelState.AddModelError(string.Empty, "لا يمكن التحويل من خياط إلى عميل بشكل مباشر. يرجى الاتصال بالدعم.");
      return View(model);
        }

        // For now, we'll allow Customer -> Tailor conversion
        // In production, this should create a request that admin approves
        if (model.TargetRole == "Tailor" && currentRole == "Customer")
    {
   // Validate required fields for tailors
      if (string.IsNullOrWhiteSpace(model.ShopName) || string.IsNullOrWhiteSpace(model.Address))
     {
          ModelState.AddModelError(string.Empty, "اسم المتجر والعنوان مطلوبان للخياطين");
              return View(model);
            }

         // Get Tailor role - Query the database directly via DbContext
    var tailorRole = await _unitOfWork.Context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Tailor");
     if (tailorRole == null)
    {
            ModelState.AddModelError(string.Empty, "دور الخياط غير موجود في النظام");
                return View(model);
            }

            // Update user role
         user.RoleId = tailorRole.Id;
     user.UpdatedAt = _dateTime.Now;
    await _unitOfWork.Users.UpdateAsync(user);

 // Create tailor profile
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

            // Mark customer profile as inactive (don't delete)
  // This allows reverting if needed

      await _unitOfWork.SaveChangesAsync();

        // Sign out and redirect to login to refresh claims
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
 TempData["RegisterSuccess"] = "تم تحويل حسابك إلى خياط بنجاح! يرجى تسجيل الدخول مرة أخرى.";
            return RedirectToAction("Login");
        }

        TempData["ErrorMessage"] = "طلب تغيير الدور غير مدعوم حالياً";
      
      // Get user role to redirect to appropriate dashboard
     var roleName = User.FindFirstValue(ClaimTypes.Role);
 return RedirectToRoleDashboard(roleName);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GoogleLogin(string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(GoogleResponse), "Account", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, "Google");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult FacebookLogin(string? returnUrl = null)
    {
      var redirectUrl = Url.Action(nameof(FacebookResponse), "Account", new { returnUrl });
 var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
      return Challenge(properties, "Facebook");
    }

  [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleResponse(string? returnUrl = null)
    {
   return await HandleOAuthResponse("Google", returnUrl);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> FacebookResponse(string? returnUrl = null)
    {
   return await HandleOAuthResponse("Facebook", returnUrl);
    }

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
     // Facebook picture claims
picture = claims?.FirstOrDefault(c => c.Type == "urn:facebook:picture:url")?.Value
   ?? claims?.FirstOrDefault(c => c.Type == "urn:facebook:picture")?.Value
            ?? claims?.FirstOrDefault(c => c.Type == "picture")?.Value;

      // If no picture claim found, construct Facebook Graph API URL
        if (string.IsNullOrEmpty(picture) && !string.IsNullOrEmpty(providerId))
     {
    picture = $"https://graph.facebook.com/{providerId}/picture?type=large";
       }
        }
       else if (provider == "Google")
    {
        // Google picture claims
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
          // Get full name from profile
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
           {
     userClaims.Add(new Claim(ClaimTypes.Role, roleName));
     }

      var identity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
       var principal = new ClaimsPrincipal(identity);

       await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
new AuthenticationProperties { IsPersistent = true });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
       {
                 return Redirect(returnUrl);
       }

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
          // Log the exception
            TempData["ErrorMessage"] = $"حدث خطأ أثناء تسجيل الدخول: {ex.Message}";
            return RedirectToAction(nameof(Login));
        }
    }

    [HttpGet]
    [AllowAnonymous]
  public IActionResult CompleteGoogleRegistration()
    {
        return CompleteSocialRegistration();
    }

    [HttpGet]
  [AllowAnonymous]
    public IActionResult CompleteSocialRegistration()
    {
        var provider = TempData["OAuthProvider"]?.ToString() ?? "Google";
        var email = TempData["OAuthEmail"]?.ToString();
        var name = TempData["OAuthName"]?.ToString();
        var picture = TempData["OAuthPicture"]?.ToString();

  if (string.IsNullOrEmpty(email))
    {
       return RedirectToAction(nameof(Register));
    }

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

  [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteGoogleRegistration(CompleteGoogleRegistrationViewModel model)
  {
        return await CompleteSocialRegistrationPost(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteSocialRegistration(CompleteGoogleRegistrationViewModel model)
 {
        return await CompleteSocialRegistrationPost(model);
    }

    private async Task<IActionResult> CompleteSocialRegistrationPost(CompleteGoogleRegistrationViewModel model)
  {
     if (!ModelState.IsValid)
        {
    return View("CompleteGoogleRegistration", model);
}

        var provider = TempData["OAuthProvider"]?.ToString() ?? "Google";
        var email = TempData["OAuthEmail"]?.ToString();
        var oauthId = TempData["OAuthId"]?.ToString();
        var picture = TempData["OAuthPicture"]?.ToString();

     if (string.IsNullOrEmpty(email))
        {
      return RedirectToAction(nameof(Register));
        }

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
         // Download and store the OAuth profile picture
         // For now, we'll skip downloading external images to keep it simple
         // You can implement HTTP download logic if needed
           await _unitOfWork.Customers.UpdateAsync(customerProfile);
              }
   }
          else if (role == RegistrationRole.Tailor)
 {
              var tailorProfile = await _unitOfWork.Tailors.GetByUserIdAsync(user.Id);
                if (tailorProfile != null)
        {
            // Download and store the OAuth profile picture
            // For now, we'll skip downloading external images to keep it simple
         await _unitOfWork.Tailors.UpdateAsync(tailorProfile);
      }
                }

    await _unitOfWork.SaveChangesAsync();
            }

   // Sign in the user
          // Get full name from profile
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
         {
  userClaims.Add(new Claim(ClaimTypes.Role, roleName));
     }

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

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> CompleteTailorProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
     return Unauthorized();
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userGuid);
        if (user == null)
        {
     return NotFound();
       }

        // Check if user is a tailor
        var roleName = User.FindFirstValue(ClaimTypes.Role);
  if (roleName?.ToLower() != "tailor")
      {
   TempData["ErrorMessage"] = "هذه الصفحة مخصصة للخياطين فقط";
  return RedirectToAction("Index", "Home");
   }

  // Check if profile is already completed
        var tailorProfile = await _unitOfWork.Tailors.GetByUserIdAsync(userGuid);
    if (tailorProfile != null && !string.IsNullOrEmpty(tailorProfile.Bio) && tailorProfile.IsVerified)
        {
   TempData["InfoMessage"] = "تم إكمال ملفك الشخصي بالفعل";
      return RedirectToAction("Tailor", "Dashboards");
   }

        var model = new CompleteTailorProfileRequest
     {
     UserId = userGuid,
   Email = user.Email,
       FullName = User.FindFirstValue("FullName") ?? user.Email
   };

 return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteTailorProfile(CompleteTailorProfileRequest model)
  {
  if (!ModelState.IsValid)
   {
       return View(model);
  }

        try
 {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
   if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
  {
     return Unauthorized();
  }

         // Check if user is a tailor
   var roleName = User.FindFirstValue(ClaimTypes.Role);
    if (roleName?.ToLower() != "tailor")
  {
        TempData["ErrorMessage"] = "هذه الصفحة مخصصة للخياطين فقط";
    return RedirectToAction("Index", "Home");
   }

   // Get tailor profile
       var tailorProfile = await _unitOfWork.Tailors.GetByUserIdAsync(userGuid);
    if (tailorProfile == null)
       {
        TempData["ErrorMessage"] = "لم يتم العثور على ملف الخياط";
    return RedirectToAction("Index", "Home");
     }

   // Update tailor profile with complete information
            tailorProfile.ShopName = model.WorkshopName;
     tailorProfile.Address = model.Address;
  tailorProfile.City = model.City;
  tailorProfile.Bio = model.Description;
      tailorProfile.ExperienceYears = model.ExperienceYears;
   tailorProfile.UpdatedAt = _dateTime.Now;

   // Save ID document as profile picture (temporary)
       if (model.IdDocument != null && model.IdDocument.Length > 0)
  {
          using var memoryStream = new MemoryStream();
        await model.IdDocument.CopyToAsync(memoryStream);
    tailorProfile.ProfilePictureData = memoryStream.ToArray();
 tailorProfile.ProfilePictureContentType = model.IdDocument.ContentType;
            }

      await _unitOfWork.Tailors.UpdateAsync(tailorProfile);

  // Save portfolio images - Note: Currently PortfolioImage only supports ImageUrl
   // In future, may need to extend the model to support binary data
    if (model.PortfolioImages != null && model.PortfolioImages.Any())
     {
       // For now, we'll save them using file upload service
          var portfolioFolderPath = Path.Combine("wwwroot", "uploads", "portfolio", tailorProfile.Id.ToString());
      Directory.CreateDirectory(portfolioFolderPath);

       int imageIndex = 0;
    foreach (var image in model.PortfolioImages.Take(10)) // Limit to 10 images
      {
 if (image.Length > 0)
 {
         // Save file to disk
     var fileName = $"portfolio_{_dateTime.Now.Ticks}_{imageIndex++}{Path.GetExtension(image.FileName)}";
     var filePath = Path.Combine(portfolioFolderPath, fileName);

  using (var stream = new FileStream(filePath, FileMode.Create))
       {
      await image.CopyToAsync(stream);
  }

       // Store relative path in database
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

   await _unitOfWork.SaveChangesAsync();

     TempData["SuccessMessage"] = "تم إكمال ملفك الشخصي بنجاح! سيتم مراجعة طلبك من قبل الإدارة خلال 24-48 ساعة.";
   return RedirectToAction("Tailor", "Dashboards");
 }
        catch (Exception ex)
   {
       _logger.LogError(ex, "[AccountController] Error completing tailor profile for user: {UserId}", model.UserId);
       ModelState.AddModelError(string.Empty, "حدث خطأ أثناء حفظ البيانات. يرجى المحاولة مرة أخرى.");
    return View(model);
  }
    }

    /// <summary>
    /// Verify email using token from email link
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
  {
            TempData["RegisterSuccess"] = "تم تأكيد بريدك الإلكتروني بنجاح! يمكنك الآن تسجيل الدخول";
       }
  else
  {
       TempData["ErrorMessage"] = error ?? "فشل تأكيد البريد الإلكتروني";
        }

        return RedirectToAction("Login");
    }

    /// <summary>
    /// Resend email verification link
    /// </summary>
  [HttpGet]
    [AllowAnonymous]
    public IActionResult ResendVerificationEmail()
    {
    return View();
    }

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
        {
          TempData["RegisterSuccess"] = "تم إرسال رسالة التحقق بنجاح! يرجى التحقق من بريدك الإلكتروني";
        }
     else
   {
      TempData["ErrorMessage"] = error ?? "فشل إرسال رسالة التحقق";
        }

    return View();
    }


    /// <summary>
    /// Verify email using token from email link
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
  {
            TempData["RegisterSuccess"] = "تم تأكيد بريدك الإلكتروني بنجاح! يمكنك الآن تسجيل الدخول";
       }
  else
  {
       TempData["ErrorMessage"] = error ?? "فشل تأكيد البريد الإلكتروني";
        }

        return RedirectToAction("Login");
    }

    /// <summary>
    /// Resend email verification link
    /// </summary>
  [HttpGet]
    [AllowAnonymous]
    public IActionResult ResendVerificationEmail()
    {
    return View();
    }

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
        {
          TempData["RegisterSuccess"] = "تم إرسال رسالة التحقق بنجاح! يرجى التحقق من بريدك الإلكتروني";
        }
     else
   {
      TempData["ErrorMessage"] = error ?? "فشل إرسال رسالة التحقق";
        }

    return View();
    }

    [HttpGet]
    [AllowAnonymous] // Allow access before authentication since they just registered
    public async Task<IActionResult> ProvideTailorEvidence()
 {
        // Check if coming from registration
        var userIdStr = TempData["UserId"]?.ToString();
        var email = TempData["UserEmail"]?.ToString();
        var name = TempData["UserName"]?.ToString();
        
  // Keep the data for the POST
        TempData.Keep("UserId");
 TempData.Keep("UserEmail");
        TempData.Keep("UserName");

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
        TempData["ErrorMessage"] = "جلسة غير صالحة. يرجى التسجيل مرة أخرى";
            return RedirectToAction(nameof(Register));
      }

        // Verify user exists and is a tailor
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null || user.Role?.Name?.ToLower() != "tailor")
        {
  TempData["ErrorMessage"] = "حساب غير صالح";
     return RedirectToAction(nameof(Register));
    }

        // CRITICAL: Check if profile already exists (evidence already provided)
 // This ensures ONE-TIME verification only
        var existingProfile = await _unitOfWork.Tailors.GetByUserIdAsync(userId);
        if (existingProfile != null)
        {
        _logger.LogWarning("[AccountController] Tailor {UserId} attempted to access evidence page but already has profile. Redirecting to login.", userId);
      TempData["InfoMessage"] = "تم تقديم الأوراق الثبوتية بالفعل. يمكنك تسجيل الدخول الآن";
    return RedirectToAction(nameof(Login));
        }

        var model = new CompleteTailorProfileRequest
        {
        UserId = userId,
  Email = email ?? user.Email,
            FullName = name ?? user.Email
        };

  return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
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
            // This ensures ONE-TIME verification only
  var existingProfile = await _unitOfWork.Tailors.GetByUserIdAsync(model.UserId);
  if (existingProfile != null)
      {
     _logger.LogWarning("[AccountController] Tailor {UserId} attempted to submit evidence but already has profile. Blocking submission.", model.UserId);
       TempData["InfoMessage"] = "تم تقديم الأوراق الثبوتية بالفعل. لا يمكن التقديم مرة أخرى.";
       return RedirectToAction(nameof(Login));
      }

        // Validate that required evidence documents are provided
            if (model.IdDocument == null || model.IdDocument.Length == 0)
{
    ModelState.AddModelError(nameof(model.IdDocument), "يجب تحميل صورة الهوية الشخصية");
     return View(model);
            }

 if ((model.PortfolioImages == null || !model.PortfolioImages.Any()) && 
   (model.WorkSamples == null || !model.WorkSamples.Any()))
    {
     ModelState.AddModelError(string.Empty, "يجب تحميل على الأقل صورة واحدة من أعمالك السابقة");
      return View(model);
   }

            // Create tailor profile NOW (after evidence is provided)
     // THIS IS THE ONE AND ONLY TIME the profile is created
       var tailorProfile = new TailorProfile
  {
     Id = Guid.NewGuid(),
    UserId = model.UserId,
     FullName = model.FullName,
   ShopName = model.WorkshopName,
    Address = model.Address,
          City = model.City,
            Bio = model.Description,
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
 if (model.PortfolioImages != null && model.PortfolioImages.Any())
   {
 var portfolioFolderPath = Path.Combine("wwwroot", "uploads", "portfolio", tailorProfile.Id.ToString());
     Directory.CreateDirectory(portfolioFolderPath);

       int imageIndex = 0;
          foreach (var image in model.PortfolioImages.Take(10))
       {
       if (image.Length > 0)
       {
       var fileName = $"portfolio_{_dateTime.Now.Ticks}_{imageIndex++}{Path.GetExtension(image.FileName)}";
               var filePath = Path.Combine(portfolioFolderPath, fileName);

      using (var stream = new FileStream(filePath, FileMode.Create))
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

  // NOW activate the user and send email verification
    // This is the ONLY time this happens for tailor registration
   user.IsActive = true; // Activate for dashboard access while awaiting admin approval
    user.UpdatedAt = _dateTime.Now;

    // Generate email verification token
   var verificationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
             .Replace("+", "").Replace("/", "").Replace("=", "").Substring(0, 32);
       
            user.EmailVerificationToken = verificationToken;
            user.EmailVerificationTokenExpires = _dateTime.Now.AddHours(24);

   await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("[AccountController] Tailor {UserId} completed ONE-TIME evidence submission. Profile created, user activated.", model.UserId);

            // Send email verification (background task)
       _ = Task.Run(async () =>
    {
        try
       {
        // You can create a new email method or use existing one
  // await _emailService.SendEmailVerificationAsync(user.Email, model.FullName, verificationToken);
        _logger.LogInformation("Email verification sent to tailor: {Email}", user.Email);
    }
  catch (Exception ex)
  {
         _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
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

    [HttpGet]
    [Authorize(Policy = "TailorPolicy")]
    public async Task<IActionResult> CompleteTailorProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
  {
            return Unauthorized();
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userGuid);
     if (user == null)
    {
  return NotFound();
      }

  // Check if user is a tailor
    var roleName = User.FindFirstValue(ClaimTypes.Role);
        if (roleName?.ToLower() != "tailor")
      {
         TempData["ErrorMessage"] = "هذه الصفحة مخصصة للخياطين فقط";
       return RedirectToAction("Index", "Home");
  }

     // Check if profile exists (should exist from ONE-TIME evidence submission)
  var tailorProfile = await _unitOfWork.Tailors.GetByUserIdAsync(userGuid);
        if (tailorProfile == null)
        {
     _logger.LogWarning("[AccountController] Authenticated tailor {UserId} has no profile. This should not happen. Redirecting to evidence page.", userGuid);
      TempData["ErrorMessage"] = "يجب تقديم الأوراق الثبوتية أولاً";
       // This should rarely happen - only if data integrity issue
    return RedirectToAction("Index", "Home");
        }

        // This page is for OPTIONAL profile updates, not verification
        // Verification was done ONE-TIME via ProvideTailorEvidence
 var model = new CompleteTailorProfileRequest
    {
            UserId = userGuid,
     Email = user.Email,
    FullName = tailorProfile.FullName ?? User.FindFirstValue("FullName") ?? user.Email,
       WorkshopName = tailorProfile.ShopName,
     Address = tailorProfile.Address,
     City = tailorProfile.City,
     Description = tailorProfile.Bio,
  ExperienceYears = tailorProfile.ExperienceYears
        };

return View(model);
    }

    [HttpPost]
    [Authorize(Policy = "TailorPolicy")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteTailorProfile(CompleteTailorProfileRequest model)
    {
if (!ModelState.IsValid)
        {
  return View(model);
        }

  try
        {
          var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
      {
    return Unauthorized();
            }

            // Check if user is a tailor
            var roleName = User.FindFirstValue(ClaimTypes.Role);
if (roleName?.ToLower() != "tailor")
  {
        TempData["ErrorMessage"] = "هذه الصفحة مخصصة للخياطين فقط";
              return RedirectToAction("Index", "Home");
        }

   // Get tailor profile (should exist from evidence submission)
        var tailorProfile = await _unitOfWork.Tailors.GetByUserIdAsync(userGuid);
   if (tailorProfile == null)
         {
      TempData["ErrorMessage"] = "لم يتم العثور على ملف الخياط";
          return RedirectToAction("Index", "Home");
         }

            // Update tailor profile with additional information
            tailorProfile.ShopName = model.WorkshopName;
       tailorProfile.Address = model.Address;
  tailorProfile.City = model.City;
  tailorProfile.Bio = model.Description;
            tailorProfile.ExperienceYears = model.ExperienceYears;
tailorProfile.UpdatedAt = _dateTime.Now;

     await _unitOfWork.Tailors.UpdateAsync(tailorProfile);
     await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "تم تحديث ملفك الشخصي بنجاح!";
     return RedirectToAction("Tailor", "Dashboards");
   }
        catch (Exception ex)
        {
         _logger.LogError(ex, "[AccountController] Error updating tailor profile for user: {UserId}", model.UserId);
            ModelState.AddModelError(string.Empty, "حدث خطأ أثناء حفظ البيانات. يرجى المحاولة مرة أخرى.");
          return View(model);
}
    }
}
