using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Extensions;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.ViewModels.Dashboard;

namespace TafsilkPlatform.Web.Controllers;

[Authorize]
public class DashboardsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;
    private readonly ILogger<DashboardsController> _logger;

    public DashboardsController(
        IUnitOfWork unitOfWork,
        AppDbContext context,
        ILogger<DashboardsController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    [Authorize(Roles = "Customer")]
    public IActionResult Customer()
    {
        ViewData["Title"] = "لوحة العميل";
        return View();
    }

    [Authorize(Roles = "Corporate")]
    public IActionResult Corporate()
    {
        ViewData["Title"] = "لوحة الشركة";
        return View();
    }

    [Authorize(Roles = "Tailor")]
    public async Task<IActionResult> Tailor()
    {
        try
        {
            var userId = User.GetUserId();

            // Get tailor profile
            var tailor = await _context.TailorProfiles
                .Include(t => t.User)
                .Include(t => t.TailorServices)
                .Include(t => t.PortfolioImages)
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (tailor == null)
            {
                _logger.LogWarning("Tailor profile not found for user {UserId}", userId);
                return RedirectToAction("CompleteTailorProfile", "Account");
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

 // Get wallet balance (if Wallet table exists)
   try
      {
 var wallet = await _context.Set<Wallet>()
    .FirstOrDefaultAsync(w => w.UserId == tailor.UserId);
        model.WalletBalance = wallet?.Balance ?? 0;
      }
      catch
        {
      model.WalletBalance = 0;
        }

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
    var recentOrders = await _context.Orders
         .Where(o => o.TailorId == tailor.Id)
        .OrderByDescending(o => o.CreatedAt)
.Take(5)
       .Include(o => o.Customer)
      .ThenInclude(c => c.User)
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

       // Get reviews and rating breakdown
        var reviews = await _context.Reviews
 .Where(r => r.TailorId == tailor.Id && !r.IsDeleted)
           .ToListAsync();

            model.TotalReviews = reviews.Count;
      
            if (reviews.Any())
   {
           model.AverageRating = (decimal)reviews.Average(r => r.Rating);
   
   model.RatingBreakdown = new RatingBreakdown
{
       FiveStars = reviews.Count(r => r.Rating == 5),
       FourStars = reviews.Count(r => r.Rating == 4),
    ThreeStars = reviews.Count(r => r.Rating == 3),
         TwoStars = reviews.Count(r => r.Rating == 2),
          OneStar = reviews.Count(r => r.Rating == 1)
                };
      }

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
          CustomerSatisfactionRate = reviews.Any() 
            ? (decimal)(reviews.Average(r => r.Rating) * 20) 
           : 0,
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
}
