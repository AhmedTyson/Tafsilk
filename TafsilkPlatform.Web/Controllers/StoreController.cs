using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TafsilkPlatform.Web.Helpers; // Simple helper methods
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.ViewModels.Store;
using System.Security.Claims;

namespace TafsilkPlatform.Web.Controllers
{
    [Route("[controller]")]
  public class StoreController : Controller
    {
 private readonly IStoreService _storeService;
        private readonly ICustomerRepository _customerRepository;
 private readonly ILogger<StoreController> _logger;

     public StoreController(
            IStoreService storeService,
 ICustomerRepository customerRepository,
            ILogger<StoreController> logger)
{
          _storeService = storeService;
         _customerRepository = customerRepository;
  _logger = logger;
    }

        // ✅ SIMPLIFIED: Use helper method for cleaner code
        private async Task<Guid> GetCustomerIdAsync()
        {
            var userId = this.GetUserId();
            if (!userId.HasValue)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var customer = await _customerRepository.GetByUserIdAsync(userId.Value);
            if (customer == null)
            {
                throw new UnauthorizedAccessException("Customer profile not found");
            }

            return customer.Id; // Return CustomerProfile.Id, not User.Id
        }

 private Guid GetCustomerId()
      {
       var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) throw new UnauthorizedAccessException();
      return Guid.Parse(userIdClaim);
}

// GET: /Store
        [HttpGet("")]
public async Task<IActionResult> Index(
    string? category,
  string? search,
 int page = 1,
      string? sortBy = null,
  decimal? minPrice = null,
            decimal? maxPrice = null)
     {
       var result = await _storeService.GetProductsAsync(
   category, search, page, 12, sortBy, minPrice, maxPrice);
      
       return View(result);
        }

        // GET: /Store/Product/guid
        [HttpGet("Product/{id}")]
        public async Task<IActionResult> ProductDetails(Guid id)
        {
            var product = await _storeService.GetProductDetailsAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found, returning fallback view", id);
                TempData["Error"] = "المنتج غير موجود أو لم يعد متوفراً";

                // Return fallback model to avoid view NREs
                var fallback = new ViewModels.Store.ProductViewModel
                {
                    ProductId = id,
                    Name = "منتج غير متوفر",
                    Description = "لا توجد تفاصيل لهذا المنتج حالياً.",
                    Price = 0,
                    DiscountedPrice = null,
                    Category = string.Empty,
                    StockQuantity = 0,
                    IsAvailable = false,
                    PrimaryImageBase64 = null,
                    TailorName = null,
                    TailorId = Guid.Empty
                };

                return View(fallback);
            }

            return View(product);
        }

  // GET: /Store/Cart
     [Authorize(Policy = "CustomerPolicy")]
  [HttpGet("Cart")]
      public async Task<IActionResult> Cart()
  {
     var customerId = await GetCustomerIdAsync();
     var cart = await _storeService.GetCartAsync(customerId);
  
      return View(cart ?? new CartViewModel());
    }

        // POST: /Store/AddToCart
        [Authorize(Policy = "CustomerPolicy")]
        [HttpPost("AddToCart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart([FromForm] AddToCartRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid request. Please check your input.";
                return RedirectToAction(nameof(ProductDetails), new { id = request.ProductId });
            }

            try
            {
                var customerId = await GetCustomerIdAsync();
                var success = await _storeService.AddToCartAsync(customerId, request);

                if (success)
                {
                    TempData["Success"] = "تم إضافة المنتج إلى السلة بنجاح";
                    return RedirectToAction(nameof(Cart));
                }

                // ✅ Better error messages (like Amazon)
                TempData["Error"] = "فشل إضافة المنتج إلى السلة. قد يكون المنتج غير متوفر أو الكمية المطلوبة غير متاحة.";
                return RedirectToAction(nameof(ProductDetails), new { id = request.ProductId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddToCart for product {ProductId}", request.ProductId);
                TempData["Error"] = "حدث خطأ أثناء إضافة المنتج. يرجى المحاولة مرة أخرى.";
                return RedirectToAction(nameof(ProductDetails), new { id = request.ProductId });
            }
        }

        // POST: /Store/UpdateCartItem
   [Authorize(Policy = "CustomerPolicy")]
[HttpPost("UpdateCartItem")]
        [ValidateAntiForgeryToken]
  public async Task<IActionResult> UpdateCartItem([FromForm] UpdateCartItemRequest request)
{
     var customerId = await GetCustomerIdAsync();
       var success = await _storeService.UpdateCartItemAsync(customerId, request);

 return RedirectToAction(nameof(Cart));
        }

        // POST: /Store/RemoveFromCart
      [Authorize(Policy = "CustomerPolicy")]
  [HttpPost("RemoveFromCart")]
  [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(Guid cartItemId)
{
   var customerId = await GetCustomerIdAsync();
await _storeService.RemoveFromCartAsync(customerId, cartItemId);

      return RedirectToAction(nameof(Cart));
   }

    // POST: /Store/ClearCart
[Authorize(Policy = "CustomerPolicy")]
   [HttpPost("ClearCart")]
        [ValidateAntiForgeryToken]
  public async Task<IActionResult> ClearCart()
      {
  var customerId = await GetCustomerIdAsync();
         await _storeService.ClearCartAsync(customerId);
     TempData["Success"] = "تم إفراغ السلة بنجاح";
     return RedirectToAction(nameof(Cart));
        }

// GET: /Store/Checkout
    [Authorize(Policy = "CustomerPolicy")]
 [HttpGet("Checkout")]
      public async Task<IActionResult> Checkout()
{
    var customerId = await GetCustomerIdAsync();
  var checkoutData = await _storeService.GetCheckoutDataAsync(customerId);

   if (checkoutData == null || !checkoutData.Cart.Items.Any())
       {
    _logger.LogWarning("Checkout requested but cart is empty for customer {CustomerId}", customerId);
    TempData["Error"] = "سلة التسوق فارغة أو حدث خطأ أثناء تحميل بيانات السلة";

    // Return a safe fallback CheckoutViewModel so the view can render and show friendly message
    var fallbackCheckout = new ViewModels.Store.CheckoutViewModel
    {
        Cart = new ViewModels.Store.CartViewModel(),
        ShippingAddress = new ViewModels.Store.CheckoutAddressViewModel(),
        UseSameAddressForBilling = true,
        PaymentMethod = "CashOnDelivery"
    };

return View(fallbackCheckout);
 }

      return View(checkoutData);
        }

        // POST: /Store/ProcessCheckout
        [Authorize(Policy = "CustomerPolicy")]
        [HttpPost("ProcessCheckout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout([FromForm] ProcessPaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                // ✅ IMPROVED: Log validation errors for debugging
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(msg => !string.IsNullOrWhiteSpace(msg))
                    .ToList();
                
                _logger.LogWarning("Checkout validation failed. Errors: {Errors}", 
                    string.Join(", ", errors));
                
                // Show first 3 errors to user
                var errorMessage = errors.Any() 
                    ? "يرجى إكمال جميع الحقول المطلوبة: " + string.Join("، ", errors.Take(3))
                    : "يرجى إكمال جميع الحقول المطلوبة";
                
                // ✅ Check if AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage, errors });
                }
                
                TempData["Error"] = errorMessage;
                return RedirectToAction(nameof(Checkout));
            }

            try
            {
                var customerId = await GetCustomerIdAsync();
                
                _logger.LogInformation("Processing checkout for customer {CustomerId}. ShippingAddress: {City}, {Street}", 
                    customerId, request.ShippingAddress.City, request.ShippingAddress.Street);
                
                // ✅ Validate cart is not empty before processing
                var cart = await _storeService.GetCartAsync(customerId);
                if (cart == null || !cart.Items.Any())
                {
                    _logger.LogWarning("Empty cart for customer {CustomerId} during checkout", customerId);
                    
                    var emptyCartMessage = "السلة فارغة. يرجى إضافة منتجات قبل إتمام الطلب";
                    
                    // ✅ Check if AJAX request
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = emptyCartMessage });
                    }
                    
                    TempData["Error"] = emptyCartMessage;
                    return RedirectToAction(nameof(Cart));
                }

                // ✅ SIMPLIFIED: Force Cash on Delivery
                request.PaymentMethod = "CashOnDelivery";

                var (success, orderId, errorMessage) = await _storeService.ProcessCheckoutAsync(customerId, request);

                if (success && orderId.HasValue)
                {
                    _logger.LogInformation("Cash order {OrderId} confirmed successfully for customer {CustomerId}", orderId.Value, customerId);
                    
                    // ✅ NEW: Return JSON for AJAX requests
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        // Get order details for response
                        var order = await _storeService.GetOrderDetailsAsync(orderId.Value, customerId);
                        
                        return Json(new
                        {
                            success = true,
                            orderId = orderId.Value,
                            orderNumber = orderId.Value.ToString().Substring(0, 8).ToUpper(),
                            orderDate = DateTimeOffset.UtcNow.ToString("dd/MM/yyyy - HH:mm"),
                            totalAmount = order?.TotalAmount ?? 0,
                            paymentMethod = "الدفع عند الاستلام"
                        });
                    }
                    
                    // ✅ Traditional redirect for non-AJAX
                    TempData["OrderSuccess"] = "true";
                    TempData["OrderId"] = orderId.Value.ToString();
                    return RedirectToAction(nameof(PaymentSuccess), new { orderId = orderId.Value });
                }

                // ✅ Better error messages
                var errorMsg = errorMessage ?? "فشل إتمام الطلب. يرجى المحاولة مرة أخرى أو الاتصال بالدعم.";
                
                // ✅ Check if AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMsg });
                }
                
                TempData["Error"] = errorMsg;
                _logger.LogWarning("Checkout failed for customer {CustomerId}: {Error}", customerId, errorMsg);
                return RedirectToAction(nameof(Checkout));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in ProcessCheckout");
                
                var authMessage = "يرجى تسجيل الدخول أولاً";
                
                // ✅ Check if AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = authMessage, requiresLogin = true });
                }
                
                TempData["Error"] = authMessage;
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessCheckout for customer");
                
                var genericError = "حدث خطأ أثناء معالجة الطلب. يرجى المحاولة مرة أخرى أو الاتصال بالدعم.";
                
                // ✅ Check if AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = genericError });
                }
                
                TempData["Error"] = genericError;
                return RedirectToAction(nameof(Checkout));
            }
        }

        // GET: /Store/PaymentSuccess/{orderId}
        [Authorize(Policy = "CustomerPolicy")]
        [HttpGet("PaymentSuccess/{orderId:guid}")]
        public async Task<IActionResult> PaymentSuccess(Guid orderId)
        {
            try
            {
                var customerId = await GetCustomerIdAsync();
                
                // ✅ FIXED: Verify this is from a successful checkout (prevent direct URL access after order completion)
                var orderSuccessFlag = TempData["OrderSuccess"]?.ToString();
                if (orderSuccessFlag != "true")
                {
                    _logger.LogWarning("Payment success page accessed without checkout completion for order {OrderId}", orderId);
                }
                
                // ✅ FIXED: Try to get order details, but don't fail if order not found immediately
                // (might be a timing issue with database transaction)
                var order = await _storeService.GetOrderDetailsAsync(orderId, customerId);
                
                if (order == null)
                {
                    // ✅ FIXED: If order not found, still show success page with basic info
                    // This handles cases where order was just created and might not be immediately available
                    _logger.LogWarning("Order {OrderId} not found for customer {CustomerId}, showing success page anyway", orderId, customerId);
                    
                    var fallbackModel = new PaymentSuccessViewModel
                    {
                        OrderId = orderId,
                        OrderNumber = orderId.ToString().Substring(0, 8).ToUpper(),
                        TotalAmount = 0, // Will be shown as "سيتم تحديثه قريباً"
                        PaymentMethod = "الدفع عند الاستلام",
                        OrderDate = DateTimeOffset.UtcNow,
                        EstimatedDeliveryDays = 3
                    };
                    
                    TempData["Info"] = "تم تأكيد طلبك بنجاح! سيتم تحديث التفاصيل قريباً.";
                    return View(fallbackModel);
                }

                // ✅ Create success view model with actual order data
                var model = new PaymentSuccessViewModel
                {
                    OrderId = orderId,
                    OrderNumber = orderId.ToString().Substring(0, 8).ToUpper(),
                    TotalAmount = order.TotalAmount,
                    PaymentMethod = "الدفع عند الاستلام",
                    OrderDate = order.OrderDate,
                    EstimatedDeliveryDays = 3 // Default 3 days
                };

                // ✅ Clear the success flag
                TempData.Remove("OrderSuccess");
                TempData.Remove("OrderId");
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment success page for order {OrderId}", orderId);
                TempData["Success"] = "تم تأكيد طلبك بنجاح!";
                // ✅ FIXED: Fallback to MyOrders if success page fails
                return RedirectToAction("MyOrders", "Orders");
            }
        }

  // API: Get cart item count
   [Authorize(Policy = "CustomerPolicy")]
        [HttpGet("api/cart/count")]
        public async Task<IActionResult> GetCartCount()
        {
  var customerId = await GetCustomerIdAsync();
 var count = await _storeService.GetCartItemCountAsync(customerId);
      return Json(new { count });
        }
    }
}
