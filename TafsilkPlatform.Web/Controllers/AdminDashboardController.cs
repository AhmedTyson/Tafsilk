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
          var pendingVerifications = await _db.TailorProfiles.CountAsync(t => !t.IsVerified);
var pendingPortfolioReviews = await _db.PortfolioImages.CountAsync(p => !p.IsDeleted);
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
       PendingTailorVerifications = pendingVerifications,
      PendingPortfolioReviews = pendingPortfolioReviews,
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
    public async Task<IActionResult> TailorVerification()
    {
        try
      {
 var pendingTailors = await _db.TailorProfiles
                .Include(t => t.User)
           .Where(t => !t.IsVerified)
   .OrderByDescending(t => t.CreatedAt)
    .ToListAsync();

       return View(pendingTailors);
   }
        catch (Exception ex)
        {
  _logger.LogError(ex, "Error loading tailor verification page");
  TempData["Error"] = "حدث خطأ أثناء تحميل صفحة التحقق من الخياطين";
            return View(new List<TailorProfile>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> ReviewTailor(Guid id)
    {
        try
        {
          var tailor = await _db.TailorProfiles
   .Include(t => t.User)
       .Include(t => t.PortfolioImages)
  .Include(t => t.TailorServices)
      .Include(t => t.Verification)
 .FirstOrDefaultAsync(t => t.Id == id);

      if (tailor == null)
          {
      TempData["Error"] = "الخياط غير موجود";
    return RedirectToAction(nameof(TailorVerification));
     }

            return View(tailor);
      }
        catch (Exception ex)
    {
          _logger.LogError(ex, "Error loading tailor review for {TailorId}", id);
            TempData["Error"] = "حدث خطأ أثناء تحميل تفاصيل الخياط";
            return RedirectToAction(nameof(TailorVerification));
 }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveTailor(Guid id, string? notes)
    {
        try
        {
            var tailor = await _db.TailorProfiles
      .Include(t => t.Verification)
        .FirstOrDefaultAsync(t => t.Id == id);

if (tailor == null)
   {
             TempData["Error"] = "الخياط غير موجود";
     return RedirectToAction(nameof(TailorVerification));
 }

            var adminUserId = GetUserId();

         tailor.IsVerified = true;
  tailor.VerifiedAt = DateTime.UtcNow;
     tailor.UpdatedAt = DateTime.UtcNow;

     if (tailor.Verification != null)
    {
        tailor.Verification.Status = VerificationStatus.Approved;
  tailor.Verification.ReviewedAt = DateTime.UtcNow;
         tailor.Verification.ReviewedByAdminId = adminUserId;
       tailor.Verification.ReviewNotes = notes;
            }

  if (string.IsNullOrEmpty(tailor.ShopDescription))
            {
      tailor.ShopDescription = tailor.Bio ?? "ورشة خياطة محترفة";
            }

  await _db.SaveChangesAsync();

   _logger.LogInformation("[AdminDashboard] Admin {AdminId} approved tailor {TailorId}. Profile is now public.", adminUserId, id);

            TempData["Success"] = "تم الموافقة على تحقق الخياط بنجاح! الملف الشخصي أصبح عاماً الآن.";
 return RedirectToAction(nameof(TailorVerification));
     }
        catch (Exception ex)
        {
     _logger.LogError(ex, "Error approving tailor {TailorId}", id);
    TempData["Error"] = "حدث خطأ أثناء الموافقة على التحقق";
  return RedirectToAction(nameof(TailorVerification));
      }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectTailor(Guid id, string? reason)
    {
        try
        {
     var tailor = await _db.TailorProfiles
    .Include(t => t.Verification)
                .FirstOrDefaultAsync(t => t.Id == id);

     if (tailor == null)
            {
          TempData["Error"] = "الخياط غير موجود";
    return RedirectToAction(nameof(TailorVerification));
     }

  var adminUserId = GetUserId();

            if (tailor.Verification != null)
       {
     tailor.Verification.Status = VerificationStatus.Rejected;
      tailor.Verification.ReviewedAt = DateTime.UtcNow;
    tailor.Verification.ReviewedByAdminId = adminUserId;
          tailor.Verification.RejectionReason = reason ?? "لم يتم توفير السبب";
  tailor.Verification.ReviewNotes = reason;
          }

         tailor.IsVerified = false;
            tailor.UpdatedAt = DateTime.UtcNow;

     await _db.SaveChangesAsync();

      _logger.LogInformation("[AdminDashboard] Admin {AdminId} rejected tailor {TailorId}. Reason: {Reason}", adminUserId, id, reason);

 TempData["Success"] = "تم رفض طلب التحقق. سيتم إشعار الخياط بالسبب.";
 return RedirectToAction(nameof(TailorVerification));
     }
        catch (Exception ex)
        {
    _logger.LogError(ex, "Error rejecting tailor {TailorId}", id);
            TempData["Error"] = "حدث خطأ أثناء رفض الطلب";
     return RedirectToAction(nameof(TailorVerification));
        }
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
    public async Task<IActionResult> Reviews()
    {
        try
        {
            var reviews = await _db.Reviews
      .Include(r => r.Tailor)
                .ThenInclude(t => t.User)
         .Include(r => r.Customer)
   .ThenInclude(c => c.User)
  .Where(r => !r.IsDeleted)
       .OrderByDescending(r => r.CreatedAt)
     .ToListAsync();

    return View(reviews);
      }
     catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reviews page");
  TempData["Error"] = "حدث خطأ أثناء تحميل صفحة التقييمات";
   return View(new List<Review>());
     }
    }

    [HttpGet]
    public IActionResult Analytics()
  {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Notifications()
    {
    try
      {
         var adminId = GetUserId();
            var notifications = await _db.Notifications
      .Where(n => n.UserId == adminId)
              .OrderByDescending(n => n.SentAt)
        .ToListAsync();

   return View(notifications);
        }
    catch (Exception ex)
        {
    _logger.LogError(ex, "Error loading notifications");
     TempData["Error"] = "حدث خطأ أثناء تحميل الإشعارات";
return View(new List<Notification>());
        }
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
    public async Task<IActionResult> ViewVerificationDocument(Guid tailorId, string documentType)
    {
        try
        {
     var verification = await _db.TailorVerifications
       .FirstOrDefaultAsync(v => v.TailorProfileId == tailorId);

   if (verification == null)
            {
     return NotFound("وثائق التحقق غير موجودة");
            }

         byte[]? documentData = null;
            string? contentType = null;

      switch (documentType.ToLower())
            {
        case "idfront":
      documentData = verification.IdDocumentFrontData;
                contentType = verification.IdDocumentFrontContentType;
           break;
      case "idback":
   documentData = verification.IdDocumentBackData;
        contentType = verification.IdDocumentBackContentType;
        break;
          case "commercial":
     documentData = verification.CommercialRegistrationData;
           contentType = verification.CommercialRegistrationContentType;
 break;
                case "license":
      documentData = verification.ProfessionalLicenseData;
      contentType = verification.ProfessionalLicenseContentType;
 break;
 default:
         return BadRequest("نوع الوثيقة غير صالح");
       }

    if (documentData == null || documentData.Length == 0)
   {
      return NotFound("الوثيقة غير متوفرة");
            }

            return File(documentData, contentType ?? "application/octet-stream");
        }
   catch (Exception ex)
        {
            _logger.LogError(ex, "Error viewing verification document for tailor {TailorId}, type {DocumentType}", tailorId, documentType);
      return StatusCode(500, "حدث خطأ أثناء تحميل الوثيقة");
        }
    }
}
