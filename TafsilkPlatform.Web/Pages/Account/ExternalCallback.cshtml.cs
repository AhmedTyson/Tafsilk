using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using TafsilkPlatform.Core.Interfaces;

namespace TafsilkPlatform.Web.Pages.Account
{
    public class ExternalCallbackModel : PageModel
    {
        private readonly IUserRepository _users;

        public ExternalCallbackModel(IUserRepository users) => _users = users;

        public async Task<IActionResult> OnGetAsync(string? returnUrl = "/")
        {
            // After external provider callback, the cookie should be issued
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToPage("/Account/Login");

            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToPage("/Account/Login", new { error = "No email returned from provider" });

            var existing = await _users.GetByEmailAsync(email);
            if (existing is not null)
            {
                // Already signed-in via cookie; just continue
                return LocalRedirect(returnUrl ?? "/");
            }

            TempData["ExternalEmail"] = email;
            return RedirectToPage("/Account/ChooseRole", new { returnUrl });
        }
    }
}