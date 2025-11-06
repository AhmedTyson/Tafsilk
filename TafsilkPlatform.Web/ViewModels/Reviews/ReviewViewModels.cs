using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TafsilkPlatform.Web.ViewModels.Reviews;

// ============================================
// CREATE REVIEW
// ============================================

/// <summary>
/// ViewModel for submitting a new review
/// </summary>
public class CreateReviewViewModel
{
    public Guid OrderId { get; set; }
 public Guid TailorId { get; set; }
  public string TailorName { get; set; } = string.Empty;
 public string ServiceType { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
 public decimal OrderPrice { get; set; }
}

/// <summary>
/// Request model for creating a review
/// </summary>
public class CreateReviewRequest
{
    [Required(ErrorMessage = "Overall rating is required")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
  public int OverallRating { get; set; }

    [Required(ErrorMessage = "Review text is required")]
  [StringLength(1000, MinimumLength = 10, ErrorMessage = "Review must be between 10 and 1000 characters")]
  public string ReviewText { get; set; } = string.Empty;

    // Dimension ratings (Quality, Communication, Timeliness, Pricing)
    // FIXED: Changed from decimal to int to match RatingDimension.Score type
    public Dictionary<string, int> DimensionRatings { get; set; } = new();

    // Optional review photos
  public List<IFormFile>? Photos { get; set; }

// Recommendation
    public bool WouldRecommend { get; set; } = true;
}

/// <summary>
/// Request model for updating a review
/// </summary>
public class UpdateReviewRequest
{
 [Required]
    [Range(1, 5)]
    public int OverallRating { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string ReviewText { get; set; } = string.Empty;

    // FIXED: Changed from decimal to int
    public Dictionary<string, int> DimensionRatings { get; set; } = new();
}

// ============================================
// REVIEW DISPLAY
// ============================================

/// <summary>
/// ViewModel for displaying a single review
/// </summary>
public class ReviewDisplayDto
{
    public Guid ReviewId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
  // FIXED: Changed from decimal to int
  public Dictionary<string, int> DimensionRatings { get; set; } = new();
  public List<string> PhotoUrls { get; set; } = new();
    public bool IsVerifiedPurchase { get; set; } = true;
    
    // Helpful votes
  public int HelpfulCount { get; set; }
 public int UnhelpfulCount { get; set; }
}

/// <summary>
/// Detailed review information
/// </summary>
public class ReviewDetailsViewModel
{
    public Guid ReviewId { get; set; }
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string TailorName { get; set; } = string.Empty;
  public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    // FIXED: Changed from decimal to int
    public Dictionary<string, int> DimensionRatings { get; set; } = new();
    public List<string> PhotoUrls { get; set; } = new();
}

// ============================================
// TAILOR REVIEWS
// ============================================

/// <summary>
/// ViewModel for tailor reviews page with analytics
/// </summary>
public class TailorReviewsViewModel
{
    public Guid TailorId { get; set; }
    public string TailorName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    
    // Rating Distribution (5 stars: 50, 4 stars: 30, etc.)
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
  
    // Dimension Ratings (Quality: 4.5, Communication: 4.8, etc.)
    // Note: Keep as decimal for display (calculated average)
  public Dictionary<string, decimal> DimensionRatings { get; set; } = new();
    
 // Reviews list
    public List<ReviewDisplayDto> Reviews { get; set; } = new();
    
    // Pagination
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string SortBy { get; set; } = "recent";
    
    // Helper properties
    public int FiveStarCount => RatingDistribution.GetValueOrDefault(5, 0);
    public int FourStarCount => RatingDistribution.GetValueOrDefault(4, 0);
  public int ThreeStarCount => RatingDistribution.GetValueOrDefault(3, 0);
    public int TwoStarCount => RatingDistribution.GetValueOrDefault(2, 0);
    public int OneStarCount => RatingDistribution.GetValueOrDefault(1, 0);
    
    public int FiveStarPercentage => TotalReviews > 0 ? (FiveStarCount * 100) / TotalReviews : 0;
    public int FourStarPercentage => TotalReviews > 0 ? (FourStarCount * 100) / TotalReviews : 0;
    public int ThreeStarPercentage => TotalReviews > 0 ? (ThreeStarCount * 100) / TotalReviews : 0;
    public int TwoStarPercentage => TotalReviews > 0 ? (TwoStarCount * 100) / TotalReviews : 0;
 public int OneStarPercentage => TotalReviews > 0 ? (OneStarCount * 100) / TotalReviews : 0;
}

// ============================================
// PORTFOLIO MANAGEMENT
// ============================================

/// <summary>
/// ViewModel for managing tailor portfolio
/// </summary>
public class PortfolioManagementViewModel
{
    public Guid TailorId { get; set; }
 public string TailorName { get; set; } = string.Empty;
    public List<PortfolioImageDto> PortfolioImages { get; set; } = new();
    public int TotalImages { get; set; }
  public int MaxImages { get; set; } = 100;
  public bool CanAddMore => TotalImages < MaxImages;
}

/// <summary>
/// Portfolio image DTO
/// </summary>
public class PortfolioImageDto
{
    public Guid ImageId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
  public bool IsBeforeAfter { get; set; }
    public string? Caption { get; set; }
    public DateTime UploadedDate { get; set; }
}

/// <summary>
/// Request for uploading portfolio image
/// </summary>
public class UploadPortfolioImageRequest
{
  [Required]
    public IFormFile Image { get; set; } = null!;
    
    [StringLength(500)]
    public string? Caption { get; set; }
    
    public bool IsBeforeAfter { get; set; }
}
