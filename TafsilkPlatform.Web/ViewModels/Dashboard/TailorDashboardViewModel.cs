using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.ViewModels.Dashboard;

/// <summary>
/// ViewModel for Tailor Dashboard
/// </summary>
public class TailorDashboardViewModel
{
    // Tailor Profile Info
    public Guid TailorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public string? City { get; set; }
    public decimal? AverageRating { get; set; }
    public int TotalReviews { get; set; }

    // Statistics
    public int NewOrdersCount { get; set; }
    public int ActiveOrdersCount { get; set; }
    public int CompletedOrdersCount { get; set; }
    public int TotalOrdersCount { get; set; }

    // Financial Stats
    public decimal MonthlyRevenue { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingPayments { get; set; }
    public decimal WalletBalance { get; set; }

    // Services Stats
    public int TotalServices { get; set; }
    public int ActiveServices { get; set; }
    public int PortfolioImagesCount { get; set; }

    // Recent Orders
    public List<RecentOrderDto> RecentOrders { get; set; } = new();

    // Rating Breakdown
    public RatingBreakdown RatingBreakdown { get; set; } = new();

    // Alerts
    public List<DashboardAlert> Alerts { get; set; } = new();

    // Performance Metrics
    public PerformanceMetrics Performance { get; set; } = new();
}

/// <summary>
/// DTO for recent order display
/// </summary>
public class RecentOrderDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveryDate { get; set; }
}

/// <summary>
/// Rating breakdown by star count
/// </summary>
public class RatingBreakdown
{
    public int FiveStars { get; set; }
    public int FourStars { get; set; }
    public int ThreeStars { get; set; }
    public int TwoStars { get; set; }
    public int OneStar { get; set; }

    public int TotalReviews => FiveStars + FourStars + ThreeStars + TwoStars + OneStar;

    public decimal FiveStarsPercentage => TotalReviews > 0 ? (FiveStars * 100.0m / TotalReviews) : 0;
    public decimal FourStarsPercentage => TotalReviews > 0 ? (FourStars * 100.0m / TotalReviews) : 0;
    public decimal ThreeStarsPercentage => TotalReviews > 0 ? (ThreeStars * 100.0m / TotalReviews) : 0;
    public decimal TwoStarsPercentage => TotalReviews > 0 ? (TwoStars * 100.0m / TotalReviews) : 0;
    public decimal OneStarPercentage => TotalReviews > 0 ? (OneStar * 100.0m / TotalReviews) : 0;
}

/// <summary>
/// Dashboard alert/notification
/// </summary>
public class DashboardAlert
{
    public string Type { get; set; } = "info"; // info, warning, success, danger
    public string Icon { get; set; } = "fa-info-circle";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
}

/// <summary>
/// Performance metrics
/// </summary>
public class PerformanceMetrics
{
    public decimal OrderGrowthPercentage { get; set; }
    public decimal RevenueGrowthPercentage { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal AverageCompletionTime { get; set; } // in days
    public decimal CustomerSatisfactionRate { get; set; }
    public int RepeatCustomersCount { get; set; }
}
