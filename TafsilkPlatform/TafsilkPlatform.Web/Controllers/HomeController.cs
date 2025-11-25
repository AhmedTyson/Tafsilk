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
