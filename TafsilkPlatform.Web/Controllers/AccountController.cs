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
    public IActionResult Settings()
    {
        // Redirect to the consolidated settings page in UserSettings controller
        return RedirectToAction("Edit", "UserSettings");
 }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Settings(AccountSettingsViewModel model)
    {
     // Redirect POST requests to the consolidated settings page
        return RedirectToAction("Edit", "UserSettings");
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
}
