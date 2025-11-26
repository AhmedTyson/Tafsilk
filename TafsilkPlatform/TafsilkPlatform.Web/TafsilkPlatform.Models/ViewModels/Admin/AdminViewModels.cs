using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.Models.ViewModels.Admin;

// ============================================
// DASHBOARD HOME - ENHANCED VERSION
// ============================================
public class DashboardHomeViewModel
{
    // Basic Metrics
    public int TotalUsers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalTailors { get; set; }
    public int VerifiedTailors { get; set; }
    public int UnverifiedTailors { get; set; }
    public int PendingTailorVerifications { get; set; }
    public int PendingPortfolioReviews { get; set; }

    // Order Metrics
    public int TotalOrders { get; set; }
    public int ActiveOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int PendingOrders { get; set; }

    // Revenue Metrics
    public decimal TotalSales { get; set; } // Gross Sales
    public decimal TotalRevenue { get; set; } // Net Commission
    public decimal RevenueToday { get; set; }
    public decimal RevenueThisWeek { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal AverageOrderValue { get; set; }

    // Growth Metrics (compared to previous period)
    public decimal UserGrowthPercentage { get; set; }
    public decimal OrderGrowthPercentage { get; set; }
    public decimal RevenueGrowthPercentage { get; set; }
    public decimal TailorGrowthPercentage { get; set; }

    // Recent Activity
    public List<ActivityLogDto> RecentActivity { get; set; } = new();

    // Chart Data
    public List<OrdersByDayDto> OrdersByDay { get; set; } = new();
    public List<RevenueByMonthDto> RevenueByMonth { get; set; } = new();
    public List<UserRegistrationDto> UserRegistrations { get; set; } = new();
    public OrderStatusDistributionDto OrderStatusDistribution { get; set; } = new();

    // Recent Records
    public List<OrderSummaryDto> RecentOrders { get; set; } = new();
    public List<UserSummaryDto> RecentSignups { get; set; } = new();

    // System Health
    public SystemHealthDto SystemHealth { get; set; } = new();
}

// ============================================
// ACTIVITY LOG DTO
// ============================================
public class ActivityLogDto
{
    public string Action { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}

// Alias for backward compatibility
public class ActivityLogViewModel : ActivityLogDto
{
}

// ============================================
// CHART DATA DTOs
// ============================================

// DTO for Orders by Day Chart
public class OrdersByDayDto
{
    public DateOnly Date { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }

    public string DateLabel => Date.ToString("dd MMM");
}

// DTO for Revenue by Month Chart
public class RevenueByMonthDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }

    public string MonthName => new DateTime(Year, Month, 1).ToString("MMM yyyy");
}

// DTO for User Registrations Trend
public class UserRegistrationDto
{
    public DateOnly Date { get; set; }
    public int CustomerCount { get; set; }
    public int TailorCount { get; set; }

    public string DateLabel => Date.ToString("dd MMM");
    public int TotalCount => CustomerCount + TailorCount;
}

// DTO for Order Status Distribution (Pie Chart)
public class OrderStatusDistributionDto
{
    public int Pending { get; set; }
    public int Processing { get; set; }
    public int Shipped { get; set; }
    public int Delivered { get; set; }
    public int Cancelled { get; set; }

    public int Total => Pending + Processing + Shipped + Delivered + Cancelled;
}

// DTO for Recent Orders
public class OrderSummaryDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string TailorName { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
}

// DTO for Recent Signups
public class UserSummaryDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime SignedUpAt { get; set; }
    public string? PhoneNumber { get; set; }
}

// DTO for System Health Monitoring
public class SystemHealthDto
{
    public int DatabaseResponseTimeMs { get; set; }
    public decimal CpuUsagePercent { get; set; }
    public decimal MemoryUsagePercent { get; set; }
    public int ActiveConnections { get; set; }
    public DateTime LastBackup { get; set; }
    public bool IsHealthy => DatabaseResponseTimeMs < 1000 && CpuUsagePercent < 80;
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
    public List<ActivityLogDto> Logs { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
