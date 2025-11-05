using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.ViewModels.Admin;

// ============================================
// DASHBOARD HOME
// ============================================
public class DashboardHomeViewModel
{
    public int TotalUsers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalTailors { get; set; }
    public int PendingTailorVerifications { get; set; }
    public int PendingPortfolioReviews { get; set; }
    public int ActiveOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<ActivityLogDto> RecentActivity { get; set; } = new();
}

// Alias for backward compatibility
public class ActivityLogViewModel : ActivityLogDto
{
}

public class ActivityLogDto
{
    public string Action { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}

// ============================================
// USER MANAGEMENT
// ============================================
public class UserManagementViewModel
{
    public List<User> Users { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string? SearchTerm { get; set; }
    public string? SelectedRole { get; set; }
    public string? SelectedStatus { get; set; }
}

// ============================================
// TAILOR VERIFICATION
// ============================================
public class TailorVerificationViewModel
{
    public List<TailorProfile> Tailors { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string? SelectedStatus { get; set; }
}

// ============================================
// PORTFOLIO REVIEW
// ============================================
public class PortfolioReviewViewModel
{
    public List<PortfolioImage> Images { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string? SelectedStatus { get; set; }
}

// ============================================
// ORDER MANAGEMENT
// ============================================
public class OrderManagementViewModel
{
    public List<Order> Orders { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string? SelectedStatus { get; set; }
}

// ============================================
// REVIEW MODERATION
// ============================================
public class ReviewModerationViewModel
{
    public List<Review> Reviews { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string? Filter { get; set; }
}

// ============================================
// ANALYTICS
// ============================================
public class AnalyticsViewModel
{
    public int TotalUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public List<TailorPerformanceDto> TopTailors { get; set; } = new();
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
}

public class TailorPerformanceDto
{
    public Guid TailorId { get; set; }
    public string TailorName { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int TotalOrders { get; set; }
    public decimal Revenue { get; set; }
}

public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }

    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
}

// ============================================
// NOTIFICATIONS
// ============================================
public class SendNotificationDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info"; // Info, Success, Warning, Error
    public string TargetType { get; set; } = "All"; // All, Role, Specific
    public string TargetValue { get; set; } = string.Empty;
}

// ============================================
// AUDIT LOGS
// ============================================
public class AuditLogViewModel
{
    public List<ActivityLogDto> Logs { get; set; } = new(); // Use local ActivityLogDto instead
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
