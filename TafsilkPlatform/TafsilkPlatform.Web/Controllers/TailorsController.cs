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
    private readonly TafsilkPlatform.Web.Services.Interfaces.IReviewService _reviewService;

    public TailorsController(ApplicationDbContext db, ILogger<TailorsController> logger, TafsilkPlatform.Web.Services.Interfaces.IReviewService reviewService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _reviewService = reviewService ?? throw new ArgumentNullException(nameof(reviewService));
    }

    /// <summary>
    /// Browse all verified tailors
    /// GET: /tailors or /tailors/index
    /// </summary>
    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index(string? search = null, string? city = null, string? specialization = null, string? filter = null, int page = 1, int pageSize = 12)
    {
        try
        {
            var query = _db.TailorProfiles
                .Include(t => t.User)
                .Include(t => t.PortfolioImages.Where(p => !p.IsDeleted))
                .Where(t => t.User.IsActive && !t.User.IsDeleted);

            // Search by name or specialization
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(t =>
                    (t.ShopName != null && t.ShopName.Contains(search)) ||
                    (t.FullName != null && t.FullName.Contains(search)) ||
                    (t.Specialization != null && t.Specialization.Contains(search)));
            }

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

            // Apply sorting based on filter
            switch (filter)
            {
                case "Verified":
                    // Sort by name for "Verified" (or just general browsing)
                    query = query.OrderBy(t => t.ShopName).ThenBy(t => t.FullName);
                    break;
                case "TopRated":
                default:
                    // Default to Top Rated
                    // Treat 0 rating (no reviews) as 5 for sorting purposes to give new tailors a chance
                    query = query.OrderByDescending(t => t.AverageRating == 0 ? 5 : t.AverageRating).ThenBy(t => t.ShopName);
                    break;
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Get tailors with pagination
            var tailors = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get available cities for filter
            var cities = await _db.TailorProfiles
                .Where(t => !string.IsNullOrEmpty(t.City))
           .Select(t => t.City)
          .Distinct()
      .OrderBy(c => c)
  .ToListAsync();

            // Get available specializations for filter
            var specializations = await _db.TailorProfiles
                .Where(t => !string.IsNullOrEmpty(t.Specialization))
               .Select(t => t.Specialization)
             .Distinct()
                  .OrderBy(s => s)
           .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCity = city;
            ViewBag.CurrentSpecialization = specialization;
            ViewBag.Cities = cities;
            ViewBag.Specializations = specializations;

            return View(tailors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tailors");
            TempData["Error"] = "An error occurred while loading tailors";
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
                return NotFound("Tailor not found");
            }

            // Only show verified tailors to public - REMOVED per user request
            // if (!tailor.IsVerified)
            // {
            //     _logger.LogWarning("Attempt to view unverified tailor {TailorId}", id);
            //     return NotFound("Tailor not found");
            // }

            // Reviews with pagination
            int page = 1;
            int pageSize = 5;
            if (Request.Query.ContainsKey("page") && int.TryParse(Request.Query["page"], out int p))
            {
                page = p;
            }

            var reviews = await _reviewService.GetTailorReviewsAsync(id, page, pageSize);
            ViewBag.Reviews = reviews;
            
            // For pagination
            ViewBag.ReviewPage = page;
            ViewBag.ReviewPageSize = pageSize;
            ViewBag.ReviewCount = reviews.Count; // This is page count, need total.
            // Using tailor.ReviewCount if available, or fetch total count later. 
            // The model TailorProfile has ReviewCount? Let's check. 
            // In ReviewService.UpdateTailorRatingAsync we see: product.ReviewCount, but for TailorProfile it updates AverageRating. 
            // Does TailorProfile have ReviewCount? 
            // Let's assume passed in ViewBag.ReviewCount is total for now or fix model if needed.
            // Wait, previous code had `ViewBag.ReviewCount = 0;`. 
            // We can get total count from DB if needed, or if AverageRating is there, maybe we can assume ReviewCount is stored on TailorProfile too?
            // In ReviewService.cs:46 `product.ReviewCount` is used.
            // In ReviewService.cs:101 `tailor.AverageRating` is used. It doesn't seem to update `tailor.ReviewCount`.
            
            // To get accurate total count for pagination:
             var totalReviews = await _db.Reviews.CountAsync(r => r.Product.TailorId == id);
             ViewBag.TotalReviews = totalReviews;
             ViewBag.TotalReviewPages = (int)Math.Ceiling((double)totalReviews / pageSize);

            // Calculate statistics
            ViewBag.TotalOrders = await _db.Orders.CountAsync(o => o.TailorId == id);
            ViewBag.CompletedOrders = await _db.Orders
                .CountAsync(o => o.TailorId == id && o.Status == OrderStatus.Delivered);

            return View(tailor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tailor details {TailorId}", id);
            TempData["Error"] = "An error occurred while loading tailor details";
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

            if (tailor == null)
            {
                return NotFound();
            }

            // Prefer filesystem-backed image
            if (!string.IsNullOrWhiteSpace(tailor.ProfileImageUrl))
            {
                var imgUrl = tailor.ProfileImageUrl;

                if (Uri.IsWellFormedUriString(imgUrl, UriKind.Absolute))
                {
                    return Redirect(imgUrl);
                }

                if (imgUrl.StartsWith('/'))
                {
                    var relativePath = imgUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
                    if (System.IO.File.Exists(physicalPath))
                    {
                        var contentType = tailor.ProfilePictureContentType ?? "image/jpeg";
                        return PhysicalFile(physicalPath, contentType);
                    }
                }

                return Redirect(imgUrl);
            }

            // Fallback to DB stored image
            if (tailor.ProfilePictureData == null)
            {
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
                  .Where(t => t.User.IsActive && !t.User.IsDeleted);

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
            TempData["Error"] = "An error occurred while searching";
            return RedirectToAction(nameof(Index));
        }
    }
}
