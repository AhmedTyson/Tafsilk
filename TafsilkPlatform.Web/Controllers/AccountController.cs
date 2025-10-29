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

namespace TafsilkPlatform.Web.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly IAuthService _auth;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileUploadService _fileUploadService;

    public AccountController(
        IAuthService auth, 
        IUserRepository userRepository, 
        IUnitOfWork unitOfWork,
        IFileUploadService fileUploadService)
    {
      _auth = auth;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _fileUploadService = fileUploadService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string name, string email, string password, string userType, string? phoneNumber)
    {
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

        // After register, redirect to login page with role preselected or direct login
        TempData["RegisterSuccess"] = "تم إنشاء الحساب بنجاح. يمكنك تسجيل الدخول الآن";
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

        // Build claims with role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.Email ?? string.Empty)
        };
        
        var roleName = user.Role?.Name ?? string.Empty;
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

    [HttpGet]
    public async Task<IActionResult> Settings()
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

        // Get the user's full name and profile info from their profile
    string fullName = string.Empty;
        string? profilePictureUrl = null;
     string? shopName = null;
        string? address = null;
      string? city = null;
 int? experienceYears = null;
        string? pricingRange = null;
        string? bio = null;
        string? gender = null;
        DateOnly? dateOfBirth = null;

        var role = User.FindFirstValue(ClaimTypes.Role);

   if (role == "Customer")
   {
    var profile = await _unitOfWork.Customers.GetByUserIdAsync(userGuid);
    if (profile != null)
         {
     fullName = profile.FullName;
    profilePictureUrl = profile.ProfilePictureUrl;
                city = profile.City;
       gender = profile.Gender;
      bio = profile.Bio;
       dateOfBirth = profile.DateOfBirth;
   }
  }
else if (role == "Tailor")
{
    var profile = await _unitOfWork.Tailors.GetByUserIdAsync(userGuid);
       if (profile != null)
       {
           fullName = profile.FullName ?? string.Empty;
        profilePictureUrl = profile.ProfilePictureUrl;
                shopName = profile.ShopName;
    address = profile.Address;
      city = profile.City;
     experienceYears = profile.ExperienceYears;
          pricingRange = profile.PricingRange;
   bio = profile.Bio;
 }
        }
 else if (role == "Corporate")
 {
          var profile = await _unitOfWork.Corporates.GetByUserIdAsync(userGuid);
  if (profile != null)
            {
   fullName = profile.ContactPerson ?? string.Empty;
            }
  }

        var model = new AccountSettingsViewModel
        {
   Email = user.Email ?? string.Empty,
       FullName = fullName,
     PhoneNumber = user.PhoneNumber ?? string.Empty,
          ProfilePictureUrl = profilePictureUrl,
       CurrentRole = role ?? "Customer",
      CurrentRoleId = user.RoleId,
   CanChangeRole = true, // Can request role change
         // Tailor-specific
     ShopName = shopName,
 Address = address,
     City = city,
     ExperienceYears = experienceYears,
            PricingRange = pricingRange,
    Bio = bio,
            // Customer-specific
     Gender = gender,
        DateOfBirth = dateOfBirth,
        // Notifications
EmailNotifications = user.EmailNotifications,
 SmsNotifications = user.SmsNotifications,
   PromotionalNotifications = user.PromotionalNotifications
 };

    return View(model);
 }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(AccountSettingsViewModel model)
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

        // Handle profile picture upload
   string? newProfilePictureUrl = null;
    if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
        {
       try
   {
    // Validate file
        if (!_fileUploadService.IsValidImage(model.ProfilePictureFile))
   {
    ModelState.AddModelError(nameof(model.ProfilePictureFile), "يرجى تحميل صورة صالحة (JPG, PNG, GIF, WEBP)");
       return View(model);
      }

    if (model.ProfilePictureFile.Length > _fileUploadService.GetMaxFileSizeInBytes())
{
   ModelState.AddModelError(nameof(model.ProfilePictureFile), "حجم الصورة كبير جداً. الحد الأقصى 5 ميجابايت");
       return View(model);
 }

        // Upload new picture
     newProfilePictureUrl = await _fileUploadService.UploadProfilePictureAsync(model.ProfilePictureFile, userGuid.ToString());

      // Delete old picture if exists
       if (!string.IsNullOrEmpty(model.ProfilePictureUrl))
      {
 await _fileUploadService.DeleteProfilePictureAsync(model.ProfilePictureUrl);
   }
    }
       catch (Exception ex)
 {
    ModelState.AddModelError(nameof(model.ProfilePictureFile), $"فشل تحميل الصورة: {ex.Message}");
     return View(model);
  }
  }

// Update user information
   user.PhoneNumber = model.PhoneNumber;
        user.EmailNotifications = model.EmailNotifications;
  user.SmsNotifications = model.SmsNotifications;
  user.PromotionalNotifications = model.PromotionalNotifications;
    user.UpdatedAt = DateTime.UtcNow;

      await _unitOfWork.Users.UpdateAsync(user);

    // Update profile based on role
     var role = User.FindFirstValue(ClaimTypes.Role);
     
        if (role == "Customer")
  {
         var profile = await _unitOfWork.Customers.GetByUserIdAsync(userGuid);
      if (profile != null)
   {
  profile.FullName = model.FullName;
       profile.City = model.City;
       profile.Gender = model.Gender;
   profile.Bio = model.Bio;
     profile.DateOfBirth = model.DateOfBirth;
      
      if (newProfilePictureUrl != null)
      {
         profile.ProfilePictureUrl = newProfilePictureUrl;
    }
      
       profile.UpdatedAt = DateTime.UtcNow;
          await _unitOfWork.Customers.UpdateAsync(profile);
      }
}
        else if (role == "Tailor")
{
    var profile = await _unitOfWork.Tailors.GetByUserIdAsync(userGuid);
   if (profile != null)
    {
   profile.FullName = model.FullName;
           profile.ShopName = model.ShopName;
   profile.Address = model.Address;
     profile.City = model.City;
    profile.ExperienceYears = model.ExperienceYears;
      profile.PricingRange = model.PricingRange;
    profile.Bio = model.Bio;

    if (newProfilePictureUrl != null)
   {
  profile.ProfilePictureUrl = newProfilePictureUrl;
   }

       profile.UpdatedAt = DateTime.UtcNow;
 await _unitOfWork.Tailors.UpdateAsync(profile);
    }
 }
        else if (role == "Corporate")
        {
   var profile = await _unitOfWork.Corporates.GetByUserIdAsync(userGuid);
      if (profile != null)
        {
     profile.ContactPerson = model.FullName;
  profile.UpdatedAt = DateTime.UtcNow;
    await _unitOfWork.Corporates.UpdateAsync(profile);
   }
      }

        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "تم تحديث الإعدادات بنجاح!";
        return RedirectToAction(nameof(Settings));
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
     user.UpdatedAt = DateTime.UtcNow;

 await _unitOfWork.Users.UpdateAsync(user);
  await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "تم تغيير كلمة المرور بنجاح!";
    return RedirectToAction(nameof(Settings));
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
  user.UpdatedAt = DateTime.UtcNow;
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
      CreatedAt = DateTime.UtcNow
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
return RedirectToAction(nameof(Settings));
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
    public async Task<IActionResult> GoogleResponse(string? returnUrl = null)
    {
        try
        {
      var authenticateResult = await HttpContext.AuthenticateAsync("Google");
            
         if (!authenticateResult.Succeeded)
     {
      TempData["ErrorMessage"] = "فشل تسجيل الدخول عبر Google";
 return RedirectToAction(nameof(Login));
      }

            var claims = authenticateResult.Principal?.Claims;
     var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
         var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var picture = claims?.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value;

     if (string.IsNullOrEmpty(email))
            {
     TempData["ErrorMessage"] = "لم نتمكن من الحصول على بريدك الإلكتروني من Google";
                return RedirectToAction(nameof(Login));
            }

            // Check if user exists
      var existingUser = await _unitOfWork.Users.GetByEmailAsync(email);

     if (existingUser != null)
    {
     // User exists, sign them in
      var userClaims = new List<Claim>
                {
              new Claim(ClaimTypes.NameIdentifier, existingUser.Id.ToString()),
         new Claim(ClaimTypes.Email, existingUser.Email ?? string.Empty),
  new Claim(ClaimTypes.Name, existingUser.Email ?? string.Empty)
  };

       var roleName = existingUser.Role?.Name ?? string.Empty;
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
     TempData["GoogleEmail"] = email;
            TempData["GoogleName"] = name ?? string.Empty;
    TempData["GooglePicture"] = picture ?? string.Empty;
             TempData["GoogleId"] = googleId ?? string.Empty;
     
      return RedirectToAction(nameof(CompleteGoogleRegistration));
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
        var email = TempData["GoogleEmail"]?.ToString();
      var name = TempData["GoogleName"]?.ToString();
        var picture = TempData["GooglePicture"]?.ToString();
        
        if (string.IsNullOrEmpty(email))
        {
    return RedirectToAction(nameof(Register));
        }

    // Keep data for the form
        TempData.Keep("GoogleEmail");
        TempData.Keep("GoogleName");
  TempData.Keep("GooglePicture");
        TempData.Keep("GoogleId");

    var model = new CompleteGoogleRegistrationViewModel
        {
            Email = email,
            FullName = name ?? string.Empty,
 ProfilePictureUrl = picture
        };

  return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteGoogleRegistration(CompleteGoogleRegistrationViewModel model)
    {
        if (!ModelState.IsValid)
{
      return View(model);
      }

        var email = TempData["GoogleEmail"]?.ToString();
     var googleId = TempData["GoogleId"]?.ToString();
    var picture = TempData["GooglePicture"]?.ToString();

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
      return View(model);
    }

            // Update profile picture if available from Google
            if (!string.IsNullOrEmpty(picture))
   {
       if (role == RegistrationRole.Customer)
   {
          var customerProfile = await _unitOfWork.Customers.GetByUserIdAsync(user.Id);
      if (customerProfile != null)
               {
        customerProfile.ProfilePictureUrl = picture;
   await _unitOfWork.Customers.UpdateAsync(customerProfile);
               }
        }
         else if (role == RegistrationRole.Tailor)
     {
     var tailorProfile = await _unitOfWork.Tailors.GetByUserIdAsync(user.Id);
   if (tailorProfile != null)
 {
       tailorProfile.ProfilePictureUrl = picture;
          await _unitOfWork.Tailors.UpdateAsync(tailorProfile);
          }
    }

      await _unitOfWork.SaveChangesAsync();
            }

            // Sign in the user
  var userClaims = new List<Claim>
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
      new Claim(ClaimTypes.Name, user.Email ?? string.Empty)
     };

        var roleName = user.Role?.Name ?? string.Empty;
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
            return View(model);
        }
    }
}
