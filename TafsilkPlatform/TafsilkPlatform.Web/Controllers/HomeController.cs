using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly Microsoft.AspNetCore.Identity.UI.Services.IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, Microsoft.AspNetCore.Identity.UI.Services.IEmailSender emailSender, IConfiguration configuration)
    {
        _logger = logger;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Contact()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(string name, string email, string subject, string message)
    {
        try
        {
            var supportEmail = _configuration["SendGrid:FromEmail"] ?? "support@tafsilk.com";
            var emailSubject = $"Contact Form: {subject}";
            var emailBody = $@"
                <h3>New Contact Form Submission</h3>
                <p><strong>Name:</strong> {name}</p>
                <p><strong>Email:</strong> {email}</p>
                <p><strong>Subject:</strong> {subject}</p>
                <hr/>
                <p><strong>Message:</strong></p>
                <p>{message}</p>
            ";

            await _emailSender.SendEmailAsync(supportEmail, emailSubject, emailBody);

            TempData["SuccessMessage"] = "Thank you for contacting us! We'll get back to you shortly.";
            _logger.LogInformation("Contact form submitted and email sent: {Name}, {Email}", name, email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending contact email");
            TempData["ErrorMessage"] = "Sorry, something went wrong. Please try again later.";
        }

        return RedirectToAction(nameof(Contact));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        var errorViewModel = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = statusCode ?? 500
        };

        if (statusCode.HasValue)
        {
            HttpContext.Response.StatusCode = statusCode.Value;
        }

        return View(errorViewModel);
    }
}
