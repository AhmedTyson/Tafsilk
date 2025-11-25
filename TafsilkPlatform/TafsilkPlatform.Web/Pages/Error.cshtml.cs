using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace TafsilkPlatform.Web.Pages
{
    public class ErrorModel : PageModel
    {
        private readonly ILogger<ErrorModel> _logger;
        public string? ErrorMessage { get; set; }

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(string? message)
        {
            ErrorMessage = message;
            _logger.LogError("Error page displayed. Message: {Message}", message);
        }
    }
}
