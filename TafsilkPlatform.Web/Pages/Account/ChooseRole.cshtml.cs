using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using TafsilkPlatform.Core.Interfaces;
using TafsilkPlatform.Application.Models.Auth;

namespace TafsilkPlatform.Web.Pages.Account
{
    public class ChooseRoleModel : PageModel
    {
        private readonly IAuthService _auth;
        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string Role { get; set; } = "Customer";
        [BindProperty] public string? FullName { get; set; }
        [BindProperty] public string? ShopName { get; set; }
        [BindProperty] public string? Address { get; set; }
        [BindProperty] public string? CompanyName { get; set; }
        [BindProperty] public string? ContactPerson { get; set; }
        public string? Error { get; set; }

        public ChooseRoleModel(IAuthService auth) => _auth = auth;

        public IActionResult OnGet(string? returnUrl = "/")
        {
            Email = TempData["ExternalEmail"]?.ToString() ?? Email;
            if (string.IsNullOrWhiteSpace(Email)) return RedirectToPage("/Account/Login");
            ViewData["ReturnUrl"] = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = "/")
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                Error = "Missing email";
                return Page();
            }

            var req = new RegisterRequest
            {
                Email = Email,
                Password = Guid.NewGuid().ToString("N") + "!aA1", // random local password
                Role = Enum.TryParse<RegistrationRole>(Role, out var r) ? r : RegistrationRole.Customer,
                FullName = FullName,
                ShopName = ShopName,
                Address = Address,
                CompanyName = CompanyName,
                ContactPerson = ContactPerson
            };

            var (ok, err, user) = await _auth.RegisterAsync(req);
            if (!ok || user is null)
            {
                Error = err ?? "Registration failed";
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

            return LocalRedirect(returnUrl ?? "/");
        }
    }
}