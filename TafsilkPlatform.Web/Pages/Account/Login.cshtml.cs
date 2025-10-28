using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using TafsilkPlatform.Core.Interfaces;

namespace TafsilkPlatform.Web.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _auth;
        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string Password { get; set; } = string.Empty;
        public string? Error { get; set; }

        public LoginModel(IAuthService auth) => _auth = auth;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            var (ok, err, user) = await _auth.ValidateUserAsync(Email, Password);
            if (!ok || user is null)
            {
                Error = err;
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return RedirectToPage("/Index");
        }

        public IActionResult OnPostExternalLogin(string provider, string? returnUrl = "/")
        {
            var redirectUrl = Url.Page("/Account/ExternalCallback", new { returnUrl, provider });
            var props = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(props, provider);
        }
    }
}
