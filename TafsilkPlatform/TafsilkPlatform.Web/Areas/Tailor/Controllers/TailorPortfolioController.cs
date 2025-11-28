using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Web.Services;

namespace TafsilkPlatform.Web.Areas.Tailor.Controllers;

/// <summary>
/// Controller for public tailor portfolio views
/// </summary>
[Area("Tailor")]
[Route("portfolio")]
public class TailorPortfolioController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<TailorPortfolioController> _logger;
    // ✅ NEW: Inject portfolio service for clean architecture
    private readonly IPortfolioService _portfolioService;

    public TailorPortfolioController(
        ApplicationDbContext db,
        ILogger<TailorPortfolioController> logger,
        IPortfolioService portfolioService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _portfolioService = portfolioService ?? throw new ArgumentNullException(nameof(portfolioService));
    }

    /// <summary>
    /// View public tailor profile page
    /// GET: /tailor-portfolio/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ViewPublicTailorProfile(Guid id)
    {
        try
        {
            var tailor = await _db.TailorProfiles
                .Include(t => t.User)
                .Include(t => t.TailorServices)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tailor == null)
            {
                _logger.LogWarning("Tailor profile not found: {TailorId}", id);
                TempData["Error"] = "Tailor profile not found";
                return RedirectToAction("Index", "Home");
            }

            // Use service layer to get portfolio images
            var portfolioImages = await _portfolioService.GetPublicPortfolioAsync(id);
            if (portfolioImages == null)
            {
                portfolioImages = Enumerable.Empty<TafsilkPlatform.Models.Models.PortfolioImage>();
            }

            // Ensure navigation collections are not null to avoid NRE in views
            tailor.PortfolioImages = portfolioImages.ToList();
            // Calculate statistics with safety guards
            ViewBag.PortfolioCount = tailor.PortfolioImages?.Count ?? 0;
            ViewBag.PortfolioCount = tailor.PortfolioImages?.Count ?? 0;
            ViewBag.ReviewCount = 0; // Simplified - no reviews
            ViewBag.AverageRating = tailor.AverageRating;
            ViewBag.CompletedOrders = await _db.Orders.CountAsync(o =>
                o.TailorId == id &&
                o.Status == TafsilkPlatform.Models.Models.OrderStatus.Delivered);

            // Fetch products for this tailor
            var products = await _db.Products
                .Where(p => p.TailorId == id && !p.IsDeleted && p.IsAvailable)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            ViewBag.Products = products;

            return View(tailor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading public tailor profile for {TailorId}", id);
            TempData["Error"] = "An error occurred while loading the profile";
            return RedirectToAction("Index", "Home");
        }
    }
}
