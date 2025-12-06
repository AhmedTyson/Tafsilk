using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TafsilkPlatform.DataAccess.Repository;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels;

namespace TafsilkPlatform.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly Microsoft.AspNetCore.Identity.UI.Services.IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(
        ILogger<HomeController> logger,
        Microsoft.AspNetCore.Identity.UI.Services.IEmailSender emailSender,
        IConfiguration configuration,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _emailSender = emailSender;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        try
        {
            // 1. Get Featured Tailors (Top 4 verified, highest rated)
            var featuredTailors = await _unitOfWork.Tailors.GetTopRatedTailorsAsync(4);

            // Take top 4 in memory (or use pagination if repo supports it)
            var topTailors = featuredTailors.ToList();

            // 2. Get Statistics
            var trustedTailorsCount = await _unitOfWork.Tailors.CountAsync(t => t.IsVerified);
            var completedOrdersCount = await _unitOfWork.Orders.CountAsync(o => o.Status == OrderStatus.Delivered);

            // Calculate average rating across platform (if possible, otherwise mock or calculate from tailors)
            var avgRating = topTailors.Any() ? topTailors.Average(t => (double)t.AverageRating) : 5.0;

            // 3. Get Trending Products
            var trendingProducts = await _unitOfWork.Products.GetFeaturedProductsAsync(4);

            var model = new HomeViewModel
            {
                FeaturedTailors = topTailors,
                TrendingProducts = trendingProducts.ToList(),
                TrustedTailorsCount = trustedTailorsCount > 0 ? trustedTailorsCount : 50, // Fallback for new site
                CompletedOrdersCount = completedOrdersCount > 0 ? completedOrdersCount : 100, // Fallback
                AveragePlatformRating = Math.Round(avgRating, 1)
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading home page data");
            // Fallback to empty model to prevent crash
            return View(new HomeViewModel());
        }
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult Terms()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult FAQ()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
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
            var supportEmail = _configuration["Email:SupportEmail"] ?? "ahmedmessi2580@gmail.com";
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
