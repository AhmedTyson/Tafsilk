using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// Public controller for browsing and viewing tailors
/// Accessible to all users (authenticated and anonymous)
/// </summary>
[Route("tailors")]
public class TailorsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<TailorsController> _logger;

public TailorsController(ApplicationDbContext db, ILogger<TailorsController> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Browse all verified tailors
    /// GET: /tailors or /tailors/index
    /// </summary>
    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index(string? city = null, string? specialization = null, int page = 1, int pageSize = 12)
    {
     try
        {
            var query = _db.TailorProfiles
   .Include(t => t.User)
                .Where(t => t.IsVerified && t.User.IsActive && !t.User.IsDeleted);

            // Filter by city
    if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(t => t.City == city);
            }

       // Filter by specialization
       if (!string.IsNullOrEmpty(specialization))
            {
     query = query.Where(t => t.Specialization != null && t.Specialization.Contains(specialization));
            }

         // Get total count for pagination
            var totalCount = await query.CountAsync();

      // Get tailors with pagination
     var tailors = await query
      .OrderByDescending(t => t.AverageRating)
              .ThenBy(t => t.ShopName)
     .Skip((page - 1) * pageSize)
       .Take(pageSize)
         .ToListAsync();

   // Get available cities for filter
            var cities = await _db.TailorProfiles
 .Where(t => t.IsVerified && !string.IsNullOrEmpty(t.City))
           .Select(t => t.City)
          .Distinct()
      .OrderBy(c => c)
  .ToListAsync();

     // Get available specializations for filter
      var specializations = await _db.TailorProfiles
           .Where(t => t.IsVerified && !string.IsNullOrEmpty(t.Specialization))
         .Select(t => t.Specialization)
       .Distinct()
            .OrderBy(s => s)
     .ToListAsync();

            ViewBag.CurrentPage = page;
     ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;
     ViewBag.CurrentCity = city;
    ViewBag.CurrentSpecialization = specialization;
          ViewBag.Cities = cities;
       ViewBag.Specializations = specializations;

   return View(tailors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tailors");
     TempData["Error"] = "حدث خطأ أثناء تحميل الخياطين";
            return View(new List<TailorProfile>());
      }
    }

    /// <summary>
    /// View tailor details with services, portfolio, and reviews
    /// GET: /tailors/details/{id}
    /// </summary>
    [HttpGet("details/{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        try
{
 var tailor = await _db.TailorProfiles
     .Include(t => t.User)
   .Include(t => t.TailorServices.Where(s => !s.IsDeleted))
 .Include(t => t.PortfolioImages.Where(p => !p.IsDeleted))
  .FirstOrDefaultAsync(t => t.Id == id);

            if (tailor == null)
{
      _logger.LogWarning("Tailor {TailorId} not found", id);
   return NotFound("الخياط غير موجود");
        }

         // Only show verified tailors to public
     if (!tailor.IsVerified)
     {
      _logger.LogWarning("Attempt to view unverified tailor {TailorId}", id);
  return NotFound("الخياط غير موجود");
            }

            // Reviews simplified - no database
            ViewBag.Reviews = new List<object>();
            ViewBag.ReviewCount = 0;

            // Calculate statistics
     ViewBag.TotalOrders = await _db.Orders.CountAsync(o => o.TailorId == id);
 ViewBag.CompletedOrders = await _db.Orders
     .CountAsync(o => o.TailorId == id && o.Status == OrderStatus.Delivered);

return View(tailor);
        }
        catch (Exception ex)
        {
       _logger.LogError(ex, "Error loading tailor details {TailorId}", id);
    TempData["Error"] = "حدث خطأ أثناء تحميل تفاصيل الخياط";
       return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Get tailor profile picture
    /// GET: /tailors/picture/{id}
    /// </summary>
  [HttpGet("picture/{id:guid}")]
    public async Task<IActionResult> GetProfilePicture(Guid id)
    {
        try
        {
     var tailor = await _db.TailorProfiles
         .FirstOrDefaultAsync(t => t.Id == id);

 if (tailor == null || tailor.ProfilePictureData == null)
            {
    // Return default avatar
             return NotFound();
            }

  return File(tailor.ProfilePictureData, tailor.ProfilePictureContentType ?? "image/jpeg");
        }
        catch (Exception ex)
        {
  _logger.LogError(ex, "Error retrieving profile picture for tailor {TailorId}", id);
          return NotFound();
}
    }

    /// <summary>
    /// Get portfolio image
  /// GET: /tailors/portfolio/{id}
    /// </summary>
    [HttpGet("portfolio/{id:guid}")]
    public async Task<IActionResult> GetPortfolioImage(Guid id)
    {
        try
        {
   var image = await _db.PortfolioImages
       .FirstOrDefaultAsync(p => p.PortfolioImageId == id && !p.IsDeleted);

            if (image == null || image.ImageData == null)
    {
      return NotFound();
     }

   return File(image.ImageData, image.ContentType ?? "image/jpeg");
        }
 catch (Exception ex)
        {
    _logger.LogError(ex, "Error retrieving portfolio image {ImageId}", id);
  return NotFound();
    }
    }

    /// <summary>
    /// Search tailors (API-style endpoint)
    /// GET: /tailors/search
  /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(string? query = null, string? city = null)
    {
        try
        {
    var tailorsQuery = _db.TailorProfiles
    .Include(t => t.User)
          .Where(t => t.IsVerified && t.User.IsActive && !t.User.IsDeleted);

            if (!string.IsNullOrEmpty(query))
            {
           tailorsQuery = tailorsQuery.Where(t =>
         t.ShopName != null && t.ShopName.Contains(query) ||
        t.FullName != null && t.FullName.Contains(query) ||
   t.Specialization != null && t.Specialization.Contains(query));
            }

     if (!string.IsNullOrEmpty(city))
            {
   tailorsQuery = tailorsQuery.Where(t => t.City == city);
            }

       var tailors = await tailorsQuery
            .OrderByDescending(t => t.AverageRating)
 .Take(20)
    .ToListAsync();

        return View("Index", tailors);
        }
        catch (Exception ex)
        {
  _logger.LogError(ex, "Error searching tailors");
            TempData["Error"] = "حدث خطأ أثناء البحث";
         return RedirectToAction(nameof(Index));
        }
    }
}
