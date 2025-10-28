using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TafsilkPlatform.Application.Models.Auth;
using TafsilkPlatform.Core.Interfaces;

namespace TafsilkPlatform.Web.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _auth;
        [BindProperty]
        public RegisterRequest Input { get; set; } = new();
        public string? Error { get; set; }

        public RegisterModel(IAuthService auth)
        {
            _auth = auth;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Error = "Invalid form";
                return Page();
            }

            var (ok, err, user) = await _auth.RegisterAsync(Input);
            if (!ok)
            {
                Error = err;
                return Page();
            }

            return RedirectToPage("/Index");
        }
    }
}
