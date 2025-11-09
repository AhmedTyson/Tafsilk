using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Controllers.Base;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.ViewModels.Admin;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// Admin Dashboard Controller - Simplified version for basic admin functionality
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminDashboardController : BaseController
{
    private readonly AppDbContext _db;

    public AdminDashboardController(
        AppDbContext db,
      ILogger<AdminDashboardController> logger) : base(logger)
    {
        _db = db;
    }

    /// <summary>
    /// Admin Dashboard Home
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            // Get basic counts
            var totalUsers = await _db.Users.CountAsync(u => !u.IsDeleted);
            var totalCustomers = await _db.CustomerProfiles.CountAsync();
            var totalTailors = await _db.TailorProfiles.CountAsync();
            var activeOrders = await _db.Orders.CountAsync(o =>
                o.Status != OrderStatus.Delivered &&
                o.Status != OrderStatus.Cancelled);
            var totalRevenue = await _db.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            var viewModel = new DashboardHomeViewModel
            {
                TotalUsers = totalUsers,
                TotalCustomers = totalCustomers,
                TotalTailors = totalTailors,
                PendingTailorVerifications = 0, // Simplified - no verification
                PendingPortfolioReviews = 0,
                ActiveOrders = activeOrders,
                TotalRevenue = totalRevenue,
                RecentActivity = new List<ActivityLogDto>()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading admin dashboard");
            TempData["Error"] = "حدث خطأ أثناء تحميل لوحة التحكم";
            return View(new DashboardHomeViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        try
   {
     var users = await _db.Users
        .Include(u => u.Role)
          .Include(u => u.CustomerProfile)
    .Include(u => u.TailorProfile)
     .Where(u => !u.IsDeleted)
        .OrderByDescending(u => u.CreatedAt)
       .ToListAsync();

     return View(users);
 }
 catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users page");
   TempData["Error"] = "حدث خطأ أثناء تحميل صفحة المستخدمين";
 return View(new List<User>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> UserDetails(Guid id)
    {
  try
        {
            var user = await _db.Users
              .Include(u => u.Role)
                .Include(u => u.CustomerProfile)
         .Include(u => u.TailorProfile)
                .FirstOrDefaultAsync(u => u.Id == id);

      if (user == null)
            {
                TempData["Error"] = "المستخدم غير موجود";
                return RedirectToAction(nameof(Users));
}

      return View(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user details for {UserId}", id);
    TempData["Error"] = "حدث خطأ أثناء تحميل تفاصيل المستخدم";
            return RedirectToAction(nameof(Users));
   }
    }

    [HttpGet]
    public IActionResult TailorVerification()
    {
        TempData["Info"] = "تم تبسيط النظام - التحقق من الخياطين غير مطلوب";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public IActionResult ReviewTailor(Guid id)
    {
TempData["Info"] = "تم تبسيط النظام - التحقق من الخياطين غير مطلوب";
    return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ApproveTailor(Guid id, string? notes)
    {
        TempData["Info"] = "تم تبسيط النظام - التحقق من الخياطين غير مطلوب";
    return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RejectTailor(Guid id, string? reason)
    {
        TempData["Info"] = "تم تبسيط النظام - التحقق من الخياطين غير مطلوب";
    return RedirectToAction(nameof(Users));
    }

    [HttpGet]
  public async Task<IActionResult> PortfolioReview()
  {
   try
   {
    var images = await _db.PortfolioImages
     .Include(p => p.Tailor)
     .ThenInclude(t => t.User)
                .Where(p => !p.IsDeleted)
          .OrderByDescending(p => p.UploadedAt)
         .ToListAsync();

            return View(images);
   }
     catch (Exception ex)
        {
    _logger.LogError(ex, "Error loading portfolio review page");
            TempData["Error"] = "حدث خطأ أثناء تحميل صفحة مراجعة الصور";
          return View(new List<PortfolioImage>());
        }
    }

 [HttpGet]
    public async Task<IActionResult> Orders()
    {
 try
     {
            var orders = await _db.Orders
 .Include(o => o.Customer)
.ThenInclude(c => c.User)
                .Include(o => o.Tailor)
         .ThenInclude(t => t.User)
            .OrderByDescending(o => o.CreatedAt)
 .ToListAsync();

            return View(orders);
    }
        catch (Exception ex)
{
            _logger.LogError(ex, "Error loading orders page");
            TempData["Error"] = "حدث خطأ أثناء تحميل صفحة الطلبات";
 return View(new List<Order>());
    }
    }

    [HttpGet]
    public IActionResult Disputes()
  {
   TempData["Info"] = "ميزة النزاعات غير متاحة حالياً";
  return View();
    }

    [HttpGet]
    public IActionResult Refunds()
    {
   TempData["Info"] = "ميزة طلبات الاسترداد غير متاحة حالياً";
        return View();
    }

    [HttpGet]
    public IActionResult Reviews()
    {
        TempData["Info"] = "تم تبسيط النظام - التقييمات غير متاحة";
    return RedirectToAction(nameof(Orders));
    }

    [HttpGet]
    public IActionResult Analytics()
  {
        return View();
    }

    [HttpGet]
    public IActionResult Notifications()
    {
    TempData["Info"] = "تم تبسيط النظام - الإشعارات غير متاحة";
    return View(new List<string>());
    }

    [HttpGet]
    public IActionResult AuditLogs()
    {
        TempData["Info"] = "ميزة سجلات التدقيق قيد التطوير";
        return View();
    }

    /// <summary>
    /// View verification document (ID, commercial registration, etc.)
    /// </summary>
    [HttpGet]
    public IActionResult ViewVerificationDocument(Guid tailorId, string documentType)
    {
        TempData["Info"] = "تم تبسيط النظام - وثائق التحقق غير متاحة";
    return RedirectToAction(nameof(Users));
}
    }
