using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels.Orders;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Services;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// Controller for managing orders (booking, tracking, management)
/// </summary>
[Route("orders")]
[Authorize]
public class OrdersController : Controller
{

    private readonly ApplicationDbContext _db;
    private readonly ILogger<OrdersController> _logger;
    private readonly IFileUploadService _fileUploadService;
    private readonly IOrderService _orderService;
    private readonly ImageUploadService _imageUploadService;
    private readonly IWebHostEnvironment _environment;
    private readonly IAttachmentService _attachmentService;
    public OrdersController(
        ApplicationDbContext db,
        ILogger<OrdersController> logger,
        IFileUploadService fileUploadService,
        IOrderService orderService,
        ImageUploadService imageUploadService,
        IWebHostEnvironment environment,
        IAttachmentService attachmentService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
    }

    #region Customer Order Viewing

    /// <summary>
    /// Main orders page - Redirects based on role
    /// GET: /orders
    /// </summary>
    [HttpGet("")]
    public IActionResult Index()
    {
        if (User.IsInRole("Tailor"))
        {
            return RedirectToAction(nameof(TailorOrders));
        }

        return RedirectToAction(nameof(MyOrders));
    }

    /// <summary>
    /// View customer orders
    /// GET: /orders/my-orders
    /// </summary>
    [HttpGet("my-orders")]
    [Authorize(Roles = "Customer")]
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
                TempData["Error"] = "Profile not found";
                return RedirectToAction("Index", "Home");
            }

            var orders = await _db.Orders
                .Include(o => o.Tailor)
                    .ThenInclude(t => t.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Payments)
                .Where(o => o.CustomerId == customer.Id)
                .OrderByDescending(o => o.CreatedAt)
                .AsSplitQuery()
                .ToListAsync();

            var model = new CustomerOrdersViewModel
            {
                Orders = orders.Select(o => new OrderSummaryViewModel
                {
                    OrderId = o.OrderId,
                    OrderNumber = !string.IsNullOrEmpty(o.OrderNumber) ? o.OrderNumber : o.OrderId.ToString().Substring(0, 8).ToUpper(),
                    Items = o.Items.Select(i => new OrderItemViewModel
                    {
                        ItemId = i.OrderItemId,
                        ServiceName = i.Description,
                        Quantity = i.Quantity,
                        Price = i.UnitPrice
                    }).ToList(),
                    TailorName = o.Tailor?.FullName ?? "Tailor",
                    TailorShopName = o.Tailor?.ShopName,
                    ServiceType = o.OrderType ?? "Not Specified",
                    Status = o.Status,
                    StatusDisplay = GetStatusDisplay(o.Status),
                    CreatedAt = o.CreatedAt,
                    DueDate = o.DueDate,
                    TotalPrice = (decimal)o.TotalPrice,
                    IsPaid = o.Payments.Any(p => p.PaymentStatus == Enums.PaymentStatus.Completed),
                    PaymentMethod = o.Payments.Any(p => p.PaymentType == Enums.PaymentType.Card)
                        ? "Credit/Debit Card"
                        : "Cash on Delivery"
                }).ToList()
            };

            return View("~/Areas/Customer/Views/Orders/MyOrders.cshtml", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading customer orders");
            TempData["Error"] = "An error occurred while loading orders";
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
                 .ThenInclude(i => i.Product)
                     .ThenInclude(p => p.Tailor) // ✅ NEW: Include Tailor info for items
         .Include(o => o.Payments)
          .Include(o => o.OrderImages)
        .AsSplitQuery()
        .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound("Order not found");

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
                OrderNumber = !string.IsNullOrEmpty(order.OrderNumber) ? order.OrderNumber : order.OrderId.ToString().Substring(0, 8).ToUpper(),
                Description = order.Description, // ✅ FIXED: Use correct property name
                ServiceType = order.OrderType,
                Status = order.Status,
                StatusDisplay = GetStatusDisplay(order.Status),
                CreatedAt = order.CreatedAt,
                DueDate = order.DueDate,
                TotalPrice = (decimal)order.TotalPrice,

                // Customer info
                CustomerName = order.Customer.User?.Email ?? "Customer",
                CustomerPhone = order.Customer.User?.PhoneNumber,

                // Tailor info
                TailorId = order.TailorId,
                TailorName = order.Tailor.FullName ?? "Tailor",
                TailorShopName = order.Tailor.ShopName,
                TailorPhone = order.Tailor.User?.PhoneNumber,
                TailorProfilePictureData = order.Tailor.ProfilePictureData,
                TailorProfilePictureContentType = order.Tailor.ProfilePictureContentType,

                // Order items
                Items = order.Items.Select(i => new OrderItemViewModel
                {
                    ItemId = i.OrderItemId,
                    ServiceName = i.Description ?? i.Product?.Name ?? "Product",
                    Quantity = i.Quantity,
                    Price = i.UnitPrice,
                    Notes = !string.IsNullOrEmpty(i.SpecialInstructions)
                        ? i.SpecialInstructions
                        : (!string.IsNullOrEmpty(i.SelectedSize) || !string.IsNullOrEmpty(i.SelectedColor))
                            ? $"Size: {i.SelectedSize ?? "N/A"}, Color: {i.SelectedColor ?? "N/A"}"
                            : null,
                    // ✅ NEW: Populate tailor info
                    TailorId = i.Product?.TailorId,
                    TailorName = i.Product?.Tailor?.FullName,
                    TailorShopName = i.Product?.Tailor?.ShopName,

                    // ✅ NEW: Populate product info
                    ProductId = i.ProductId,
                    ProductImageUrl = i.Product?.PrimaryImageUrl,
                    ProductImageData = i.Product?.PrimaryImageData,
                    ProductImageContentType = i.Product?.PrimaryImageContentType,
                    SelectedSize = i.SelectedSize,
                    SelectedColor = i.SelectedColor,
                    Category = i.Product?.Category,
                    Material = i.Product?.Material
                }).ToList(),

                // User role
                IsCustomer = customer != null && order.CustomerId == customer.Id,
                IsTailor = tailor != null && order.TailorId == tailor.Id,

                // Payment Info
                IsPaid = order.Payments.Any(p => p.PaymentStatus == Enums.PaymentStatus.Completed),
                PaymentMethod = order.Payments.Any(p => p.PaymentType == Enums.PaymentType.Card)
                    ? "Credit/Debit Card"
                    : "Cash on Delivery"
            };

            // ✅ NEW: Populate involved tailors list
            var uniqueTailors = order.Items
                .Where(i => i.Product?.Tailor != null)
                .Select(i => i.Product!.Tailor!)
                .DistinctBy(t => t.Id)
                .Select(t => new InvolvedTailorViewModel
                {
                    TailorId = t.Id,
                    Name = t.FullName ?? "Tailor",
                    ShopName = t.ShopName ?? "Tailor Shop",
                    Phone = t.User?.PhoneNumber,
                    ProfilePictureData = t.ProfilePictureData,
                    ProfilePictureContentType = t.ProfilePictureContentType,
                    ProfileImageUrl = t.ProfileImageUrl
                })
                .ToList();

            // If no product-specific tailors found (e.g. custom order), use the main order tailor
            if (!uniqueTailors.Any() && order.Tailor != null)
            {
                uniqueTailors.Add(new InvolvedTailorViewModel
                {
                    TailorId = order.Tailor.Id,
                    Name = order.Tailor.FullName ?? "Tailor",
                    ShopName = order.Tailor.ShopName ?? "Tailor Shop",
                    Phone = order.Tailor.User?.PhoneNumber,
                    ProfilePictureData = order.Tailor.ProfilePictureData,
                    ProfilePictureContentType = order.Tailor.ProfilePictureContentType,
                    ProfileImageUrl = order.Tailor.ProfileImageUrl
                });
            }

            model.InvolvedTailors = uniqueTailors;

            return View("~/Areas/Customer/Views/Orders/OrderDetails.cshtml", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading order details {OrderId}", id);
            TempData["Error"] = "An error occurred while loading order details";
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
                return NotFound("Order not found");

            // Check if order can be cancelled
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed && order.Status != OrderStatus.Processing)
            {
                TempData["Error"] = "Order cannot be cancelled at this stage";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            order.Status = OrderStatus.Cancelled;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} cancelled by customer {CustomerId}", id, customer.Id);
            TempData["Success"] = "Order cancelled successfully";

            return RedirectToAction(nameof(OrderDetails), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", id);
            TempData["Error"] = "An error occurred while cancelling order";
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
                TempData["Error"] = "Profile not found";
                return RedirectToAction("Index", "Home");
            }

            var orders = await _db.Orders
              .Include(o => o.Customer)
             .ThenInclude(c => c.User)
          .Include(o => o.Items)
                     .Include(o => o.Payments)
            .Where(o => o.TailorId == tailor.Id)
                     .OrderByDescending(o => o.CreatedAt)
                 .AsSplitQuery()
                 .ToListAsync();

            var model = new TailorOrdersViewModel
            {
                Orders = orders.Select(o => new OrderSummaryViewModel
                {
                    OrderId = o.OrderId,
                    CustomerName = o.Customer.User?.Email ?? "Customer",
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
            TempData["Error"] = "An error occurred while loading orders";
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
                return NotFound("Order not found");

            // Validate status transition
            if (!IsValidStatusTransition(order.Status, newStatus))
            {
                TempData["Error"] = "Invalid status transition";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            order.Status = newStatus;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} status updated to {Status} by tailor {TailorId}",
           id, newStatus, tailor.Id);
            TempData["Success"] = "Order status updated successfully";

            return RedirectToAction(nameof(OrderDetails), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status {OrderId}", id);
            TempData["Error"] = "An error occurred while updating order status";
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

            if (image == null)
                return NotFound();

            // Check authorization
            var customer = await _db.CustomerProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            var tailor = await _db.TailorProfiles.FirstOrDefaultAsync(t => t.UserId == userId);

            bool isAuthorized = (customer != null && image.Order.CustomerId == customer.Id) ||
                     (tailor != null && image.Order.TailorId == tailor.Id);

            if (!isAuthorized && !User.IsInRole("Admin"))
                return Forbid();

            // If stored in DB
            if (image.ImageData != null && image.ImageData.Length > 0)
            {
                return File(image.ImageData, image.ContentType ?? "image/jpeg");
            }

            // If stored on disk (ImgUrl)
            if (!string.IsNullOrWhiteSpace(image.ImgUrl))
            {
                if (Uri.IsWellFormedUriString(image.ImgUrl, UriKind.Absolute))
                {
                    return Redirect(image.ImgUrl);
                }

                if (image.ImgUrl.StartsWith('/'))
                {
                    var relativePath = image.ImgUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    var physicalPath = Path.Combine(_environment.WebRootPath ?? string.Empty, relativePath);
                    if (System.IO.File.Exists(physicalPath))
                    {
                        var contentType = image.ContentType ?? "image/jpeg";
                        return PhysicalFile(physicalPath, contentType);
                    }
                }

                return Redirect(image.ImgUrl);
            }

            return NotFound();
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


    private string GetStatusDisplay(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Pending",
            OrderStatus.PendingPayment => "Pending Payment",
            OrderStatus.Confirmed => "Confirmed",
            OrderStatus.Processing => "Processing",
            OrderStatus.Shipped => "Shipped",
            OrderStatus.ReadyForPickup => "Ready for Pickup",
            OrderStatus.Delivered => "Delivered",
            OrderStatus.Cancelled => "Cancelled",
            _ => "Not Specified"
        };
    }


    private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        // Define valid status transitions
        var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
 {
            { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.Confirmed, OrderStatus.Processing, OrderStatus.Cancelled } },
            { OrderStatus.PendingPayment, new List<OrderStatus> { OrderStatus.Confirmed, OrderStatus.Cancelled } },
            { OrderStatus.Confirmed, new List<OrderStatus> { OrderStatus.Processing, OrderStatus.Cancelled } },
            { OrderStatus.Processing, new List<OrderStatus> { OrderStatus.Shipped, OrderStatus.ReadyForPickup, OrderStatus.Cancelled } },
            { OrderStatus.Shipped, new List<OrderStatus> { OrderStatus.Delivered, OrderStatus.ReadyForPickup } },
            { OrderStatus.ReadyForPickup, new List<OrderStatus> { OrderStatus.Delivered } },
            { OrderStatus.Delivered, new List<OrderStatus>() },
            { OrderStatus.Cancelled, new List<OrderStatus>() }
   };

        return validTransitions.ContainsKey(currentStatus) &&
                validTransitions[currentStatus].Contains(newStatus);
    }

    #endregion
}
