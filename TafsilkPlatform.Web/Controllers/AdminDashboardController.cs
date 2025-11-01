using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.ViewModels.Admin;

namespace TafsilkPlatform.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("Admin")]
public class AdminDashboardController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(AppDbContext context, ILogger<AdminDashboardController> logger)
  {
    _context = context;
      _logger = logger;
    }

    // ============================================
    // 1. DASHBOARD HOME
    // ============================================
    
    [HttpGet("")]
    [HttpGet("Dashboard")]
    public async Task<IActionResult> Index()
    {
        var viewModel = new DashboardHomeViewModel
        {
    TotalUsers = await _context.Users.CountAsync(),
     TotalCustomers = await _context.CustomerProfiles.CountAsync(),
 TotalTailors = await _context.TailorProfiles.CountAsync(),
            TotalCorporate = await _context.CorporateAccounts.CountAsync(),
          
    PendingTailorVerifications = await _context.TailorProfiles
          .Where(t => !t.IsVerified)
     .CountAsync(),
            
 // Portfolio reviews - count all images (no Status field yet)
       PendingPortfolioReviews = await _context.PortfolioImages
                .Where(p => !p.IsDeleted)
  .CountAsync(),
      
        // Use enum values properly
          ActiveOrders = await _context.Orders
          .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing)
         .CountAsync(),
      
            // Disputes using string Status
    OpenDisputes = await _context.Disputes
            .Where(d => d.Status == "Open" || d.Status == "UnderReview")
    .CountAsync(),
     
         // Refund requests
       PendingRefunds = await _context.RefundRequests
                .Where(r => r.Status == "Pending")
   .CountAsync(),
      
   // Total revenue from completed payments
      TotalRevenue = await _context.Payment
        .Where(p => p.PaymentStatus == Enums.PaymentStatus.Completed)
     .SumAsync(p => (decimal?)p.Amount) ?? 0,
     
        RecentActivity = await _context.UserActivityLogs
  .OrderByDescending(l => l.CreatedAt)
    .Take(10)
        .Include(l => l.User)
            .Select(l => new ActivityLogDto
         {
                Action = l.Action,
             UserName = l.User != null ? l.User.Email : "Unknown",
      Timestamp = l.CreatedAt,
   IpAddress = l.IpAddress
     })
.ToListAsync()
        };

        return View(viewModel);
    }

    // ============================================
    // 2. USER MANAGEMENT
    // ============================================

    [HttpGet("Users")]
    public async Task<IActionResult> Users([FromQuery] string? search, [FromQuery] string? role, [FromQuery] string? status, [FromQuery] int page = 1)
    {
        var query = _context.Users
  .Include(u => u.Role)
 .Include(u => u.CustomerProfile)
 .Include(u => u.TailorProfile)
     .Include(u => u.CorporateAccount)
       .AsQueryable();

 if (!string.IsNullOrWhiteSpace(search))
      {
 query = query.Where(u => u.Email.Contains(search) || (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
        }

if (!string.IsNullOrWhiteSpace(role))
        {
        query = query.Where(u => u.Role.Name == role);
     }

      if (!string.IsNullOrWhiteSpace(status))
   {
      if (status == "Active")
  query = query.Where(u => u.IsActive && !u.IsDeleted);
  else if (status == "Inactive")
      query = query.Where(u => !u.IsActive);
   else if (status == "Banned")
       query = query.Where(u => !u.IsActive);
        }

        var pageSize = 20;
   var users = await query
  .OrderByDescending(u => u.CreatedAt)
.Skip((page - 1) * pageSize)
 .Take(pageSize)
 .ToListAsync();

        var totalUsers = await query.CountAsync();

        // Calculate statistics
        var totalUsersCount = await _context.Users.CountAsync();
  var onlineUsers = await _context.Users
   .Where(u => u.LastLoginAt.HasValue && 
        u.LastLoginAt.Value >= DateTime.UtcNow.AddMinutes(-30))
            .CountAsync();
        var activeUsers = await _context.Users
          .Where(u => u.IsActive && !u.IsDeleted)
            .CountAsync();
   var bannedUsers = await _context.Users
            .Where(u => !u.IsActive)
         .CountAsync();

        ViewBag.TotalUsers = totalUsersCount;
        ViewBag.OnlineUsers = onlineUsers;
        ViewBag.ActiveUsers = activeUsers;
        ViewBag.BannedUsers = bannedUsers;
        ViewBag.SearchTerm = search;
        ViewBag.RoleFilter = role;
        ViewBag.StatusFilter = status;
     ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

        return View(users);
  }

    [HttpPost("Users/Ban/{userId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BanUser(Guid userId)
  {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound();

// Prevent banning admin users
        var userRole = await _context.Roles.FindAsync(user.RoleId);
        if (userRole?.Name == "Admin")
        {
            TempData["ErrorMessage"] = "لا يمكن حظر حسابات المديرين";
            return RedirectToAction(nameof(Users));
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await LogAdminAction("User Banned", $"User {user.Email} has been banned", "User");
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "تم حظر المستخدم بنجاح";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost("Users/Unban/{userId}")]
    [ValidateAntiForgeryToken]
 public async Task<IActionResult> UnbanUser(Guid userId)
  {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
      return NotFound();

        user.IsActive = true;
      user.UpdatedAt = DateTime.UtcNow;

await LogAdminAction("User Unbanned", $"User {user.Email} has been unbanned", "User");
      await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "تم إلغاء حظر المستخدم بنجاح";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet("GetOnlineCount")]
    public async Task<IActionResult> GetOnlineCount()
    {
   var count = await _context.Users
 .Where(u => u.LastLoginAt.HasValue && 
          u.LastLoginAt.Value >= DateTime.UtcNow.AddMinutes(-30))
    .CountAsync();

        return Json(new { count });
    }

    [HttpGet("Users/Details/{userId}")]
    public async Task<IActionResult> UserDetails(Guid userId)
    {
  var user = await _context.Users
            .Include(u => u.Role)
 .Include(u => u.CustomerProfile)
       .Include(u => u.TailorProfile)
    .ThenInclude(t => t!.TailorServices)
            .Include(u => u.TailorProfile)
    .ThenInclude(t => t!.PortfolioImages)
    .Include(u => u.CorporateAccount)
        .Include(u => u.UserAddresses)
       .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound();

        // Get user activity logs
        var activityLogs = await _context.UserActivityLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
 .Take(50)
   .ToListAsync();

      ViewBag.ActivityLogs = activityLogs;

        // Get orders if customer or tailor
if (user.Role?.Name == "Customer" && user.CustomerProfile != null)
        {
            var orders = await _context.Orders
  .Where(o => o.CustomerId == user.CustomerProfile.Id)
  .Include(o => o.Tailor)
         .OrderByDescending(o => o.CreatedAt)
     .Take(10)
       .ToListAsync();
            ViewBag.Orders = orders;
        }
        else if (user.Role?.Name == "Tailor" && user.TailorProfile != null)
        {
            var orders = await _context.Orders
         .Where(o => o.TailorId == user.TailorProfile.Id)
        .Include(o => o.Customer)
      .OrderByDescending(o => o.CreatedAt)
   .Take(10)
         .ToListAsync();
            ViewBag.Orders = orders;
        }

        return View(user);
 }

    [HttpPost("Users/{id}/Suspend")]
    public async Task<IActionResult> SuspendUser(Guid id, [FromForm] string reason)
    {
        var user = await _context.Users.FindAsync(id);
    if (user == null)
      return NotFound();

 user.IsActive = false;
    user.UpdatedAt = DateTime.UtcNow;

        // Log the action
        await LogAdminAction("User Suspended", $"User {user.Email} suspended. Reason: {reason}", "User");

        await _context.SaveChangesAsync();

 TempData["Success"] = "User suspended successfully";
        return RedirectToAction(nameof(Users));
    }

  [HttpPost("Users/{id}/Activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
     var user = await _context.Users.FindAsync(id);
  if (user == null)
   return NotFound();

   user.IsActive = true;
      user.UpdatedAt = DateTime.UtcNow;

    await LogAdminAction("User Activated", $"User {user.Email} activated", "User");
        await _context.SaveChangesAsync();

    TempData["Success"] = "User activated successfully";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost("Users/{id}/Delete")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
  var user = await _context.Users.FindAsync(id);
 if (user == null)
    return NotFound();

     user.IsDeleted = true;
    user.UpdatedAt = DateTime.UtcNow;

      await LogAdminAction("User Deleted", $"User {user.Email} marked as deleted", "User");
        await _context.SaveChangesAsync();

   TempData["Success"] = "User deleted successfully";
 return RedirectToAction(nameof(Users));
    }

    // ============================================
    // 3. TAILOR VERIFICATION
    // ============================================

[HttpGet("Tailors/Verification")]
    public async Task<IActionResult> TailorVerification([FromQuery] string? status, [FromQuery] int page = 1)
    {
        var query = _context.TailorProfiles
            .Include(t => t.User)
            .Include(t => t.PortfolioImages)
        .AsQueryable();

 if (!string.IsNullOrWhiteSpace(status))
        {
 if (status == "Pending")
          query = query.Where(t => !t.IsVerified);
            else if (status == "Verified")
                query = query.Where(t => t.IsVerified);
        }
        else
        {
            query = query.Where(t => !t.IsVerified); // Default: pending only
        }

        var pageSize = 10;
      var tailors = await query
          .OrderByDescending(t => t.CreatedAt)
     .Skip((page - 1) * pageSize)
          .Take(pageSize)
        .ToListAsync();

        var totalTailors = await query.CountAsync();

     var viewModel = new TailorVerificationViewModel
        {
            Tailors = tailors,
 CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalTailors / (double)pageSize),
    SelectedStatus = status
        };

        return View(viewModel);
    }

    [HttpGet("Tailors/{id}/Review")]
    public async Task<IActionResult> ReviewTailor(Guid id)
    {
   var tailor = await _context.TailorProfiles
            .Include(t => t.User)
         .Include(t => t.PortfolioImages)
            .Include(t => t.TailorServices)
    .FirstOrDefaultAsync(t => t.Id == id);

        if (tailor == null)
 return NotFound();

        return View(tailor);
    }

    [HttpPost("Tailors/{id}/Approve")]
    public async Task<IActionResult> ApproveTailor(Guid id, [FromForm] string? notes)
 {
    var tailor = await _context.TailorProfiles
  .Include(t => t.User)
          .FirstOrDefaultAsync(t => t.Id == id);

        if (tailor == null)
        return NotFound();

tailor.IsVerified = true;
   tailor.UpdatedAt = DateTime.UtcNow;

      // Create notification for tailor
 var notification = new Notification
        {
 UserId = tailor.UserId,
          Title = "تم التحقق من حسابك",
     Message = "تهانينا! تم التحقق من حسابك بنجاح. يمكنك الآن استقبال الطلبات.",
        Type = "Success",
   SentAt = DateTime.UtcNow
     };
   _context.Notifications.Add(notification);

        await LogAdminAction("Tailor Approved", $"Tailor {tailor.FullName} ({tailor.User?.Email ?? "unknown"}) approved. Notes: {notes}", "TailorProfile");
        await _context.SaveChangesAsync();

     TempData["Success"] = "Tailor verified successfully";
        return RedirectToAction(nameof(TailorVerification));
    }

    [HttpPost("Tailors/{id}/Reject")]
    public async Task<IActionResult> RejectTailor(Guid id, [FromForm] string reason)
    {
        var tailor = await _context.TailorProfiles
            .Include(t => t.User)
.FirstOrDefaultAsync(t => t.Id == id);

        if (tailor == null)
       return NotFound();

        // Create notification for tailor
        var notification = new Notification
        {
         UserId = tailor.UserId,
      Title = "تم رفض طلب التحقق",
      Message = $"عذراً، تم رفض طلب التحقق. السبب: {reason}",
         Type = "Warning",
            SentAt = DateTime.UtcNow
        };
      _context.Notifications.Add(notification);

        await LogAdminAction("Tailor Rejected", $"Tailor {tailor.FullName} ({tailor.User?.Email ?? "unknown"}) rejected. Reason: {reason}", "TailorProfile");
        await _context.SaveChangesAsync();

        TempData["Info"] = "Tailor verification rejected";
      return RedirectToAction(nameof(TailorVerification));
   }

// ============================================
    // 4. PORTFOLIO REVIEW
    // ============================================

    [HttpGet("Portfolio")]
    public async Task<IActionResult> PortfolioReview([FromQuery] string? status, [FromQuery] int page = 1)
    {
 var query = _context.PortfolioImages
.Include(p => p.Tailor)
       .ThenInclude(t => t!.User)
   .Where(p => !p.IsDeleted)
            .AsQueryable();

   var pageSize = 12;
        var images = await query
    .OrderByDescending(p => p.UploadedAt)
       .Skip((page - 1) * pageSize)
 .Take(pageSize)
         .ToListAsync();

        var totalImages = await query.CountAsync();

        var viewModel = new PortfolioReviewViewModel
        {
     Images = images,
    CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalImages / (double)pageSize),
            SelectedStatus = status
        };

        return View(viewModel);
    }

    [HttpPost("Portfolio/{id}/Approve")]
    public async Task<IActionResult> ApprovePortfolioImage(Guid id)
    {
 var image = await _context.PortfolioImages
      .Include(p => p.Tailor)
      .FirstOrDefaultAsync(p => p.PortfolioImageId == id);

        if (image == null)
   return NotFound();

 await LogAdminAction("Portfolio Approved", $"Portfolio image {id} for tailor {image.Tailor?.FullName ?? "unknown"} approved", "PortfolioImage");
     await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Image approved successfully" });
    }

  [HttpPost("Portfolio/{id}/Reject")]
    public async Task<IActionResult> RejectPortfolioImage(Guid id, [FromForm] string reason)
    {
     var image = await _context.PortfolioImages
.Include(p => p.Tailor)
    .ThenInclude(t => t!.User)
.FirstOrDefaultAsync(p => p.PortfolioImageId == id);

        if (image == null)
        return NotFound();

        // Notify tailor
  if (image.Tailor != null)
    {
        var notification = new Notification
        {
  UserId = image.Tailor.UserId,
     Title = "تم رفض صورة من معرض أعمالك",
 Message = $"تم رفض إحدى الصور. السبب: {reason}",
    Type = "Warning",
 SentAt = DateTime.UtcNow
       };
     _context.Notifications.Add(notification);
  }

        await LogAdminAction("Portfolio Rejected", $"Portfolio image {id} rejected. Reason: {reason}", "PortfolioImage");
 await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Image rejected successfully" });
    }

    // ============================================
    // 5. ORDER MANAGEMENT
    // ============================================

    [HttpGet("Orders")]
  public async Task<IActionResult> Orders([FromQuery] string? status, [FromQuery] int page = 1)
    {
        var query = _context.Orders
.Include(o => o.Customer)
  .ThenInclude(c => c.User)
            .Include(o => o.Tailor)
    .ThenInclude(t => t.User)
       .Include(o => o.Items)
      .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
        {
       query = query.Where(o => o.Status == orderStatus);
        }

 var pageSize = 20;
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
        .Skip((page - 1) * pageSize)
  .Take(pageSize)
            .ToListAsync();

  var totalOrders = await query.CountAsync();

        var viewModel = new OrderManagementViewModel
        {
      Orders = orders,
       CurrentPage = page,
        TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize),
    SelectedStatus = status
    };

        return View(viewModel);
    }

    [HttpGet("Orders/{id}")]
    public async Task<IActionResult> OrderDetails(Guid id)
    {
        var order = await _context.Orders
     .Include(o => o.Customer)
                .ThenInclude(c => c.User)
    .Include(o => o.Tailor)
        .ThenInclude(t => t.User)
     .Include(o => o.Items)
            .Include(o => o.orderImages)
         .Include(o => o.Payments)
       .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null)
            return NotFound();

        return View(order);
    }

    // ============================================
    // 6. DISPUTE RESOLUTION
    // ============================================

 [HttpGet("Disputes")]
    public async Task<IActionResult> Disputes([FromQuery] string? status, [FromQuery] int page = 1)
    {
   var query = _context.Disputes
      .Include(d => d.Order)
                .ThenInclude(o => o.Customer)
     .ThenInclude(c => c.User)
          .Include(d => d.Order)
    .ThenInclude(o => o.Tailor)
       .ThenInclude(t => t.User)
          .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
{
   query = query.Where(d => d.Status == status);
        }
     else
   {
            query = query.Where(d => d.Status != "Resolved" && d.Status != "Closed");
        }

      var pageSize = 10;
    var disputes = await query
            .OrderByDescending(d => d.CreatedAt)
      .Skip((page - 1) * pageSize)
         .Take(pageSize)
       .ToListAsync();

        var totalDisputes = await query.CountAsync();

  var viewModel = new DisputeManagementViewModel
    {
      Disputes = disputes,
      CurrentPage = page,
TotalPages = (int)Math.Ceiling(totalDisputes / (double)pageSize),
            SelectedStatus = status
    };

        return View(viewModel);
    }

    [HttpGet("Disputes/{id}")]
    public async Task<IActionResult> DisputeDetails(Guid id)
    {
      var dispute = await _context.Disputes
      .Include(d => d.Order)
             .ThenInclude(o => o.Customer)
        .ThenInclude(c => c.User)
            .Include(d => d.Order)
             .ThenInclude(o => o.Tailor)
         .ThenInclude(t => t.User)
          .Include(d => d.Order)
         .ThenInclude(o => o.orderImages)
       .FirstOrDefaultAsync(d => d.Id == id);

        if (dispute == null)
            return NotFound();

    return View(dispute);
    }

 [HttpPost("Disputes/{id}/Resolve")]
    public async Task<IActionResult> ResolveDispute(Guid id, [FromForm] string resolution, [FromForm] string favoredParty)
    {
     var dispute = await _context.Disputes
  .Include(d => d.Order)
   .FirstOrDefaultAsync(d => d.Id == id);

        if (dispute == null)
   return NotFound();

        dispute.Status = "Resolved";
  dispute.ResolutionDetails = resolution;
   dispute.ResolvedAt = DateTime.UtcNow;

        await LogAdminAction("Dispute Resolved", $"Dispute {id} resolved in favor of {favoredParty}. Resolution: {resolution}", "Dispute");
    await _context.SaveChangesAsync();

     TempData["Success"] = "Dispute resolved successfully";
  return RedirectToAction(nameof(Disputes));
    }

    // ============================================
// 7. REFUND MANAGEMENT
    // ============================================

    [HttpGet("Refunds")]
    public async Task<IActionResult> Refunds([FromQuery] string? status, [FromQuery] int page = 1)
    {
        var query = _context.RefundRequests
         .Include(r => r.Order)
                .ThenInclude(o => o.Customer)
     .ThenInclude(c => c.User)
  .Include(r => r.Order)
  .ThenInclude(o => o.Tailor)
       .ThenInclude(t => t.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
  {
            query = query.Where(r => r.Status == status);
        }
 else
        {
            query = query.Where(r => r.Status == "Pending");
        }

        var pageSize = 10;
        var refunds = await query
     .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
  .Take(pageSize)
            .ToListAsync();

   var totalRefunds = await query.CountAsync();

        var viewModel = new RefundManagementViewModel
   {
   Refunds = refunds,
 CurrentPage = page,
         TotalPages = (int)Math.Ceiling(totalRefunds / (double)pageSize),
        SelectedStatus = status
        };

        return View(viewModel);
    }

    [HttpPost("Refunds/{id}/Approve")]
    public async Task<IActionResult> ApproveRefund(Guid id, [FromForm] string? notes)
    {
     var refund = await _context.RefundRequests
       .Include(r => r.Order)
   .ThenInclude(o => o.Customer)
    .FirstOrDefaultAsync(r => r.Id == id);

        if (refund == null)
    return NotFound();

refund.Status = "Approved";
 refund.ProcessedAt = DateTime.UtcNow;

        // Create notification
 var notification = new Notification
  {
 UserId = refund.Order.CustomerId,
    Title = "تمت الموافقة على طلب الاسترداد",
Message = $"تمت الموافقة على طلب استرداد {refund.Amount:C}. سيتم تحويل المبلغ خلال 3-5 أيام عمل.",
   Type = "Success",
  SentAt = DateTime.UtcNow
  };
        _context.Notifications.Add(notification);

     await LogAdminAction("Refund Approved", $"Refund {id} for amount {refund.Amount:C} approved", "RefundRequest");
      await _context.SaveChangesAsync();

    TempData["Success"] = "Refund approved successfully";
        return RedirectToAction(nameof(Refunds));
    }

  [HttpPost("Refunds/{id}/Reject")]
    public async Task<IActionResult> RejectRefund(Guid id, [FromForm] string reason)
    {
  var refund = await _context.RefundRequests
    .Include(r => r.Order)
.ThenInclude(o => o.Customer)
     .FirstOrDefaultAsync(r => r.Id == id);

   if (refund == null)
     return NotFound();

  refund.Status = "Rejected";
        refund.ProcessedAt = DateTime.UtcNow;

  // Create notification
    var notification = new Notification
  {
      UserId = refund.Order.CustomerId,
   Title = "تم رفض طلب الاسترداد",
  Message = $"عذراً، تم رفض طلب الاسترداد. السبب: {reason}",
Type = "Warning",
    SentAt = DateTime.UtcNow
 };
        _context.Notifications.Add(notification);

      await LogAdminAction("Refund Rejected", $"Refund {id} rejected. Reason: {reason}", "RefundRequest");
    await _context.SaveChangesAsync();

        TempData["Info"] = "Refund rejected";
      return RedirectToAction(nameof(Refunds));
    }

    // ============================================
    // 8. REVIEW MODERATION
    // ============================================

 [HttpGet("Reviews")]
    public async Task<IActionResult> Reviews([FromQuery] string? filter, [FromQuery] int page = 1)
    {
        var query = _context.Reviews
    .Include(r => r.RatingDimensions)
     .AsQueryable();

    if (filter == "Flagged")
        {
       query = query.Where(r => r.Rating <= 2);
        }

        var pageSize = 20;
        var reviews = await query
   .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
      .ToListAsync();

        var totalReviews = await query.CountAsync();

        var viewModel = new ReviewModerationViewModel
        {
            Reviews = reviews,
            CurrentPage = page,
    TotalPages = (int)Math.Ceiling(totalReviews / (double)pageSize),
       Filter = filter
        };

        return View(viewModel);
    }

    [HttpPost("Reviews/{id}/Delete")]
 public async Task<IActionResult> DeleteReview(Guid id, [FromForm] string reason)
    {
        var review = await _context.Reviews.FindAsync(id);
   if (review == null)
   return NotFound();

     review.IsDeleted = true;

        await LogAdminAction("Review Deleted", $"Review {id} deleted. Reason: {reason}", "Review");
      await _context.SaveChangesAsync();

  return Json(new { success = true, message = "Review deleted successfully" });
    }

    // ============================================
    // 9. ANALYTICS & REPORTS
    // ============================================

    [HttpGet("Analytics")]
 public async Task<IActionResult> Analytics()
    {
     var viewModel = new AnalyticsViewModel
 {
 // User Analytics
            TotalUsers = await _context.Users.CountAsync(),
       NewUsersThisMonth = await _context.Users
        .Where(u => u.CreatedAt >= DateTime.UtcNow.AddMonths(-1))
        .CountAsync(),
   
          // Order Analytics
        TotalOrders = await _context.Orders.CountAsync(),
    CompletedOrders = await _context.Orders
       .Where(o => o.Status == OrderStatus.Delivered)
.CountAsync(),
          
     // Revenue Analytics
 TotalRevenue = await _context.Payment
          .Where(p => p.PaymentStatus == Enums.PaymentStatus.Completed)
         .SumAsync(p => (decimal?)p.Amount) ?? 0,
     RevenueThisMonth = await _context.Payment
      .Where(p => p.PaymentStatus == Enums.PaymentStatus.Completed && p.PaidAt >= DateTimeOffset.UtcNow.AddMonths(-1))
    .SumAsync(p => (decimal?)p.Amount) ?? 0,
 
   // Tailor Performance
     TopTailors = await _context.TailorPerformanceViews
   .OrderByDescending(t => t.AverageRating)
  .Take(10)
       .ToListAsync(),
 
  // Monthly Revenue Chart
        MonthlyRevenue = await _context.Payment
           .Where(p => p.PaymentStatus == Enums.PaymentStatus.Completed && p.PaidAt >= DateTimeOffset.UtcNow.AddMonths(-12))
         .GroupBy(p => new { p.PaidAt.Year, p.PaidAt.Month })
  .Select(g => new MonthlyRevenueDto
        {
   Year = g.Key.Year,
 Month = g.Key.Month,
         Revenue = g.Sum(p => p.Amount)
       })
   .OrderBy(m => m.Year).ThenBy(m => m.Month)
           .ToListAsync()
    };

return View(viewModel);
    }

    // ============================================
 // 10. NOTIFICATIONS
    // ============================================

 [HttpGet("Notifications")]
    public async Task<IActionResult> Notifications()
    {
   var notifications = await _context.Notifications
     .Include(n => n.User)
   .OrderByDescending(n => n.SentAt)
      .Take(100)
        .ToListAsync();

return View(notifications);
    }

  [HttpPost("Notifications/Send")]
    public async Task<IActionResult> SendNotification([FromForm] SendNotificationDto dto)
    {
        // Get target users
        var userQuery = _context.Users.AsQueryable();

if (dto.TargetType == "Role")
     {
      userQuery = userQuery.Where(u => u.Role.Name == dto.TargetValue);
        }
    else if (dto.TargetType == "Specific")
      {
var userId = Guid.Parse(dto.TargetValue);
          userQuery = userQuery.Where(u => u.Id == userId);
        }

     var targetUsers = await userQuery.ToListAsync();

 foreach (var user in targetUsers)
     {
          var notification = new Notification
      {
      UserId = user.Id,
             Title = dto.Title,
          Message = dto.Message,
         Type = dto.Type,
     SentAt = DateTime.UtcNow
            };
    _context.Notifications.Add(notification);
        }

        await LogAdminAction("Notification Sent", $"Sent '{dto.Title}' to {targetUsers.Count} users", "Notification");
        await _context.SaveChangesAsync();

   TempData["Success"] = $"Notification sent to {targetUsers.Count} users";
        return RedirectToAction(nameof(Notifications));
    }

    // ============================================
    // 11. AUDIT LOGS
    // ============================================

    [HttpGet("AuditLogs")]
    public async Task<IActionResult> AuditLogs([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int page = 1)
  {
        var query = _context.UserActivityLogs.AsQueryable();

        if (startDate.HasValue)
    query = query.Where(a => a.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
       query = query.Where(a => a.CreatedAt <= endDate.Value);

   var pageSize = 50;
     var logs = await query
        .OrderByDescending(a => a.CreatedAt)
   .Skip((page - 1) * pageSize)
    .Take(pageSize)
      .Include(a => a.User)
    .Select(a => new AuditLogDto
      {
    Action = a.Action,
 Details = a.Details ?? "",  // Use Details field instead of EntityType
          PerformedBy = a.User != null ? a.User.Email : "Unknown",
        Timestamp = a.CreatedAt,
      IpAddress = a.IpAddress
  })
  .ToListAsync();

 var totalLogs = await query.CountAsync();

      var viewModel = new AuditLogViewModel
   {
   Logs = logs,
CurrentPage = page,
      TotalPages = (int)Math.Ceiling(totalLogs / (double)pageSize),
   StartDate = startDate,
 EndDate = endDate
  };

     return View(viewModel);
  }

    // ============================================
    // HELPER METHODS
    // ============================================

    private async Task LogAdminAction(string action, string details, string entityType = "System")
    {
        var adminEmail = User.Identity?.Name ?? "Unknown";
      
        var log = new UserActivityLog
        {
       UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) != null 
    ? Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value)
        : Guid.Empty,
  Action = action,
   EntityType = entityType,  // Actual entity type (e.g., "TailorProfile", "Order")
Details = details,         // Detailed description of the action
 IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
   CreatedAt = DateTime.UtcNow
    };

 _context.UserActivityLogs.Add(log);
    }
}

// Helper DTO for AuditLog display (to avoid conflict with Models.AuditLog)
public class AuditLogDto
{
    public string Action { get; set; } = string.Empty;
 public string Details { get; set; } = string.Empty;
 public string PerformedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}
