using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.ViewModels.Orders;
using TafsilkPlatform.Web.Services;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// Controller for managing orders (booking, tracking, management)
/// </summary>
[Route("orders")]
[Authorize]
public class OrdersController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrdersController> _logger;
    private readonly IFileUploadService _fileUploadService;

    public OrdersController(
        AppDbContext db,
        ILogger<OrdersController> logger,
        IFileUploadService fileUploadService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
     _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
    }

    #region Customer Order Actions

/// <summary>
    /// Create new order (booking page)
    /// GET: /orders/create/{tailorId}
    /// </summary>
    [HttpGet("create/{tailorId:guid}")]
    [Authorize(Roles = "Customer,Corporate")]
    public async Task<IActionResult> CreateOrder(Guid tailorId)
    {
        try
        {
            var userId = GetCurrentUserId();
     if (userId == Guid.Empty) return Unauthorized();

          // Get tailor information
            var tailor = await _db.TailorProfiles
        .Include(t => t.User)
  .Include(t => t.TailorServices.Where(s => !s.IsDeleted))
     .Include(t => t.Reviews)
         .FirstOrDefaultAsync(t => t.Id == tailorId);

         if (tailor == null)
            {
    TempData["Error"] = "الخياط غير موجود";
        return RedirectToAction("SearchTailors", "Profiles");
            }

            // Check if tailor is verified and active
 if (!tailor.IsVerified)
    {
    TempData["Error"] = "هذا الخياط غير مفعّل حالياً";
                return RedirectToAction("ViewPublicTailorProfile", "Profiles", new { id = tailorId });
  }

 // Get customer profile
          var customer = await _db.CustomerProfiles
   .FirstOrDefaultAsync(c => c.UserId == userId);

if (customer == null)
   {
     TempData["Error"] = "الملف الشخصي غير موجود";
    return RedirectToAction("Index", "Home");
   }

   var model = new CreateOrderViewModel
            {
          TailorId = tailorId,
     TailorName = tailor.FullName ?? "خياط",
          TailorShopName = tailor.ShopName,
     TailorCity = tailor.City,
          TailorDistrict = tailor.District,
         TailorAverageRating = tailor.AverageRating,
      TailorReviewCount = tailor.Reviews?.Count ?? 0,
            TailorProfilePictureData = tailor.ProfilePictureData,
         TailorProfilePictureContentType = tailor.ProfilePictureContentType,
         AvailableServices = tailor.TailorServices
    .Where(s => !s.IsDeleted)
          .Select(s => new ServiceOptionViewModel
         {
                 ServiceId = s.TailorServiceId,
     ServiceName = s.ServiceName,
       ServiceDescription = s.Description,
 ServicePrice = s.BasePrice,
               ServiceIcon = GetServiceIcon(s.ServiceName)
      })
                    .ToList(),
   CustomerId = customer.Id
 };

       return View(model);
        }
   catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading order creation page for tailor {TailorId}", tailorId);
            TempData["Error"] = "حدث خطأ أثناء تحميل الصفحة";
return RedirectToAction("SearchTailors", "Profiles");
        }
  }

  /// <summary>
    /// Submit new order
    /// POST: /orders/create
    /// </summary>
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Customer,Corporate")]
    public async Task<IActionResult> CreateOrder(CreateOrderViewModel model)
    {
        try
        {
       var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

            if (!ModelState.IsValid)
            {
          // Reload tailor services for the view
   await ReloadOrderViewModel(model);
             return View(model);
            }

            // Get customer profile
        var customer = await _db.CustomerProfiles
              .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
     {
         TempData["Error"] = "الملف الشخصي غير موجود";
       return RedirectToAction("Index", "Home");
            }

            // Verify tailor exists and is active
      var tailor = await _db.TailorProfiles
             .FirstOrDefaultAsync(t => t.Id == model.TailorId);

    if (tailor == null || !tailor.IsVerified)
            {
    ModelState.AddModelError("", "الخياط غير متاح حالياً");
     await ReloadOrderViewModel(model);
         return View(model);
            }

            // Create order
     var order = new Order
         {
                OrderId = Guid.NewGuid(),
    CustomerId = customer.Id,
        TailorId = model.TailorId,
  Description = model.Description ?? string.Empty, // ✅ FIXED: Use correct property name
         OrderType = model.ServiceType ?? "خدمة عامة",
     Status = OrderStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
    DueDate = model.DueDate,
       TotalPrice = (double)model.EstimatedPrice,
      Customer = customer,
        Tailor = tailor
  };

            _db.Orders.Add(order);

            // Save order images if provided
        if (model.ReferenceImages != null && model.ReferenceImages.Any())
  {
        foreach (var image in model.ReferenceImages)
          {
     if (image.Length > 0)
            {
     // Validate image
             if (!_fileUploadService.IsValidImage(image))
       continue;

            using (var memoryStream = new MemoryStream())
                {
                    await image.CopyToAsync(memoryStream);
           var orderImage = new OrderImages
        {
  OrderImageId = Guid.NewGuid(),
              OrderId = order.OrderId,
    ImageData = memoryStream.ToArray(),
        ContentType = image.ContentType,
     ImgUrl = string.Empty,
            UploadedId = userId.ToString(),
       Order = order,
    CreatedAt = DateTime.UtcNow
           };
_db.OrderImages.Add(orderImage);
           }
    }
          }
        }

        // Save measurements as order items (if provided)
            if (!string.IsNullOrEmpty(model.Measurements))
          {
 var orderItem = new OrderItem
        {
     OrderItemId = Guid.NewGuid(),
         OrderId = order.OrderId,
        ItemName = model.ServiceType ?? "خدمة عامة",
    Quantity = 1,
       UnitPrice = model.EstimatedPrice,
    Total = model.EstimatedPrice,
    Order = order
     };
 _db.OrderItems.Add(orderItem);
            }

   await _db.SaveChangesAsync();

          _logger.LogInformation("Order {OrderId} created by customer {CustomerId}", order.OrderId, customer.Id);
          TempData["Success"] = "تم إنشاء الطلب بنجاح! سيتواصل معك الخياط قريباً.";

        return RedirectToAction(nameof(OrderDetails), new { id = order.OrderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
    ModelState.AddModelError("", "حدث خطأ أثناء إنشاء الطلب");
            await ReloadOrderViewModel(model);
     return View(model);
        }
  }

    /// <summary>
    /// View customer orders
    /// GET: /orders/my-orders
    /// </summary>
    [HttpGet("my-orders")]
    [Authorize(Roles = "Customer,Corporate")]
    public async Task<IActionResult> MyOrders()
    {
        try
        {
         var userId = GetCurrentUserId();
    if (userId == Guid.Empty) return Unauthorized();

     var customer = await _db.CustomerProfiles
         .FirstOrDefaultAsync(c => c.UserId == userId);

      if (customer == null)
      {
    TempData["Error"] = "الملف الشخصي غير موجود";
 return RedirectToAction("Index", "Home");
      }

            var orders = await _db.Orders
 .Include(o => o.Tailor)
     .ThenInclude(t => t.User)
     .Include(o => o.Items)
    .Include(o => o.Payments)
    .Where(o => o.CustomerId == customer.Id)
    .OrderByDescending(o => o.CreatedAt)
      .ToListAsync();

  var model = new CustomerOrdersViewModel
            {
         Orders = orders.Select(o => new OrderSummaryViewModel
      {
        OrderId = o.OrderId,
       TailorName = o.Tailor.FullName ?? "خياط",
       TailorShopName = o.Tailor.ShopName,
    ServiceType = o.OrderType,
           Status = o.Status,
        StatusDisplay = GetStatusDisplay(o.Status),
       CreatedAt = o.CreatedAt,
           DueDate = o.DueDate,
            TotalPrice = (decimal)o.TotalPrice,
            IsPaid = o.Payments.Any(p => p.PaymentStatus == Enums.PaymentStatus.Completed)
        }).ToList()
  };

            return View(model);
      }
      catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading customer orders");
 TempData["Error"] = "حدث خطأ أثناء تحميل الطلبات";
            return RedirectToAction("Index", "Home");
        }
    }

  /// <summary>
    /// View order details and tracking
    /// GET: /orders/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> OrderDetails(Guid id)
    {
    try
 {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var order = await _db.Orders
      .Include(o => o.Customer)
          .ThenInclude(c => c.User)
     .Include(o => o.Tailor)
               .ThenInclude(t => t.User)
             .Include(o => o.Items)
         .Include(o => o.Payments)
          .Include(o => o.orderImages)
        .FirstOrDefaultAsync(o => o.OrderId == id);

     if (order == null)
        return NotFound("الطلب غير موجود");

         // Check authorization
   var customer = await _db.CustomerProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
      var tailor = await _db.TailorProfiles.FirstOrDefaultAsync(t => t.UserId == userId);

            bool isAuthorized = (customer != null && order.CustomerId == customer.Id) ||
           (tailor != null && order.TailorId == tailor.Id);

            if (!isAuthorized && !User.IsInRole("Admin"))
            {
    _logger.LogWarning("Unauthorized access attempt to order {OrderId} by user {UserId}", id, userId);
          return Forbid();
   }

 var model = new OrderDetailsViewModel
      {
         OrderId = order.OrderId,
         OrderNumber = order.OrderId.ToString().Substring(0, 8).ToUpper(),
     Description = order.Description, // ✅ FIXED: Use correct property name
     ServiceType = order.OrderType,
    Status = order.Status,
     StatusDisplay = GetStatusDisplay(order.Status),
                CreatedAt = order.CreatedAt,
         DueDate = order.DueDate,
            TotalPrice = (decimal)order.TotalPrice,

     // Customer info
     CustomerName = order.Customer.User?.Email ?? "عميل",
   CustomerPhone = order.Customer.User?.PhoneNumber,

    // Tailor info
         TailorId = order.TailorId,
       TailorName = order.Tailor.FullName ?? "خياط",
        TailorShopName = order.Tailor.ShopName,
                TailorPhone = order.Tailor.User?.PhoneNumber,
             TailorProfilePictureData = order.Tailor.ProfilePictureData,
                TailorProfilePictureContentType = order.Tailor.ProfilePictureContentType,

            // Order items
           Items = order.Items.Select(i => new OrderItemViewModel
   {
    ItemId = i.OrderItemId,
          ServiceName = i.ItemName,
         Quantity = i.Quantity,
               Price = i.UnitPrice,
          Notes = $"المقاسات والملاحظات" // Can be extended
           }).ToList(),

     // Payment info
     IsPaid = order.Payments.Any(p => p.PaymentStatus == Enums.PaymentStatus.Completed),
      PaymentAmount = order.Payments
      .Where(p => p.PaymentStatus == Enums.PaymentStatus.Completed)
     .Sum(p => p.Amount),

   // Images
                ReferenceImages = order.orderImages.Select(img => new OrderImageViewModel
         {
          ImageId = img.OrderImageId,
          ContentType = img.ContentType
        }).ToList(),

        // User role
    IsCustomer = customer != null && order.CustomerId == customer.Id,
      IsTailor = tailor != null && order.TailorId == tailor.Id
          };

   return View(model);
        }
        catch (Exception ex)
        {
     _logger.LogError(ex, "Error loading order details {OrderId}", id);
       TempData["Error"] = "حدث خطأ أثناء تحميل تفاصيل الطلب";
         return RedirectToAction(nameof(MyOrders));
        }
    }

    /// <summary>
    /// Cancel order
    /// POST: /orders/{id}/cancel
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Customer,Corporate")]
    public async Task<IActionResult> CancelOrder(Guid id, string? cancellationReason)
    {
     try
     {
    var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

            var customer = await _db.CustomerProfiles
    .FirstOrDefaultAsync(c => c.UserId == userId);

     if (customer == null)
      return Unauthorized();

   var order = await _db.Orders
         .FirstOrDefaultAsync(o => o.OrderId == id && o.CustomerId == customer.Id);

      if (order == null)
  return NotFound("الطلب غير موجود");

  // Check if order can be cancelled
    if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
            {
       TempData["Error"] = "لا يمكن إلغاء الطلب في هذه المرحلة";
     return RedirectToAction(nameof(OrderDetails), new { id });
            }

order.Status = OrderStatus.Cancelled;
          await _db.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} cancelled by customer {CustomerId}", id, customer.Id);
       TempData["Success"] = "تم إلغاء الطلب بنجاح";

         return RedirectToAction(nameof(OrderDetails), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", id);
            TempData["Error"] = "حدث خطأ أثناء إلغاء الطلب";
   return RedirectToAction(nameof(OrderDetails), new { id });
        }
    }

    #endregion

    #region Tailor Order Management

    /// <summary>
    /// View tailor orders
    /// GET: /orders/tailor/manage
    /// </summary>
    [HttpGet("tailor/manage")]
    [Authorize(Roles = "Tailor")]
    public async Task<IActionResult> TailorOrders()
    {
        try
 {
   var userId = GetCurrentUserId();
   if (userId == Guid.Empty) return Unauthorized();

            var tailor = await _db.TailorProfiles
     .FirstOrDefaultAsync(t => t.UserId == userId);

            if (tailor == null)
        {
        TempData["Error"] = "الملف الشخصي غير موجود";
        return RedirectToAction("Index", "Home");
            }

   var orders = await _db.Orders
     .Include(o => o.Customer)
    .ThenInclude(c => c.User)
 .Include(o => o.Items)
            .Include(o => o.Payments)
   .Where(o => o.TailorId == tailor.Id)
            .OrderByDescending(o => o.CreatedAt)
        .ToListAsync();

   var model = new TailorOrdersViewModel
       {
        Orders = orders.Select(o => new OrderSummaryViewModel
           {
 OrderId = o.OrderId,
       CustomerName = o.Customer.User?.Email ?? "عميل",
          ServiceType = o.OrderType,
            Status = o.Status,
       StatusDisplay = GetStatusDisplay(o.Status),
    CreatedAt = o.CreatedAt,
                    DueDate = o.DueDate,
      TotalPrice = (decimal)o.TotalPrice,
     IsPaid = o.Payments.Any(p => p.PaymentStatus == Enums.PaymentStatus.Completed)
       }).ToList(),

      // Statistics
                TotalOrders = orders.Count,
  PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
       ProcessingOrders = orders.Count(o => o.Status == OrderStatus.Processing),
          CompletedOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
        TotalRevenue = orders
           .Where(o => o.Status == OrderStatus.Delivered)
        .Sum(o => (decimal)o.TotalPrice)
            };

 return View(model);
        }
        catch (Exception ex)
        {
 _logger.LogError(ex, "Error loading tailor orders");
            TempData["Error"] = "حدث خطأ أثناء تحميل الطلبات";
    return RedirectToAction("TailorProfile", "Profiles");
        }
    }

    /// <summary>
    /// Update order status
    /// POST: /orders/{id}/update-status
    /// </summary>
  [HttpPost("{id:guid}/update-status")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Tailor")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, OrderStatus newStatus, string? notes)
    {
 try
        {
         var userId = GetCurrentUserId();
    if (userId == Guid.Empty) return Unauthorized();

            var tailor = await _db.TailorProfiles
             .FirstOrDefaultAsync(t => t.UserId == userId);

            if (tailor == null)
            return Unauthorized();

         var order = await _db.Orders
    .FirstOrDefaultAsync(o => o.OrderId == id && o.TailorId == tailor.Id);

        if (order == null)
            return NotFound("الطلب غير موجود");

       // Validate status transition
            if (!IsValidStatusTransition(order.Status, newStatus))
   {
    TempData["Error"] = "تحديث الحالة غير صالح";
   return RedirectToAction(nameof(OrderDetails), new { id });
      }

  order.Status = newStatus;
        await _db.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} status updated to {Status} by tailor {TailorId}",
           id, newStatus, tailor.Id);
            TempData["Success"] = "تم تحديث حالة الطلب بنجاح";

      return RedirectToAction(nameof(OrderDetails), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status {OrderId}", id);
   TempData["Error"] = "حدث خطأ أثناء تحديث حالة الطلب";
            return RedirectToAction(nameof(OrderDetails), new { id });
        }
  }

    #endregion

    #region Order Images

    /// <summary>
    /// Get order image
    /// GET: /orders/image/{imageId}
    /// </summary>
    [HttpGet("image/{imageId:guid}")]
    public async Task<IActionResult> GetOrderImage(Guid imageId)
    {
        try
        {
            var userId = GetCurrentUserId();
       if (userId == Guid.Empty) return Unauthorized();

     var image = await _db.OrderImages
         .Include(i => i.Order)
      .FirstOrDefaultAsync(i => i.OrderImageId == imageId);

            if (image == null || image.ImageData == null)
       return NotFound();

      // Check authorization
          var customer = await _db.CustomerProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
       var tailor = await _db.TailorProfiles.FirstOrDefaultAsync(t => t.UserId == userId);

 bool isAuthorized = (customer != null && image.Order.CustomerId == customer.Id) ||
          (tailor != null && image.Order.TailorId == tailor.Id);

    if (!isAuthorized && !User.IsInRole("Admin"))
       return Forbid();

    return File(image.ImageData, image.ContentType ?? "image/jpeg");
    }
        catch (Exception ex)
      {
       _logger.LogError(ex, "Error retrieving order image {ImageId}", imageId);
   return NotFound();
        }
    }

  #endregion

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
  ?? User.FindFirst("sub");

        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
    }

        _logger.LogWarning("Unable to extract valid user ID from claims");
    return Guid.Empty;
    }

    private async Task ReloadOrderViewModel(CreateOrderViewModel model)
    {
        var tailor = await _db.TailorProfiles
   .Include(t => t.TailorServices.Where(s => !s.IsDeleted))
    .FirstOrDefaultAsync(t => t.Id == model.TailorId);

        if (tailor != null)
        {
     model.AvailableServices = tailor.TailorServices
    .Where(s => !s.IsDeleted)
     .Select(s => new ServiceOptionViewModel
       {
        ServiceId = s.TailorServiceId,
     ServiceName = s.ServiceName,
   ServiceDescription = s.Description,
                    ServicePrice = s.BasePrice,
 ServiceIcon = GetServiceIcon(s.ServiceName)
   })
  .ToList();
    }
    }

    private string GetServiceIcon(string serviceName)
    {
        var serviceIconMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "بدل رجالية", "fa-user-tie" },
   { "فساتين سهرة", "fa-tshirt" },
            { "فساتين زفاف", "fa-ring" },
          { "تعديلات", "fa-tools" },
         { "ملابس أطفال", "fa-child" },
   { "ثياب", "fa-dharmachakra" },
     { "عبايات", "fa-user-tie" }
        };

  foreach (var kvp in serviceIconMap)
        {
        if (serviceName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
        return kvp.Value;
        }

        return "fa-cut"; // Default icon
    }

    private string GetStatusDisplay(OrderStatus status)
    {
        return status switch
        {
   OrderStatus.Pending => "قيد الانتظار",
        OrderStatus.Processing => "قيد التنفيذ",
    OrderStatus.Shipped => "قيد الشحن",
            OrderStatus.Delivered => "تم التسليم",
            OrderStatus.Cancelled => "ملغي",
   _ => "غير محدد"
        };
    }

    private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        // Define valid status transitions
        var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
        {
            { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.Processing, OrderStatus.Cancelled } },
            { OrderStatus.Processing, new List<OrderStatus> { OrderStatus.Shipped, OrderStatus.Cancelled } },
{ OrderStatus.Shipped, new List<OrderStatus> { OrderStatus.Delivered } },
            { OrderStatus.Delivered, new List<OrderStatus>() },
            { OrderStatus.Cancelled, new List<OrderStatus>() }
        };

        return validTransitions.ContainsKey(currentStatus) &&
          validTransitions[currentStatus].Contains(newStatus);
    }

    #endregion
}
