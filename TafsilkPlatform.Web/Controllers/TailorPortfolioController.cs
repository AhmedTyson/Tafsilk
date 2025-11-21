using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Services;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// Controller for public tailor portfolio views
/// </summary>
[Route("tailor-portfolio")]
public class TailorPortfolioController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<TailorPortfolioController> _logger;
    // ✅ NEW: Inject portfolio service for clean architecture
    private readonly IPortfolioService _portfolioService;

    public TailorPortfolioController(
        AppDbContext db,
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
                .Include(t => t.TailorServices.Where(s => !s.IsDeleted))
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tailor == null)
            {
                _logger.LogWarning("Tailor profile not found: {TailorId}", id);
                TempData["Error"] = "الملف الشخصي للخياط غير موجود";
                return RedirectToAction("Index", "Home");
            }

            // Use service layer to get portfolio images
            var portfolioImages = await _portfolioService.GetPublicPortfolioAsync(id) ?? Enumerable.Empty<Models.PortfolioImage>();

            // Ensure navigation collections are not null to avoid NRE in views
            tailor.PortfolioImages = portfolioImages.ToList();
            tailor.TailorServices = tailor.TailorServices ?? new List<Models.TailorService>();
            tailor.User = tailor.User ?? new Models.User();

            // Calculate statistics with safety guards
            ViewBag.ServiceCount = tailor.TailorServices?.Count ?? 0;
            ViewBag.PortfolioCount = tailor.PortfolioImages?.Count ?? 0;
            ViewBag.ReviewCount = 0; // Simplified - no reviews
            ViewBag.AverageRating = tailor.AverageRating;
            ViewBag.CompletedOrders = await _db.Orders.CountAsync(o =>
                o.TailorId == id &&
                o.Status == Models.OrderStatus.Delivered);

            return View(tailor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading public tailor profile for {TailorId}", id);
            TempData["Error"] = "حدث خطأ أثناء تحميل الملف الشخصي";
            return RedirectToAction("Index", "Home");
        }
    }
}
