using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TafsilkPlatform.Models.ViewModels.Store;
using TafsilkPlatform.Utility.Extensions; // Extension methods
using TafsilkPlatform.Web.Interfaces;

namespace TafsilkPlatform.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Route("/Store")]
    public class StoreController : Controller
    {
        private readonly IStoreService _storeService;
        private readonly TafsilkPlatform.Web.Services.Interfaces.IReviewService _reviewService;
        private readonly TafsilkPlatform.DataAccess.Repository.ICustomerRepository _customerRepository;
        private readonly TafsilkPlatform.DataAccess.Repository.IUnitOfWork _unitOfWork;
        private readonly ILogger<StoreController> _logger;

        public StoreController(
               IStoreService storeService,
               TafsilkPlatform.Web.Services.Interfaces.IReviewService reviewService,
               TafsilkPlatform.DataAccess.Repository.ICustomerRepository customerRepository,
               TafsilkPlatform.DataAccess.Repository.IUnitOfWork unitOfWork,
               ILogger<StoreController> logger)
        {
            _storeService = storeService;
            _reviewService = reviewService;
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ✅ SIMPLIFIED: Use helper method for cleaner code
        private async Task<Guid> GetCustomerIdAsync()
        {
            var userId = User.GetUserId();
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

            return View("~/Areas/Customer/Views/Shop/Index.cshtml", result);
        }

        // GET: /Store/Product/guid
        [HttpGet("Product/{id}")]
        public async Task<IActionResult> ProductDetails(Guid id)
        {
            var product = await _storeService.GetProductDetailsAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found, returning fallback view", id);
                TempData["Error"] = "Product not found or no longer available";

                // Return fallback model to avoid view NREs
                var fallback = new TafsilkPlatform.Models.ViewModels.Store.ProductViewModel
                {
                    ProductId = id,
                    Name = "Product Unavailable",
                    Description = "No details available for this product.",
                    Price = 0,
                    DiscountedPrice = null,
                    Category = string.Empty,
                    StockQuantity = 0,
                    IsAvailable = false,
                    PrimaryImageBase64 = null,
                    TailorName = null,
                    TailorId = Guid.Empty
                };

                return View("~/Areas/Customer/Views/Shop/ProductDetails.cshtml", fallback);
            }

            // ✅ REVIEWS: Fetch reviews and check permission
            product.Reviews = await _reviewService.GetProductReviewsAsync(id);

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.GetUserId();
                if (userId.HasValue)
                {
                    product.CanReview = await _reviewService.CanUserReviewProductAsync(userId.Value, id);
                }
            }

            return View("~/Areas/Customer/Views/Shop/ProductDetails.cshtml", product);
        }

        // POST: /Store/SubmitReview
        [Authorize(Policy = "CustomerPolicy")]
        [HttpPost("SubmitReview")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview([FromForm] SubmitReviewRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid review data.";
                return RedirectToAction(nameof(ProductDetails), new { id = request.ProductId });
            }

            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue) throw new UnauthorizedAccessException();

                var canReview = await _reviewService.CanUserReviewProductAsync(userId.Value, request.ProductId);
                if (!canReview)
                {
                    TempData["Error"] = "You can only review products you have purchased and haven't reviewed yet.";
                    return RedirectToAction(nameof(ProductDetails), new { id = request.ProductId });
                }

                var customerId = await GetCustomerIdAsync();

                var review = new TafsilkPlatform.Models.Models.Review
                {
                    ProductId = request.ProductId,
                    CustomerId = customerId,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsVerifiedPurchase = true
                };

                await _reviewService.AddReviewAsync(review);
                TempData["Success"] = "Review submitted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting review for product {ProductId}", request.ProductId);
                TempData["Error"] = "An error occurred while submitting your review.";
            }

            return RedirectToAction(nameof(ProductDetails), new { id = request.ProductId });
        }

        // GET: /Store/ProductImage/{id}
        [HttpGet("ProductImage/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductImage(Guid id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (product.PrimaryImageData != null && product.PrimaryImageData.Length > 0)
            {
                return File(product.PrimaryImageData, product.PrimaryImageContentType ?? "image/jpeg");
            }

            if (!string.IsNullOrEmpty(product.PrimaryImageUrl))
            {
                // Redirect to static file if URL is provided
                return Redirect(product.PrimaryImageUrl);
            }

            return NotFound();
        }

        // GET: /Store/Cart
        [Authorize(Policy = "CustomerPolicy")]
        [HttpGet("Cart")]
        public async Task<IActionResult> Cart()
        {
            var customerId = await GetCustomerIdAsync();
            var cart = await _storeService.GetCartAsync(customerId);

            return View("~/Areas/Customer/Views/Shop/Cart.cshtml", cart ?? new CartViewModel());
        }

        [Authorize(Policy = "CustomerPolicy")]
        [HttpPost("AddToCart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart([FromForm] AddToCartRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid request: " + errors });
                }

                TempData["Error"] = "Invalid request. Please check your input.";
                return RedirectToAction(nameof(ProductDetails), new { id = request.ProductId });
            }

            try
            {
                var customerId = await GetCustomerIdAsync();
                var success = await _storeService.AddToCartAsync(customerId, request);

                if (success)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Product added to cart successfully" });
                    }

                    TempData["Success"] = "Product added to cart successfully";
                    // Stay on the same page (Product Details) instead of redirecting to Cart
                    return RedirectToAction(nameof(ProductDetails), new { id = request.ProductId });
                }

                var errorMessage = "Failed to add product to cart. Product might be unavailable or requested quantity exceeds stock.";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }

                TempData["Error"] = errorMessage;
                return RedirectToAction(nameof(ProductDetails), new { id = request.ProductId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddToCart for product {ProductId}", request.ProductId);
                var errorMsg = "An error occurred while adding the product. Please try again.";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMsg });
                }

                TempData["Error"] = errorMsg;
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
            TempData["Success"] = "Cart cleared successfully";
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
                TempData["Error"] = "Cart is empty or an error occurred while loading cart data";

                // Return a safe fallback CheckoutViewModel so the view can render and show friendly message
                var fallbackCheckout = new TafsilkPlatform.Models.ViewModels.Store.CheckoutViewModel
                {
                    Cart = new TafsilkPlatform.Models.ViewModels.Store.CartViewModel(),
                    ShippingAddress = new TafsilkPlatform.Models.ViewModels.Store.CheckoutAddressViewModel(),
                    UseSameAddressForBilling = true,
                    PaymentMethod = "CashOnDelivery"
                };

                return View("~/Areas/Customer/Views/Orders/Checkout.cshtml", fallbackCheckout);
            }

            return View("~/Areas/Customer/Views/Orders/Checkout.cshtml", checkoutData);
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
                    ? "Please complete all required fields: " + string.Join(", ", errors.Take(3))
                    : "Please complete all required fields";

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

                    var emptyCartMessage = "Cart is empty. Please add items before checking out";

                    // ✅ Check if AJAX request
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = emptyCartMessage });
                    }

                    TempData["Error"] = emptyCartMessage;
                    return RedirectToAction(nameof(Cart));
                }

                // ✅ SIMPLIFIED: Force Cash on Delivery (REMOVED)
                // request.PaymentMethod = "CashOnDelivery";

                var (success, orderIds, errorMessage) = await _storeService.ProcessCheckoutAsync(customerId, request);

                if (success && orderIds != null && orderIds.Any())
                {
                    var orderIdsString = string.Join(",", orderIds);
                    var firstOrderId = orderIds.First();

                    // ✅ CHECK FOR STRIPE PAYMENT
                    if (request.PaymentMethod == "CreditCard")
                    {
                        _logger.LogInformation("Redirecting to Stripe payment for {OrderCount} orders. IDs: {OrderIds}", orderIds.Count, orderIdsString);

                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = true, redirectUrl = $"/payments/process?orderIds={orderIdsString}" });
                        }

                        return RedirectToAction("ProcessPayment", "Payments", new { orderIds = orderIdsString });
                    }

                    _logger.LogInformation("Cash orders confirmed successfully for customer {CustomerId}. IDs: {OrderIds}", customerId, orderIdsString);

                    // ✅ NEW: Return JSON for AJAX requests
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        // Get order details for response (using first order for simplicity in summary)
                        var order = await _storeService.GetOrderDetailsAsync(firstOrderId, customerId);

                        return Json(new
                        {
                            success = true,
                            orderId = firstOrderId, // Legacy support
                            orderIds = orderIds,
                            orderNumber = firstOrderId.ToString().Substring(0, 8).ToUpper(),
                            orderDate = DateTimeOffset.UtcNow.ToString("dd/MM/yyyy - HH:mm"),
                            totalAmount = order?.TotalAmount ?? 0, // This might need to be total of all orders
                            paymentMethod = "Cash on Delivery"
                        });
                    }

                    // ✅ Traditional redirect for non-AJAX
                    TempData["OrderSuccess"] = "true";
                    TempData["OrderIds"] = orderIdsString;
                    return RedirectToAction(nameof(OrderConfirmed), new { orderIds = orderIdsString });
                }

                // ✅ Better error messages
                var errorMsg = errorMessage ?? "Order failed. Please try again or contact support.";

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

                var authMessage = "Please login first";

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

                var genericError = "An error occurred while processing the order. Please try again or contact support.";

                // ✅ Check if AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = genericError });
                }

                TempData["Error"] = genericError;
                return RedirectToAction(nameof(Checkout));
            }
        }

        // GET: /Store/OrderConfirmed?orderIds=...
        [Authorize(Policy = "CustomerPolicy")]
        [HttpGet("OrderConfirmed")]
        public async Task<IActionResult> OrderConfirmed(string? orderIds)
        {
            try
            {
                var customerId = await GetCustomerIdAsync();

                // Verify this is from a successful checkout
                var orderSuccessFlag = TempData["OrderSuccess"]?.ToString();
                if (orderSuccessFlag != "true")
                {
                    _logger.LogWarning("Order confirmed page accessed without checkout completion flag");
                }

                if (string.IsNullOrEmpty(orderIds))
                {
                    // Fallback to TempData if query param missing (e.g. redirect)
                    orderIds = TempData["OrderIds"]?.ToString();
                }

                if (string.IsNullOrEmpty(orderIds))
                {
                    return RedirectToAction("MyOrders", "Orders");
                }

                var orderIdList = orderIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                          .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
                                          .Where(g => g != Guid.Empty)
                                          .ToList();

                if (!orderIdList.Any())
                {
                    return RedirectToAction("MyOrders", "Orders");
                }

                var model = new PaymentSuccessViewModel
                {
                    // Use first order for main display properties
                    OrderId = orderIdList.First(),
                    OrderNumber = orderIdList.First().ToString().Substring(0, 8).ToUpper(),
                    PaymentMethod = "Cash on Delivery", // Default, updated below
                    OrderDate = DateTimeOffset.UtcNow,
                    EstimatedDeliveryDays = 3,
                    Orders = new List<OrderSuccessDetailsViewModel>()
                };

                decimal totalAmount = 0;

                foreach (var id in orderIdList)
                {
                    var order = await _storeService.GetOrderDetailsAsync(id, customerId);
                    if (order != null)
                    {
                        model.Orders.Add(order);
                        totalAmount += order.TotalAmount;
                        model.PaymentMethod = order.PaymentMethod; // Update with actual method
                        model.OrderDate = order.OrderDate;
                    }
                }

                model.TotalAmount = totalAmount;

                TempData.Remove("OrderSuccess");
                TempData.Remove("OrderIds");

                return View("~/Areas/Customer/Views/Orders/OrderConfirmed.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order confirmed page");
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
