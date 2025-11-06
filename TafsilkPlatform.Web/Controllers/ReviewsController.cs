using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Controllers.Base;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.ViewModels.Reviews;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// Controller for handling reviews and ratings
/// Customers can submit reviews for completed orders
/// </summary>
public class ReviewsController : BaseController
{
    private readonly AppDbContext _db;
    private readonly IReviewService _reviewService;

    public ReviewsController(
        AppDbContext db,
        IReviewService reviewService,
        ILogger<ReviewsController> logger) : base(logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _reviewService = reviewService ?? throw new ArgumentNullException(nameof(reviewService));
    }

    // ============================================
    // CREATE REVIEW
    // ============================================

  /// <summary>
    /// Display review submission form for a completed order
    /// GET: /Reviews/Create/{orderId} or /Reviews/SubmitReview/{orderId}
    /// </summary>
    [HttpGet]
    [Route("Reviews/Create/{orderId:guid}")]
    [Route("Reviews/SubmitReview/{orderId:guid}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create(Guid orderId)
 {
        try
        {
            var customerId = GetUserId();

 // Check if customer can review this order
   var canReview = await _reviewService.CanCustomerReviewOrderAsync(orderId, customerId);
   if (!canReview)
{
    TempData["Error"] = "لا يمكنك تقييم هذا الطلب. يجب أن يكون الطلب مكتملاً ولم يتم تقييمه بعد.";
      return RedirectToAction("MyOrders", "Orders");
       }

       // Fetch order details
   var order = await _db.Orders
      .Include(o => o.Tailor)
   .ThenInclude(t => t.User)
     .FirstOrDefaultAsync(o => o.OrderId == orderId);

  if (order == null)
  {
      return NotFound("Order not found");
     }

    var viewModel = new SubmitReviewRequest
         {
    OrderId = orderId,
        TailorId = order.TailorId
 };

         // Pass order and tailor info via ViewBag for display
            ViewBag.Order = order;
  ViewBag.Tailor = order.Tailor;

       return View("SubmitReview", viewModel);
      }
        catch (Exception ex)
        {
         _logger.LogError(ex, "Error loading review form for order {OrderId}", orderId);
 TempData["Error"] = "حدث خطأ أثناء تحميل صفحة التقييم";
    return RedirectToAction("MyOrders", "Orders");
        }
    }

    /// <summary>
    /// Submit a new review
    /// POST: /Reviews/Create or /Reviews/SubmitReview
    /// </summary>
  [HttpPost]
    [Route("Reviews/Create")]
    [Route("Reviews/SubmitReview")]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
public async Task<IActionResult> SubmitReview(SubmitReviewRequest request)
  {
   try
    {
   if (!ModelState.IsValid)
{
 TempData["Error"] = "يرجى تصحيح الأخطاء في النموذج";
    return RedirectToAction(nameof(Create), new { orderId = request.OrderId });
 }

    var customerId = GetUserId();

 // Check if customer can review
      var canReview = await _reviewService.CanCustomerReviewOrderAsync(request.OrderId, customerId);
   if (!canReview)
  {
    TempData["Error"] = "لا يمكنك تقييم هذا الطلب";
          return RedirectToAction("MyOrders", "Orders");
   }

 // Create review using the service - map to CreateReviewRequest format
   var createRequest = new CreateReviewRequest
   {
    OverallRating = request.Rating,
   ReviewText = request.Comment ?? string.Empty,
        DimensionRatings = new Dictionary<string, int>
      {
  { "Quality", request.QualityRating ?? request.Rating },
       { "Communication", request.CommunicationRating ?? request.Rating },
         { "Timeliness", request.TimelinessRating ?? request.Rating },
   { "Professionalism", request.ProfessionalismRating ?? request.Rating }
        },
    WouldRecommend = request.Rating >= 4
    };

  var result = await _reviewService.SubmitReviewAsync(request.OrderId, customerId, createRequest);

    if (!result.IsSuccess)
 {
         TempData["Error"] = result.ErrorMessage;
    return RedirectToAction(nameof(Create), new { orderId = request.OrderId });
     }

   _logger.LogInformation("Review submitted successfully for order {OrderId} by customer {CustomerId}", 
     request.OrderId, customerId);

      TempData["Success"] = "شكراً لتقييمك! تم إرسال تقييمك بنجاح.";
 return RedirectToAction("MyOrders", "Orders");
 }
 catch (Exception ex)
     {
_logger.LogError(ex, "Error submitting review for order {OrderId}", request.OrderId);
    TempData["Error"] = "حدث خطأ أثناء إرسال التقييم";
    return RedirectToAction(nameof(Create), new { orderId = request.OrderId });
  }
    }

    /// <summary>
    /// Review submission success page
    /// GET: /Reviews/Success/{reviewId}
    /// </summary>
    [HttpGet]
    [Route("Reviews/Success/{reviewId:guid}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> ReviewSuccess(Guid reviewId)
    {
  try
        {
  var review = await _reviewService.GetReviewDetailsAsync(reviewId);

   if (review == null)
            {
      return NotFound("Review not found");
      }

            return View(review);
      }
        catch (Exception ex)
    {
       _logger.LogError(ex, "Error loading review success page for {ReviewId}", reviewId);
            return RedirectToAction("MyOrders", "Orders");
     }
    }

    // ============================================
    // VIEW TAILOR REVIEWS
    // ============================================

    /// <summary>
    /// Display all reviews for a tailor with ratings analytics
    /// GET: /Reviews/Tailor/{tailorId}
    /// </summary>
    [HttpGet]
    [Route("Reviews/Tailor/{tailorId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> TailorReviews(Guid tailorId, string sortBy = "recent", int page = 1)
    {
        try
        {
     _logger.LogInformation("Loading reviews for tailor {TailorId}, page {Page}, sort {SortBy}", tailorId, page, sortBy);

  var viewModel = await _reviewService.GetTailorReviewsAsync(tailorId, page, sortBy);

       return View(viewModel);
        }
  catch (Exception ex)
    {
 _logger.LogError(ex, "Error loading reviews for tailor {TailorId}", tailorId);
            TempData["Error"] = "حدث خطأ أثناء تحميل التقييمات";
      return RedirectToAction("Index", "Home");
        }
    }

    // ============================================
    // EDIT/DELETE REVIEW
    // ============================================

    /// <summary>
    /// Edit an existing review
    /// GET: /Reviews/Edit/{reviewId}
    /// </summary>
    [HttpGet]
    [Route("Reviews/Edit/{reviewId:guid}")]
  [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Edit(Guid reviewId)
    {
     try
        {
            var customerId = GetUserId();

          var review = await _db.Reviews
     .Include(r => r.Order)
      .Include(r => r.Tailor)
        .Include(r => r.RatingDimensions)
  .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.CustomerId == customerId);

       if (review == null)
            {
        TempData["Error"] = "التقييم غير موجود أو غير مصرح لك بتعديله";
      return RedirectToAction("MyOrders", "Orders");
            }

   var viewModel = new CreateReviewViewModel
        {
    OrderId = review.OrderId,
         TailorId = review.TailorId,
                TailorName = review.Tailor?.ShopName ?? "Unknown",
    ServiceType = review.Order?.OrderType ?? "Unknown",
          OrderDate = review.Order?.CreatedAt.DateTime ?? DateTime.Now,
       OrderPrice = review.Order != null ? (decimal)review.Order.TotalPrice : 0
            };

            ViewBag.ExistingRating = review.Rating;
        ViewBag.ExistingComment = review.Comment;
 ViewBag.ExistingDimensions = review.RatingDimensions.ToDictionary(d => d.DimensionName, d => d.Score);
          ViewBag.IsEdit = true;
          ViewBag.ReviewId = reviewId;

          return View("Create", viewModel);
  }
        catch (Exception ex)
        {
        _logger.LogError(ex, "Error loading edit form for review {ReviewId}", reviewId);
            TempData["Error"] = "حدث خطأ أثناء تحميل صفحة التعديل";
    return RedirectToAction("MyOrders", "Orders");
    }
    }

    /// <summary>
    /// Update an existing review
   /// POST: /Reviews/Edit/{reviewId}
    /// </summary>
    [HttpPost]
    [Route("Reviews/Edit/{reviewId:guid}")]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid reviewId, UpdateReviewRequest request)
    {
 try
   {
      if (!ModelState.IsValid)
     {
      TempData["Error"] = "يرجى تصحيح الأخطاء في النموذج";
 return RedirectToAction(nameof(Edit), new { reviewId });
    }

 var customerId = GetUserId();

 var result = await _reviewService.UpdateReviewAsync(reviewId, customerId, request);

     if (!result.IsSuccess)
  {
      TempData["Error"] = result.ErrorMessage;
 return RedirectToAction(nameof(Edit), new { reviewId });
 }

  TempData["Success"] = "تم تحديث تقييمك بنجاح!";
  return RedirectToAction("MyOrders", "Orders");
 }
 catch (Exception ex)
      {
 _logger.LogError(ex, "Error updating review {ReviewId}", reviewId);
TempData["Error"] = "حدث خطأ أثناء تحديث التقييم";
        return RedirectToAction(nameof(Edit), new { reviewId });
        }
    }

    /// <summary>
/// Delete a review
    /// POST: /Reviews/Delete/{reviewId}
    /// </summary>
    [HttpPost]
  [Route("Reviews/Delete/{reviewId:guid}")]
  [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(Guid reviewId)
    {
    try
      {
   var customerId = GetUserId();

    var result = await _reviewService.DeleteReviewAsync(reviewId, customerId);

  if (!result.IsSuccess)
     {
TempData["Error"] = result.ErrorMessage;
        }
else
       {
 TempData["Success"] = "تم حذف التقييم بنجاح";
   }

  return RedirectToAction("MyOrders", "Orders");
    }
    catch (Exception ex)
    {
     _logger.LogError(ex, "Error deleting review {ReviewId}", reviewId);
       TempData["Error"] = "حدث خطأ أثناء حذف التقييم";
     return RedirectToAction("MyOrders", "Orders");
  }
    }

    // ============================================
    // PORTFOLIO MANAGEMENT (Tailor)
    // ============================================

    /// <summary>
    /// Manage tailor portfolio images
    /// GET: /Reviews/Portfolio
    /// </summary>
  [HttpGet]
    [Route("Reviews/Portfolio")]
  [Authorize(Roles = "Tailor")]
    public async Task<IActionResult> PortfolioManagement()
{
      try
   {
            var tailorId = GetUserId();

     var tailor = await _db.TailorProfiles
    .FirstOrDefaultAsync(t => t.UserId == tailorId);

   if (tailor == null)
   {
TempData["Error"] = "الملف الشخصي للخياط غير موجود";
       return RedirectToAction("Index", "Home");
 }

   var portfolioImages = await _db.PortfolioImages
       .Where(p => p.TailorId == tailor.Id && !p.IsDeleted)
    .OrderByDescending(p => p.UploadedAt)
    .ToListAsync();

   var viewModel = new PortfolioManagementViewModel
          {
    TailorId = tailor.Id,
           TailorName = tailor.ShopName,
PortfolioImages = portfolioImages.Select(p => new PortfolioImageDto
       {
   // FIXED: Use PortfolioImageId instead of Id
        ImageId = p.PortfolioImageId,
            ImageUrl = p.ImageUrl ?? string.Empty,
         IsBeforeAfter = p.IsBeforeAfter,
 // FIXED: Use Description instead of Caption
 Caption = p.Description,
       UploadedDate = p.UploadedAt
      }).ToList(),
      TotalImages = portfolioImages.Count,
        MaxImages = 100
     };

     return View(viewModel);
        }
  catch (Exception ex)
    {
      _logger.LogError(ex, "Error loading portfolio management");
    TempData["Error"] = "حدث خطأ أثناء تحميل معرض الأعمال";
         return RedirectToAction("Index", "Home");
        }
}

    /// <summary>
    /// Upload portfolio image
    /// POST: /Reviews/Portfolio/Upload
    /// </summary>
    [HttpPost]
    [Route("Reviews/Portfolio/Upload")]
 [Authorize(Roles = "Tailor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPortfolioImage(UploadPortfolioImageRequest request)
    {
    try
        {
  if (!ModelState.IsValid)
   {
      return BadRequest(ModelState);
    }

  // Implementation would require IFileUploadService which may not exist yet
         // Placeholder for now
       TempData["Info"] = "ميزة رفع الصور قيد التطوير";
          return RedirectToAction(nameof(PortfolioManagement));
        }
        catch (Exception ex)
  {
        _logger.LogError(ex, "Error uploading portfolio image");
     TempData["Error"] = "حدث خطأ أثناء رفع الصورة";
    return RedirectToAction(nameof(PortfolioManagement));
      }
    }

    /// <summary>
/// Delete portfolio image
    /// POST: /Reviews/Portfolio/Delete/{imageId}
    /// </summary>
    [HttpPost]
  [Route("Reviews/Portfolio/Delete/{imageId:guid}")]
    [Authorize(Roles = "Tailor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePortfolioImage(Guid imageId)
  {
    try
        {
        var tailorId = GetUserId();

 var tailor = await _db.TailorProfiles
    .FirstOrDefaultAsync(t => t.UserId == tailorId);

    if (tailor == null)
 {
return Forbid();
    }

      // FIXED: Use PortfolioImageId instead of Id
      var image = await _db.PortfolioImages
    .FirstOrDefaultAsync(p => p.PortfolioImageId == imageId && p.TailorId == tailor.Id);

     if (image == null)
       {
         TempData["Error"] = "الصورة غير موجودة";
      return RedirectToAction(nameof(PortfolioManagement));
    }

image.IsDeleted = true;
     await _db.SaveChangesAsync();

      TempData["Success"] = "تم حذف الصورة بنجاح";
      return RedirectToAction(nameof(PortfolioManagement));
     }
     catch (Exception ex)
   {
            _logger.LogError(ex, "Error deleting portfolio image {ImageId}", imageId);
   TempData["Error"] = "حدث خطأ أثناء حذف الصورة";
       return RedirectToAction(nameof(PortfolioManagement));
        }
    }
}
