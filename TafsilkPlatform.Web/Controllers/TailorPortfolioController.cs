using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Data;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// Controller for public tailor portfolio views
/// </summary>
[Route("tailor-portfolio")]
public class TailorPortfolioController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<TailorPortfolioController> _logger;

    public TailorPortfolioController(
        AppDbContext db,
        ILogger<TailorPortfolioController> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        .Include(t => t.PortfolioImages.Where(p => !p.IsDeleted))
       .Include(t => t.Reviews)
 .ThenInclude(r => r.Customer)
            .FirstOrDefaultAsync(t => t.Id == id);

          if (tailor == null)
       {
    _logger.LogWarning("Tailor profile not found: {TailorId}", id);
   TempData["Error"] = "الملف الشخصي للخياط غير موجود";
    return RedirectToAction("Index", "Home");
            }

     // Calculate statistics
            ViewBag.ServiceCount = tailor.TailorServices.Count;
            ViewBag.PortfolioCount = tailor.PortfolioImages.Count;
 ViewBag.ReviewCount = tailor.Reviews.Count;
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
