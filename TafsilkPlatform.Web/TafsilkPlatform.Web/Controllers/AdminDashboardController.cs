using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Controllers.Base;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels.Admin;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// Admin Dashboard Controller - Simplified version for basic admin functionality
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminDashboardController : BaseController
{
  private readonly ApplicationDbContext _db;

    public AdminDashboardController(
        ApplicationDbContext db,
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

    /// <summary>
    /// Suspend a user account
    /// POST: /AdminDashboard/SuspendUser/{id}
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspendUser(Guid id, string? reason)
    {
        try
        {
            var user = await _db.Users.FindAsync(id);
if (user == null)
       {
    TempData["Error"] = "المستخدم غير موجود";
    return RedirectToAction(nameof(Users));
            }

            // Prevent suspending other admins
          if (user.RoleId == await _db.Roles.Where(r => r.Name == "Admin").Select(r => r.Id).FirstOrDefaultAsync())
{
         TempData["Error"] = "لا يمكن تعليق حساب المسؤول";
     return RedirectToAction(nameof(UserDetails), new { id });
        }

user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
     
     await _db.SaveChangesAsync();

  _logger.LogInformation("User {UserId} suspended by admin. Reason: {Reason}", id, reason ?? "No reason provided");
      TempData["Success"] = "تم تعليق حساب المستخدم بنجاح";

        return RedirectToAction(nameof(UserDetails), new { id });
      }
   catch (Exception ex)
        {
     _logger.LogError(ex, "Error suspending user {UserId}", id);
            TempData["Error"] = "حدث خطأ أثناء تعليق الحساب";
            return RedirectToAction(nameof(UserDetails), new { id });
        }
    }

    /// <summary>
    /// Activate a suspended user account
 /// POST: /AdminDashboard/ActivateUser/{id}
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        try
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
 {
       TempData["Error"] = "المستخدم غير موجود";
  return RedirectToAction(nameof(Users));
            }

            user.IsActive = true;
 user.UpdatedAt = DateTime.UtcNow;
            
            await _db.SaveChangesAsync();

            _logger.LogInformation("User {UserId} activated by admin", id);
      TempData["Success"] = "تم تفعيل حساب المستخدم بنجاح";

     return RedirectToAction(nameof(UserDetails), new { id });
    }
    catch (Exception ex)
        {
        _logger.LogError(ex, "Error activating user {UserId}", id);
 TempData["Error"] = "حدث خطأ أثناء تفعيل الحساب";
            return RedirectToAction(nameof(UserDetails), new { id });
   }
    }

/// <summary>
    /// Soft delete a user account
    /// POST: /AdminDashboard/DeleteUser/{id}
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(Guid id, string? reason)
    {
        try
     {
        var user = await _db.Users.FindAsync(id);
      if (user == null)
   {
    TempData["Error"] = "المستخدم غير موجود";
             return RedirectToAction(nameof(Users));
            }

            // Prevent deleting other admins
         if (user.RoleId == await _db.Roles.Where(r => r.Name == "Admin").Select(r => r.Id).FirstOrDefaultAsync())
            {
                TempData["Error"] = "لا يمكن حذف حساب المسؤول";
              return RedirectToAction(nameof(UserDetails), new { id });
            }

            // Soft delete
     user.IsDeleted = true;
        user.IsActive = false;
   user.UpdatedAt = DateTime.UtcNow;
        
     await _db.SaveChangesAsync();

  _logger.LogWarning("User {UserId} deleted by admin. Reason: {Reason}", id, reason ?? "No reason provided");
            TempData["Success"] = "تم حذف حساب المستخدم بنجاح";

        return RedirectToAction(nameof(Users));
        }
    catch (Exception ex)
        {
  _logger.LogError(ex, "Error deleting user {UserId}", id);
 TempData["Error"] = "حدث خطأ أثناء حذف الحساب";
            return RedirectToAction(nameof(UserDetails), new { id });
 }
    }

    /// <summary>
    /// Update user role
    /// POST: /AdminDashboard/UpdateUserRole/{id}
    /// </summary>
    [HttpPost]
 [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUserRole(Guid id, Guid newRoleId)
    {
        try
  {
            var user = await _db.Users
            .Include(u => u.Role)
             .FirstOrDefaultAsync(u => u.Id == id);
         
            if (user == null)
    {
     TempData["Error"] = "المستخدم غير موجود";
                return RedirectToAction(nameof(Users));
        }

            var newRole = await _db.Roles.FindAsync(newRoleId);
   if (newRole == null)
    {
       TempData["Error"] = "الدور المحدد غير موجود";
                return RedirectToAction(nameof(UserDetails), new { id });
            }

            // Prevent changing admin roles
            if (user.Role?.Name == "Admin" || newRole.Name == "Admin")
   {
       TempData["Error"] = "لا يمكن تغيير دور المسؤول";
         return RedirectToAction(nameof(UserDetails), new { id });
 }

     user.RoleId = newRoleId;
     user.UpdatedAt = DateTime.UtcNow;
      
         await _db.SaveChangesAsync();

            _logger.LogInformation("User {UserId} role updated to {RoleName} by admin", id, newRole.Name);
    TempData["Success"] = $"تم تحديث دور المستخدم إلى {newRole.Name} بنجاح";

        return RedirectToAction(nameof(UserDetails), new { id });
        }
 catch (Exception ex)
     {
      _logger.LogError(ex, "Error updating user {UserId} role", id);
            TempData["Error"] = "حدث خطأ أثناء تحديث الدور";
            return RedirectToAction(nameof(UserDetails), new { id });
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

    /// <summary>
    /// Delete portfolio image (Admin action)
    /// POST: /AdminDashboard/DeletePortfolioImage/{id}
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePortfolioImage(Guid id, string? reason)
    {
        try
        {
    var image = await _db.PortfolioImages.FindAsync(id);
       if (image == null)
       {
                TempData["Error"] = "الصورة غير موجودة";
    return RedirectToAction(nameof(PortfolioReview));
            }

            image.IsDeleted = true;
            await _db.SaveChangesAsync();

   _logger.LogWarning("Portfolio image {ImageId} deleted by admin. Reason: {Reason}", id, reason ?? "No reason provided");
            TempData["Success"] = "تم حذف الصورة بنجاح";

  return RedirectToAction(nameof(PortfolioReview));
        }
        catch (Exception ex)
     {
            _logger.LogError(ex, "Error deleting portfolio image {ImageId}", id);
 TempData["Error"] = "حدث خطأ أثناء حذف الصورة";
         return RedirectToAction(nameof(PortfolioReview));
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

    /// <summary>
    /// Cancel order (Admin action)
    /// POST: /AdminDashboard/CancelOrder/{id}
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(Guid id, string? reason)
    {
        try
        {
       var order = await _db.Orders.FindAsync(id);
      if (order == null)
         {
     TempData["Error"] = "الطلب غير موجود";
          return RedirectToAction(nameof(Orders));
            }

 if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
        {
             TempData["Error"] = "لا يمكن إلغاء هذا الطلب";
 return RedirectToAction(nameof(Orders));
      }

         order.Status = OrderStatus.Cancelled;
      await _db.SaveChangesAsync();

            _logger.LogWarning("Order {OrderId} cancelled by admin. Reason: {Reason}", id, reason ?? "No reason provided");
   TempData["Success"] = "تم إلغاء الطلب بنجاح";

        return RedirectToAction(nameof(Orders));
  }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", id);
   TempData["Error"] = "حدث خطأ أثناء إلغاء الطلب";
    return RedirectToAction(nameof(Orders));
   }
    }

    #region Product Management

    /// <summary>
    /// Manage products
    /// GET: /AdminDashboard/Products
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Products(string? category = null, string? search = null, int page = 1)
    {
        try
     {
   var query = _db.Products.Where(p => !p.IsDeleted);

          if (!string.IsNullOrEmpty(category))
            {
    query = query.Where(p => p.Category == category);
         }

         if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
}

     var products = await query
     .Include(p => p.Tailor)
              .OrderByDescending(p => p.CreatedAt)
       .Skip((page - 1) * 20)
       .Take(20)
    .ToListAsync();

       ViewBag.TotalProducts = await query.CountAsync();
 ViewBag.CurrentPage = page;
            ViewBag.Category = category;
            ViewBag.Search = search;

            return View(products);
      }
    catch (Exception ex)
        {
    _logger.LogError(ex, "Error loading products page");
        TempData["Error"] = "حدث خطأ أثناء تحميل صفحة المنتجات";
     return View(new List<Product>());
        }
    }

  /// <summary>
    /// Toggle product availability
    /// POST: /AdminDashboard/ToggleProductAvailability/{id}
    /// </summary>
  [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleProductAvailability(Guid id)
    {
 try
 {
        var product = await _db.Products.FindAsync(id);
         if (product == null)
         {
        TempData["Error"] = "المنتج غير موجود";
     return RedirectToAction(nameof(Products));
    }

      product.IsAvailable = !product.IsAvailable;
     product.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

_logger.LogInformation("Product {ProductId} availability toggled to {IsAvailable} by admin", id, product.IsAvailable);
            TempData["Success"] = $"تم {(product.IsAvailable ? "تفعيل" : "إيقاف")} المنتج بنجاح";

      return RedirectToAction(nameof(Products));
     }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling product availability {ProductId}", id);
 TempData["Error"] = "حدث خطأ أثناء تحديث حالة المنتج";
       return RedirectToAction(nameof(Products));
        }
    }

    /// <summary>
    /// Delete product (Admin action)
    /// POST: /AdminDashboard/DeleteProduct/{id}
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(Guid id, string? reason)
    {
     try
  {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
      {
       TempData["Error"] = "المنتج غير موجود";
                return RedirectToAction(nameof(Products));
   }

         // Soft delete
   product.IsDeleted = true;
  product.IsAvailable = false;
          product.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

   _logger.LogWarning("Product {ProductId} deleted by admin. Reason: {Reason}", id, reason ?? "No reason provided");
    TempData["Success"] = "تم حذف المنتج بنجاح";

        return RedirectToAction(nameof(Products));
        }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error deleting product {ProductId}", id);
 TempData["Error"] = "حدث خطأ أثناء حذف المنتج";
      return RedirectToAction(nameof(Products));
        }
    }

    /// <summary>
    /// Update product stock
    /// POST: /AdminDashboard/UpdateProductStock/{id}
  /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
  public async Task<IActionResult> UpdateProductStock(Guid id, int newStock)
    {
        try
  {
            var product = await _db.Products.FindAsync(id);
 if (product == null)
            {
    TempData["Error"] = "المنتج غير موجود";
     return RedirectToAction(nameof(Products));
            }

        if (newStock < 0)
            {
                TempData["Error"] = "الكمية يجب أن تكون أكبر من أو تساوي صفر";
       return RedirectToAction(nameof(Products));
            }

       product.StockQuantity = newStock;
            product.UpdatedAt = DateTimeOffset.UtcNow;

            // Auto-disable if out of stock
          if (newStock == 0)
         {
       product.IsAvailable = false;
            }

     await _db.SaveChangesAsync();

   _logger.LogInformation("Product {ProductId} stock updated to {Stock} by admin", id, newStock);
       TempData["Success"] = "تم تحديث المخزون بنجاح";

       return RedirectToAction(nameof(Products));
        }
        catch (Exception ex)
     {
    _logger.LogError(ex, "Error updating product stock {ProductId}", id);
            TempData["Error"] = "حدث خطأ أثناء تحديث المخزون";
            return RedirectToAction(nameof(Products));
 }
    }

    #endregion

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
