using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        private async Task<Guid> GetCustomerIdAsync()
      {
       var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) throw new UnauthorizedAccessException("User not authenticated");
       
    var userId = Guid.Parse(userIdClaim);
    
       // ✅ FIX: Get CustomerProfile.Id from User.Id
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null) throw new UnauthorizedAccessException("Customer profile not found");
            
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
       if (product == null) return NotFound();
          
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
     TempData["Error"] = "Invalid request";
   return RedirectToAction(nameof(ProductDetails), new { id = request.ProductId });
     }

     var customerId = await GetCustomerIdAsync();
  var success = await _storeService.AddToCartAsync(customerId, request);

        if (success)
{
    TempData["Success"] = "Item added to cart";
  return RedirectToAction(nameof(Cart));
     }

     TempData["Error"] = "Failed to add item to cart";
     return RedirectToAction(nameof(ProductDetails), new { id = request.ProductId });
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
    TempData["Error"] = "Your cart is empty";
return RedirectToAction(nameof(Cart));
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
TempData["Error"] = "Please complete all required fields";
   return RedirectToAction(nameof(Checkout));
         }

  var customerId = await GetCustomerIdAsync();
         var (success, orderId, errorMessage) = await _storeService.ProcessCheckoutAsync(customerId, request);

if (success && orderId.HasValue)
    {
   TempData["Success"] = "Order placed successfully!";
  return RedirectToAction("OrderDetails", "Orders", new { id = orderId.Value });
 }

      TempData["Error"] = errorMessage ?? "Failed to process order";
return RedirectToAction(nameof(Checkout));
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
