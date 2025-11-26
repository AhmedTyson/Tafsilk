using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
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
    public IActionResult Contact(string name, string email, string subject, string message)
    {
        // TODO: Implement email sending logic here
        // For now, just show success message
        TempData["SuccessMessage"] = "Thank you for contacting us! We'll get back to you shortly.";
        _logger.LogInformation("Contact form submitted: {Name}, {Email}, Subject: {Subject}", name, email, subject);
        
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
