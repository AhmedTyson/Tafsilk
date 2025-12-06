using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.DataAccess.Repository;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels.Dashboard;
using TafsilkPlatform.Utility.Extensions;

namespace TafsilkPlatform.Web.Controllers;

[Authorize]
public class DashboardsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DashboardsController> _logger;

    public DashboardsController(
        IUnitOfWork unitOfWork,
        ApplicationDbContext context,
        ILogger<DashboardsController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Router action to redirect to the correct dashboard based on role
    /// GET: /Dashboards
    /// </summary>
    [HttpGet("")]
    public IActionResult Index()
    {
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction("Index", "AdminDashboard", new { area = "Admin" });
        }

        if (User.IsInRole("Tailor"))
        {
            return RedirectToAction("Tailor");
        }

        if (User.IsInRole("Customer"))
        {
            // Customers don't have a specific dashboard, redirect to store or profile
            return RedirectToAction("Index", "Store", new { area = "Customer" });
        }

        // Fallback
        return RedirectToAction("Index", "Home");
    }



    /// <summary>
    /// Redirect route for /Dashboards/Tailor/Add to correct product add route
    /// GET: /Dashboards/Tailor/Add
    /// </summary>
    [HttpGet("Tailor/Add")]
    [Authorize(Roles = "Tailor,Admin")]
    public IActionResult TailorAdd()
    {
        return RedirectToAction("AddProduct", "TailorManagement", new { area = "Tailor" });
    }

    [Authorize(Roles = "Tailor,Admin")]
    public async Task<IActionResult> Tailor()
    {
        try
        {
            var userId = User.GetUserId();

            // ✅ ADMIN/TESTER: Check if admin is testing
            var isAdmin = User.IsInRole("Admin");

            // CRITICAL: Check if tailor has completed verification
            // ✅ Use split query to avoid cartesian explosion with multiple collections
            var tailor = await _context.TailorProfiles
                .Include(t => t.User)
                .Include(t => t.TailorServices)
                .Include(t => t.PortfolioImages)
                .AsSplitQuery()
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (tailor == null && !isAdmin)
            {
                // Tailor has not provided evidence - MANDATORY redirect
                // ✅ Skip redirect for admins (they can see all pages)
                _logger.LogWarning("Tailor profile not found for user {UserId}. Redirecting to evidence submission.", userId);
                TempData["ErrorMessage"] = "You must submit documents and complete your profile first. This step is mandatory for tailors.";
                return RedirectToAction("ProvideTailorEvidence", "Account", new { incomplete = true });
            }

            // ✅ ADMIN/TESTER: Show demo data if no profile exists
            if (tailor == null && isAdmin)
            {
                ViewData["TestingMode"] = true;
                ViewData["TestingMessage"] = "🧪 Testing Mode: No tailor profile found. Showing demo dashboard.";
                return View(GetDemoTailorDashboard());
            }

            // Check if pending admin approval
            var isPendingApproval = HttpContext.Items["PendingApproval"] as bool? ?? false;
            if (!tailor.IsVerified || isPendingApproval)
            {
                ViewData["PendingApproval"] = true;
                ViewData["PendingMessage"] = "Your account is under review by administration. All features will be activated after approval (usually within 2-3 business days).";
            }

            // Build dashboard data
            var model = new TailorDashboardViewModel
            {
                TailorId = tailor.Id,
                FullName = tailor.FullName ?? "Tailor",
                ShopName = tailor.ShopName ?? "Unnamed Workshop",
                IsVerified = tailor.IsVerified,
                City = tailor.City,
            };

            // Get order statistics
            var orders = await _context.Orders
            .Where(o => o.TailorId == tailor.Id)
           .ToListAsync();

            model.TotalOrdersCount = orders.Count;
            model.NewOrdersCount = orders.Count(o => o.Status == OrderStatus.Pending);
            model.ActiveOrdersCount = orders.Count(o =>
            o.Status == OrderStatus.Processing ||
     o.Status == OrderStatus.Shipped);
            model.CompletedOrdersCount = orders.Count(o => o.Status == OrderStatus.Delivered);

            // Get financial statistics (NET REVENUE = Total - Commission)
            var completedOrders = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();
            model.TotalRevenue = (decimal)completedOrders.Sum(o => o.TotalPrice - o.CommissionAmount);

            var monthlyOrders = completedOrders
                .Where(o => o.CreatedAt >= DateTimeOffset.UtcNow.AddMonths(-1))
                .ToList();
            model.MonthlyRevenue = (decimal)monthlyOrders.Sum(o => o.TotalPrice - o.CommissionAmount);

            // Chart Data: Net Revenue Last 7 Days
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var chartData = completedOrders
                .Where(o => o.CreatedAt >= sevenDaysAgo)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, NetTotal = g.Sum(o => o.TotalPrice - o.CommissionAmount) })
                .OrderBy(x => x.Date)
                .ToList();

            // Fill in missing days
            var dates = new List<string>();
            var values = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.UtcNow.Date.AddDays(-i);
                var entry = chartData.FirstOrDefault(x => x.Date == date);
                dates.Add(date.ToString("MMM dd"));
                values.Add(entry != null ? (decimal)entry.NetTotal : 0);
            }

            model.SalesDates = dates.ToArray();
            model.SalesValues = values.ToArray();

            // Wallet balance not available (Wallet feature removed)
            model.WalletBalance = 0;

            // Get pending payments
            var pendingPayments = await _context.Payment
     .Where(p => p.TailorId == tailor.Id &&
    p.PaymentStatus == Enums.PaymentStatus.Pending)
    .SumAsync(p => (decimal?)p.Amount) ?? 0;
            model.PendingPayments = pendingPayments;

            // Get service statistics
            model.TotalServices = tailor.TailorServices?.Count ?? 0;
            model.ActiveServices = tailor.TailorServices?.Count ?? 0; // All services are considered active
            model.PortfolioImagesCount = tailor.PortfolioImages?.Count(p => !p.IsDeleted) ?? 0;

            // Get recent orders (last 5)
            // ✅ Use split query to avoid cartesian explosion with multiple collections
            var recentOrders = await _context.Orders
                 .Where(o => o.TailorId == tailor.Id)
                .OrderByDescending(o => o.CreatedAt)
        .Take(5)
               .Include(o => o.Customer)
              .ThenInclude(c => c.User)
         .AsSplitQuery()
         .ToListAsync();

            model.RecentOrders = recentOrders.Select(o => new RecentOrderDto
            {
                OrderId = o.OrderId,
                OrderNumber = $"#{o.OrderId.ToString().Substring(0, 8).ToUpper()}",
                CustomerName = o.Customer.User.Email ?? "Customer",
                ServiceName = o.OrderType ?? "Tailoring Service",
                Status = o.Status,
                TotalAmount = (decimal)o.TotalPrice,
                CreatedAt = o.CreatedAt.DateTime,
                DeliveryDate = o.DueDate.HasValue ? o.DueDate.Value.DateTime : (DateTime?)null
            }).ToList();

            // Get reviews and rating breakdown - SIMPLIFIED (no reviews)
            model.TotalReviews = 0;
            model.AverageRating = tailor.AverageRating;
            model.RatingBreakdown = new RatingBreakdown
            {
                FiveStars = 0,
                FourStars = 0,
                ThreeStars = 0,
                TwoStars = 0,
                OneStar = 0
            };

            // Calculate performance metrics
            var lastMonthOrders = orders.Where(o => o.CreatedAt >= DateTimeOffset.UtcNow.AddMonths(-1)).Count();
            var previousMonthOrders = orders.Where(o =>
                    o.CreatedAt >= DateTimeOffset.UtcNow.AddMonths(-2) &&
                     o.CreatedAt < DateTimeOffset.UtcNow.AddMonths(-1)).Count();

            model.Performance = new PerformanceMetrics
            {
                OrderGrowthPercentage = previousMonthOrders > 0
       ? ((lastMonthOrders - previousMonthOrders) * 100.0m / previousMonthOrders)
              : 0,
                RevenueGrowthPercentage = 0, // Can calculate if needed
                AverageOrderValue = completedOrders.Any()
             ? (decimal)completedOrders.Average(o => o.TotalPrice)
                        : 0,
                AverageCompletionTime = 0, // Can calculate if needed
                CustomerSatisfactionRate = tailor.AverageRating * 20, // Simplified
                RepeatCustomersCount = 0 // Can calculate if needed
            };

            // Generate alerts
            model.Alerts = GenerateTailorAlerts(tailor, model);

            ViewData["Title"] = "Tailor Dashboard";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tailor dashboard");
            TempData["Error"] = "An error occurred while loading the dashboard";
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// Generate alerts for tailor dashboard
    /// </summary>
    private List<DashboardAlert> GenerateTailorAlerts(TailorProfile tailor, TailorDashboardViewModel model)
    {
        var alerts = new List<DashboardAlert>();

        // Verification alert
        if (!tailor.IsVerified)
        {
            alerts.Add(new DashboardAlert
            {
                Type = "info",
                Icon = "fa-info-circle",
                Title = "Your Account is Awaiting Verification",
                Message = "Please complete your data and add your products to speed up the verification process. Your account will be reviewed by administration within 24-48 hours.",
                ActionUrl = Url.Action("AddProduct", "TailorManagement"),
                ActionText = "Add Product"
            });
        }

        // No portfolio alert
        if (model.PortfolioImagesCount == 0)
        {
            alerts.Add(new DashboardAlert
            {
                Type = "warning",
                Icon = "fa-images",
                Title = "Portfolio is Empty",
                Message = "Add photos of your previous work to attract more customers and increase your credibility.",
                ActionUrl = Url.Action("AddPortfolioImage", "TailorManagement"),
                ActionText = "Add Images"
            });
        }

        // New orders alert
        if (model.NewOrdersCount > 0)
        {
            alerts.Add(new DashboardAlert
            {
                Type = "success",
                Icon = "fa-bell",
                Title = $"You have {model.NewOrdersCount} new order!",
                Message = "There are new orders that need your review and response.",
                ActionUrl = "#orders-section",
                ActionText = "View Orders"
            });
        }

        return alerts;
    }

    /// <summary>
    /// Generate demo dashboard for admin/tester when no profile exists
    /// </summary>
    private TailorDashboardViewModel GetDemoTailorDashboard()
    {
        return new TailorDashboardViewModel
        {
            TailorId = Guid.NewGuid(),
            FullName = "Tester Tailor",
            ShopName = "Test Tailor Shop",
            IsVerified = true,
            City = "Test City",
            TotalOrdersCount = 0,
            NewOrdersCount = 0,
            ActiveOrdersCount = 0,
            CompletedOrdersCount = 0,
            TotalRevenue = 0,
            MonthlyRevenue = 0,
            WalletBalance = 0,
            PendingPayments = 0,
            TotalServices = 0,
            ActiveServices = 0,
            PortfolioImagesCount = 0,
            RecentOrders = new List<RecentOrderDto>(),
            TotalReviews = 0,
            AverageRating = 0,
            RatingBreakdown = new RatingBreakdown
            {
                FiveStars = 0,
                FourStars = 0,
                ThreeStars = 0,
                TwoStars = 0,
                OneStar = 0
            },
            Performance = new PerformanceMetrics
            {
                OrderGrowthPercentage = 0,
                RevenueGrowthPercentage = 0,
                AverageOrderValue = 0,
                AverageCompletionTime = 0,
                CustomerSatisfactionRate = 0,
                RepeatCustomersCount = 0
            },
            Alerts = new List<DashboardAlert>
          {
         new DashboardAlert
         {
      Type = "info",
    Icon = "fa-flask",
                  Title = "Testing Mode",
   Message = "You are viewing a demo dashboard. Create a tailor profile to see real data.",
       ActionUrl = "/Account/CompleteTailorProfile",
           ActionText = "Create Profile"
  }
  }
        };
    }
    [HttpGet("Tailor/ExportIncomeStatement")]
    [Authorize(Roles = "Tailor")]
    public async Task<IActionResult> ExportIncomeStatement()
    {
        var userId = User.GetUserId();
        var tailor = await _context.TailorProfiles.FirstOrDefaultAsync(t => t.UserId == userId);

        if (tailor == null)
            return NotFound("Tailor profile not found.");

        var orders = await _context.Orders
            .Where(o => o.TailorId == tailor.Id && o.Status == OrderStatus.Delivered)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Customer).ThenInclude(c => c.User)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var builder = new System.Text.StringBuilder();
        // Header
        builder.AppendLine("Date,Order ID,Customer,Product,Quantity,Unit Price (EGP),Total Price (EGP),Platform Fee (10%),Net Income (EGP)");

        foreach (var order in orders)
        {
            foreach (var item in order.Items)
            {
                var lineDate = order.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                var orderId = order.OrderId.ToString().Substring(0, 8);
                var customerName = order.Customer?.FullName?.Replace(",", " ") ?? "N/A";
                var productName = item.Product?.Name?.Replace(",", " ") ?? "Unknown Product";
                var quantity = item.Quantity;
                var unitPrice = item.UnitPrice;
                var itemTotal = unitPrice * quantity;
                var platformFee = itemTotal * (decimal)0.10;
                var netIncome = itemTotal - platformFee;

                builder.AppendLine($"{lineDate},#{orderId},{customerName},{productName},{quantity},{unitPrice:F2},{itemTotal:F2},{platformFee:F2},{netIncome:F2}");
            }
        }

        var contentType = "text/csv";
        var fileName = $"IncomeStatement_Tailor_{DateTime.Now:yyyyMMddHHmm}.csv";

        return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), contentType, fileName);
    }
}
