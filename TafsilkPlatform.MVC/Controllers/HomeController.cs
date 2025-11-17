using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TafsilkPlatform.MVC.Models;
using TafsilkPlatform.MVC.Services;

namespace TafsilkPlatform.MVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly MockDataService _mockDataService;

    public HomeController(ILogger<HomeController> logger, MockDataService mockDataService)
    {
        _logger = logger;
        _mockDataService = mockDataService;
    }

    public IActionResult Index()
    {
        // Display featured tailors on home page
        var tailors = _mockDataService.GetAllTailors().Take(3).ToList();
        return View(tailors);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
