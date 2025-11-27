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
                TempData["ErrorMessage"] = "يجب تقديم الأوراق الثبوتية وإكمال ملفك الشخصي أولاً. هذه الخطوة إلزامية للخياطين.";
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
                ViewData["PendingMessage"] = "حسابك قيد المراجعة من قبل الإدارة. سيتم تفعيل جميع الميزات بعد الموافقة (عادة خلال 2-3 أيام عمل).";
            }

            // Build dashboard data
            var model = new TailorDashboardViewModel
            {
                TailorId = tailor.Id,
                FullName = tailor.FullName ?? "خياط",
                ShopName = tailor.ShopName ?? "ورشة غير مسماة",
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

            // Get financial statistics
            var completedOrders = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();
            model.TotalRevenue = (decimal)completedOrders.Sum(o => o.TotalPrice);

            var monthlyOrders = completedOrders
          .Where(o => o.CreatedAt >= DateTimeOffset.UtcNow.AddMonths(-1))
                .ToList();
            model.MonthlyRevenue = (decimal)monthlyOrders.Sum(o => o.TotalPrice);

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
                CustomerName = o.Customer.User.Email ?? "عميل",
                ServiceName = o.OrderType ?? "خدمة تفصيل",
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

            ViewData["Title"] = "لوحة الخياط";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tailor dashboard");
            TempData["Error"] = "حدث خطأ أثناء تحميل لوحة التحكم";
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
                Title = "حسابك بانتظار التحقق",
                Message = "يرجى إكمال بياناتك وإضافة خدماتك لتسريع عملية التحقق. سيتم مراجعة حسابك من قبل الإدارة خلال 24-48 ساعة.",
                ActionUrl = Url.Action("ManageServices", "TailorManagement"),
                ActionText = "إضافة خدمات"
            });
        }

        // No services alert
        if (model.TotalServices == 0)
        {
            alerts.Add(new DashboardAlert
            {
                Type = "warning",
                Icon = "fa-exclamation-triangle",
                Title = "لا توجد خدمات",
                Message = "قم بإضافة خدماتك ليتمكن العملاء من العثور عليك وطلب خدماتك.",
                ActionUrl = Url.Action("AddService", "TailorManagement"),
                ActionText = "إضافة خدمة"
            });
        }

        // No portfolio alert
        if (model.PortfolioImagesCount == 0)
        {
            alerts.Add(new DashboardAlert
            {
                Type = "warning",
                Icon = "fa-images",
                Title = "معرض الأعمال فارغ",
                Message = "أضف صور من أعمالك السابقة لجذب المزيد من العملاء وزيادة مصداقيتك.",
                ActionUrl = Url.Action("AddPortfolioImage", "TailorManagement"),
                ActionText = "إضافة صور"
            });
        }

        // New orders alert
        if (model.NewOrdersCount > 0)
        {
            alerts.Add(new DashboardAlert
            {
                Type = "success",
                Icon = "fa-bell",
                Title = $"لديك {model.NewOrdersCount} طلب جديد!",
                Message = "يوجد طلبات جديدة تحتاج إلى مراجعتك والرد عليها.",
                ActionUrl = "#orders-section",
                ActionText = "عرض الطلبات"
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
}
