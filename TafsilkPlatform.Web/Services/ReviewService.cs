using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.ViewModels.Reviews;

namespace TafsilkPlatform.Web.Services;

/// <summary>
/// Service for managing reviews and ratings
/// Handles review submission, rating calculations, and analytics
/// </summary>
public interface IReviewService
{
    Task<ServiceResult<Guid>> SubmitReviewAsync(Guid orderId, Guid customerId, CreateReviewRequest request);
    Task<ServiceResult<bool>> UpdateReviewAsync(Guid reviewId, Guid customerId, UpdateReviewRequest request);
    Task<ServiceResult<bool>> DeleteReviewAsync(Guid reviewId, Guid customerId);
    Task<ReviewDetailsViewModel?> GetReviewDetailsAsync(Guid reviewId);
    Task<TailorReviewsViewModel> GetTailorReviewsAsync(Guid tailorId, int page = 1, string sortBy = "recent");
    Task<decimal> CalculateTailorAverageRatingAsync(Guid tailorId);
    Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid tailorId);
    Task<Dictionary<string, decimal>> GetDimensionRatingsAsync(Guid tailorId);
    Task<bool> CanCustomerReviewOrderAsync(Guid orderId, Guid customerId);
    Task<bool> HasCustomerReviewedOrderAsync(Guid orderId, Guid customerId);
}

public class ReviewService : IReviewService
{
    private readonly AppDbContext _db;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<ReviewService> _logger;

 public ReviewService(
        AppDbContext db,
        IFileUploadService fileUploadService,
        ILogger<ReviewService> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
     _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
 _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Submit a new review for a completed order
    /// </summary>
    public async Task<ServiceResult<Guid>> SubmitReviewAsync(
        Guid orderId,
      Guid customerId,
        CreateReviewRequest request)
    {
        try
        {
            _logger.LogInformation("[ReviewService] Submitting review for order {OrderId} by customer {CustomerId}", orderId, customerId);

   // ==================== VALIDATION ====================

            // Check if order exists and belongs to customer
   var order = await _db.Orders
          .Include(o => o.Customer)
          .Include(o => o.Tailor)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
         {
        return ServiceResult<Guid>.Failure("Order not found");
  }

    if (order.CustomerId != customerId)
            {
        return ServiceResult<Guid>.Failure("Unauthorized: Order does not belong to this customer");
 }

   // Order must be delivered
 if (order.Status != OrderStatus.Delivered)
            {
        return ServiceResult<Guid>.Failure("Can only review completed orders");
       }

            // Check if already reviewed
     var existingReview = await _db.Reviews
           .FirstOrDefaultAsync(r => r.OrderId == orderId && r.CustomerId == customerId);

            if (existingReview != null)
     {
    return ServiceResult<Guid>.Failure("You have already reviewed this order");
      }

    // Validate rating
            if (request.OverallRating < 1 || request.OverallRating > 5)
     {
   return ServiceResult<Guid>.Failure("Rating must be between 1 and 5");
            }

            // ==================== CREATE REVIEW ====================

        var review = new Review
       {
    ReviewId = Guid.NewGuid(),
          OrderId = orderId,
         CustomerId = customerId,
    TailorId = order.TailorId,
   Rating = request.OverallRating,
   Comment = request.ReviewText,
           CreatedAt = DateTime.UtcNow,
    IsDeleted = false
            };

    // Add dimension ratings
        if (request.DimensionRatings != null && request.DimensionRatings.Any())
     {
   foreach (var dimension in request.DimensionRatings)
            {
       review.RatingDimensions.Add(new RatingDimension
 {
           RatingDimensionId = Guid.NewGuid(),
      ReviewId = review.ReviewId,
           DimensionName = dimension.Key,
            Score = dimension.Value
       });
      }
            }

       // ==================== UPLOAD PHOTOS ====================

        // TODO: Implement photo upload when IFileUploadService is ready
         if (request.Photos != null && request.Photos.Any())
         {
   _logger.LogWarning("[ReviewService] Photo upload requested but IFileUploadService not implemented yet");
        // Placeholder: Photos will be stored when file service is implemented
        /*
   foreach (var photo in request.Photos)
  {
    // Validate file
     if (photo.Length > 5 * 1024 * 1024) // 5MB
      {
return ServiceResult<Guid>.Failure($"Photo '{photo.FileName}' exceeds 5MB limit");
       }

         // Upload to storage
    var uploadResult = await _fileUploadService.UploadFileAsync(
       photo,
   $"reviews/{customerId}");

     if (!uploadResult.Success)
    {
           _logger.LogError("[ReviewService] Failed to upload photo: {Error}", uploadResult.ErrorMessage);
 return ServiceResult<Guid>.Failure($"Failed to upload photo: {uploadResult.ErrorMessage}");
   }

          // Note: ReviewPhoto entity would need to be created
       }
         */
         }

            // ==================== SAVE REVIEW ====================

        await _db.Reviews.AddAsync(review);
      await _db.SaveChangesAsync();

    // ==================== UPDATE TAILOR RATING ====================

    await UpdateTailorRatingAsync(order.TailorId);

            _logger.LogInformation("[ReviewService] Review {ReviewId} submitted successfully", review.ReviewId);

            return ServiceResult<Guid>.Success(review.ReviewId, "Review submitted successfully!");
     }
        catch (Exception ex)
        {
      _logger.LogError(ex, "[ReviewService] Error submitting review for order {OrderId}", orderId);
            return ServiceResult<Guid>.Failure($"Error submitting review: {ex.Message}");
      }
    }

    /// <summary>
    /// Update an existing review
    /// </summary>
    public async Task<ServiceResult<bool>> UpdateReviewAsync(
        Guid reviewId,
     Guid customerId,
      UpdateReviewRequest request)
    {
   try
        {
            var review = await _db.Reviews
.Include(r => r.RatingDimensions)
          .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.CustomerId == customerId);

          if (review == null)
          {
return ServiceResult<bool>.Failure("Review not found or unauthorized");
            }

            // Update basic fields
         review.Rating = request.OverallRating;
            review.Comment = request.ReviewText;

  // Update dimension ratings
            if (request.DimensionRatings != null)
          {
 // Remove old dimensions
      _db.RatingDimensions.RemoveRange(review.RatingDimensions);

        // Add new dimensions
  foreach (var dimension in request.DimensionRatings)
                {
          review.RatingDimensions.Add(new RatingDimension
    {
      RatingDimensionId = Guid.NewGuid(),
             ReviewId = review.ReviewId,
          DimensionName = dimension.Key,
             Score = dimension.Value
    });
         }
  }

     await _db.SaveChangesAsync();

     // Recalculate tailor rating
 await UpdateTailorRatingAsync(review.TailorId);

        return ServiceResult<bool>.Success(true, "Review updated successfully!");
  }
      catch (Exception ex)
        {
          _logger.LogError(ex, "[ReviewService] Error updating review {ReviewId}", reviewId);
        return ServiceResult<bool>.Failure($"Error updating review: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a review (soft delete)
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteReviewAsync(Guid reviewId, Guid customerId)
 {
        try
   {
    var review = await _db.Reviews
           .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.CustomerId == customerId);

            if (review == null)
{
         return ServiceResult<bool>.Failure("Review not found or unauthorized");
    }

         review.IsDeleted = true;
            await _db.SaveChangesAsync();

    // Recalculate tailor rating
    await UpdateTailorRatingAsync(review.TailorId);

        return ServiceResult<bool>.Success(true, "Review deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ReviewService] Error deleting review {ReviewId}", reviewId);
            return ServiceResult<bool>.Failure($"Error deleting review: {ex.Message}");
    }
    }

    /// <summary>
    /// Get detailed review information
    /// </summary>
    public async Task<ReviewDetailsViewModel?> GetReviewDetailsAsync(Guid reviewId)
    {
        try
        {
            var review = await _db.Reviews
           .Include(r => r.Customer).ThenInclude(c => c.User)
           .Include(r => r.Tailor).ThenInclude(t => t.User)
    .Include(r => r.Order)
        .Include(r => r.RatingDimensions)
             .Where(r => r.ReviewId == reviewId && !r.IsDeleted)
           .Select(r => new ReviewDetailsViewModel
             {
           ReviewId = r.ReviewId,
         OrderId = r.OrderId,
  CustomerName = r.Customer != null ? r.Customer.FullName : "Unknown",
        TailorName = r.Tailor != null ? r.Tailor.ShopName : "Unknown",
   Rating = r.Rating,
  Comment = r.Comment,
           CreatedAt = r.CreatedAt,
   DimensionRatings = r.RatingDimensions.ToDictionary(
                   d => d.DimensionName,
 d => d.Score
  )
  })
        .FirstOrDefaultAsync();

         return review;
   }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ReviewService] Error getting review details for {ReviewId}", reviewId);
   return null;
        }
    }

    /// <summary>
    /// Get all reviews for a tailor with pagination and sorting
    /// </summary>
    public async Task<TailorReviewsViewModel> GetTailorReviewsAsync(
   Guid tailorId,
        int page = 1,
        string sortBy = "recent")
    {
        const int pageSize = 10;

  var tailor = await _db.TailorProfiles
        .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == tailorId);

        if (tailor == null)
    {
          return new TailorReviewsViewModel
            {
  TailorId = tailorId,
        Reviews = new List<ReviewDisplayDto>(),
 CurrentPage = 1,
      TotalPages = 0
        };
 }

        // Get reviews query
        var reviewsQuery = _db.Reviews
       .Include(r => r.Customer).ThenInclude(c => c.User)
     .Include(r => r.RatingDimensions)
      .Where(r => r.TailorId == tailorId && !r.IsDeleted);

   // Apply sorting
        reviewsQuery = sortBy.ToLower() switch
   {
"highest" => reviewsQuery.OrderByDescending(r => r.Rating),
            "lowest" => reviewsQuery.OrderBy(r => r.Rating),
      "recent" => reviewsQuery.OrderByDescending(r => r.CreatedAt),
            _ => reviewsQuery.OrderByDescending(r => r.CreatedAt)
        };

        // Get total count
        var totalCount = await reviewsQuery.CountAsync();
 var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Get page of reviews
        var reviews = await reviewsQuery
          .Skip((page - 1) * pageSize)
 .Take(pageSize)
       .Select(r => new ReviewDisplayDto
    {
                ReviewId = r.ReviewId,
                CustomerName = r.Customer != null ? r.Customer.FullName : "Unknown",
      Rating = r.Rating,
       Comment = r.Comment,
         CreatedAt = r.CreatedAt,
    DimensionRatings = r.RatingDimensions.ToDictionary(
   d => d.DimensionName,
             d => d.Score
)
        })
  .ToListAsync();

// Calculate statistics
var averageRating = await CalculateTailorAverageRatingAsync(tailorId);
        var ratingDistribution = await GetRatingDistributionAsync(tailorId);
        var dimensionRatings = await GetDimensionRatingsAsync(tailorId);

        return new TailorReviewsViewModel
   {
     TailorId = tailorId,
            TailorName = tailor.ShopName,
            AverageRating = averageRating,
            TotalReviews = totalCount,
         RatingDistribution = ratingDistribution,
            DimensionRatings = dimensionRatings,
        Reviews = reviews,
            CurrentPage = page,
 TotalPages = totalPages,
            SortBy = sortBy
 };
    }

    /// <summary>
    /// Calculate average rating for a tailor
    /// </summary>
    public async Task<decimal> CalculateTailorAverageRatingAsync(Guid tailorId)
    {
     var reviews = await _db.Reviews
            .Where(r => r.TailorId == tailorId && !r.IsDeleted)
       .ToListAsync();

     if (!reviews.Any())
            return 0;

        return (decimal)reviews.Average(r => r.Rating);
    }

    /// <summary>
    /// Get rating distribution (1-5 stars)
    /// </summary>
    public async Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid tailorId)
    {
        var reviews = await _db.Reviews
  .Where(r => r.TailorId == tailorId && !r.IsDeleted)
            .ToListAsync();

  return new Dictionary<int, int>
        {
  { 5, reviews.Count(r => r.Rating == 5) },
 { 4, reviews.Count(r => r.Rating == 4) },
            { 3, reviews.Count(r => r.Rating == 3) },
     { 2, reviews.Count(r => r.Rating == 2) },
   { 1, reviews.Count(r => r.Rating == 1) }
        };
  }

    /// <summary>
    /// Get average ratings by dimension (Quality, Communication, etc.)
    /// </summary>
    public async Task<Dictionary<string, decimal>> GetDimensionRatingsAsync(Guid tailorId)
    {
        var dimensions = await _db.RatingDimensions
   .Where(d => d.Review != null && d.Review.TailorId == tailorId && !d.Review.IsDeleted)
      .GroupBy(d => d.DimensionName)
 .Select(g => new
  {
             Dimension = g.Key,
    Average = g.Average(d => d.Score)
            })
            .ToDictionaryAsync(x => x.Dimension, x => (decimal)x.Average);

return dimensions;
    }

    /// <summary>
    /// Check if customer can review an order
    /// </summary>
    public async Task<bool> CanCustomerReviewOrderAsync(Guid orderId, Guid customerId)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == customerId);

        if (order == null)
     return false;

        // Order must be delivered
        if (order.Status != OrderStatus.Delivered)
        return false;

        // Check if already reviewed
        var hasReviewed = await HasCustomerReviewedOrderAsync(orderId, customerId);
  return !hasReviewed;
    }

    /// <summary>
    /// Check if customer has already reviewed an order
    /// </summary>
    public async Task<bool> HasCustomerReviewedOrderAsync(Guid orderId, Guid customerId)
    {
        return await _db.Reviews
 .AnyAsync(r => r.OrderId == orderId && r.CustomerId == customerId && !r.IsDeleted);
  }

    /// <summary>
    /// Update tailor's average rating and review count
    /// </summary>
    private async Task UpdateTailorRatingAsync(Guid tailorId)
    {
 try
 {
      var tailor = await _db.TailorProfiles.FindAsync(tailorId);
            if (tailor == null)
        return;

       var averageRating = await CalculateTailorAverageRatingAsync(tailorId);
   var reviewCount = await _db.Reviews
         .CountAsync(r => r.TailorId == tailorId && !r.IsDeleted);

  tailor.AverageRating = averageRating;
            tailor.UpdateRating(averageRating); // Use existing method
        // FIXED: Removed TotalReviews assignment - it's a computed property

       await _db.SaveChangesAsync();

            _logger.LogInformation("[ReviewService] Updated tailor {TailorId} rating to {Rating} ({Count} reviews)",
            tailorId, averageRating, reviewCount);
        }
   catch (Exception ex)
      {
   _logger.LogError(ex, "[ReviewService] Error updating tailor rating for {TailorId}", tailorId);
     }
    }
}
