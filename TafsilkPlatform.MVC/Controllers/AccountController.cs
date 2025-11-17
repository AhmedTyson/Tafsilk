using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TafsilkPlatform.MVC.Models;
using TafsilkPlatform.MVC.Services;

namespace TafsilkPlatform.MVC.Controllers
{
    public class AccountController : Controller
 {
     private readonly IAuthService _authService;

  public AccountController(IAuthService authService)
  {
            _authService = authService;
      }

        // GET: Account/Login
      [HttpGet]
        public IActionResult Login(string? returnUrl = null)
  {
      ViewData["ReturnUrl"] = returnUrl;
      return View();
  }

  // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
   public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
  ViewData["ReturnUrl"] = returnUrl;

    if (!ModelState.IsValid)
     return View(model);

   // REAL authentication with password validation
            var user = await _authService.AuthenticateAsync(model.Email, model.Password);

   if (user == null)
       {
 ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة");
      return View(model);
         }

   // Create authentication claims
            var claims = new List<Claim>
   {
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new Claim(ClaimTypes.Email, user.Email),
     new Claim(ClaimTypes.Name, user.FullName),
      new Claim(ClaimTypes.Role, user.Role)
            };

     var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
     {
     IsPersistent = model.RememberMe,
          ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(2)
   };

            // Sign in the user with cookie authentication
    await HttpContext.SignInAsync(
   CookieAuthenticationDefaults.AuthenticationScheme,
      new ClaimsPrincipal(claimsIdentity),
      authProperties);

 TempData["SuccessMessage"] = $"مرحباً {user.FullName}!";

   // Redirect based on return URL or role
   if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
       return Redirect(returnUrl);

 return user.Role switch
 {
                "Admin" => RedirectToAction("Index", "Dashboard"),
         "Tailor" => RedirectToAction("Index", "Tailors"),
                _ => RedirectToAction("Index", "Home")
          };
}

        // GET: Account/Register
 [HttpGet]
        public IActionResult Register()
        {
    return View();
 }

        // POST: Account/Register
 [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Register(RegisterViewModel model)
   {
   if (!ModelState.IsValid)
    return View(model);

   var (success, errorMessage) = await _authService.RegisterAsync(model);

     if (!success)
 {
    ModelState.AddModelError(string.Empty, errorMessage ?? "حدث خطأ أثناء التسجيل");
   return View(model);
 }

   TempData["SuccessMessage"] = "تم التسجيل بنجاح! يمكنك الآن تسجيل الدخول";
 return RedirectToAction(nameof(Login));
 }

 // POST: Account/Logout
        [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Logout()
        {
   await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
TempData["SuccessMessage"] = "تم تسجيل الخروج بنجاح";
      return RedirectToAction("Index", "Home");
        }

        // GET: Account/AccessDenied
        public IActionResult AccessDenied()
   {
     return View();
        }
    }
}
