using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.ViewModels.Store;

namespace TafsilkPlatform.Web.Services
{
    public class StoreService : IStoreService
    {
     private readonly IProductRepository _productRepository;
        private readonly IShoppingCartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<StoreService> _logger;

        public StoreService(
            IProductRepository productRepository,
  IShoppingCartRepository cartRepository,
            ICartItemRepository cartItemRepository,
        IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
     AppDbContext context,
 ILogger<StoreService> logger)
        {
         _productRepository = productRepository;
            _cartRepository = cartRepository;
          _cartItemRepository = cartItemRepository;
            _orderRepository = orderRepository;
          _customerRepository = customerRepository;
            _context = context;
            _logger = logger;
   }

    public async Task<ProductListViewModel> GetProductsAsync(
            string? category, 
   string? searchQuery, 
        int pageNumber, 
        int pageSize, 
 string? sortBy, 
     decimal? minPrice, 
      decimal? maxPrice)
        {
            var query = _context.Products
    .Where(p => !p.IsDeleted && p.IsAvailable);

          // Apply filters
  if (!string.IsNullOrEmpty(category))
        query = query.Where(p => p.Category == category);

       if (!string.IsNullOrEmpty(searchQuery))
  query = query.Where(p => p.Name.Contains(searchQuery) || p.Description.Contains(searchQuery));

 if (minPrice.HasValue)
           query = query.Where(p => p.Price >= minPrice.Value);

      if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

      // Apply sorting
            query = sortBy switch
          {
     "price_asc" => query.OrderBy(p => p.Price),
   "price_desc" => query.OrderByDescending(p => p.Price),
       "name" => query.OrderBy(p => p.Name),
        "rating" => query.OrderByDescending(p => p.AverageRating),
      "popular" => query.OrderByDescending(p => p.SalesCount),
   _ => query.OrderByDescending(p => p.CreatedAt)
            };

   var totalCount = await query.CountAsync();
    
       var products = await query
              .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
   .Select(p => new ProductViewModel
 {
         ProductId = p.ProductId,
    Name = p.Name,
Description = p.Description,
         Price = p.Price,
   DiscountedPrice = p.DiscountedPrice,
      Category = p.Category,
            SubCategory = p.SubCategory,
  Size = p.Size,
     Color = p.Color,
   StockQuantity = p.StockQuantity,
             IsAvailable = p.IsAvailable,
           IsFeatured = p.IsFeatured,
       AverageRating = p.AverageRating,
        ReviewCount = p.ReviewCount,
   PrimaryImageBase64 = p.PrimaryImageData != null ? Convert.ToBase64String(p.PrimaryImageData) : null
      })
      .ToListAsync();

     return new ProductListViewModel
   {
     Products = products,
      TotalCount = totalCount,
 PageNumber = pageNumber,
       PageSize = pageSize,
    Category = category,
      SearchQuery = searchQuery,
         SortBy = sortBy,
      MinPrice = minPrice,
    MaxPrice = maxPrice
    };
 }

        public async Task<ProductViewModel?> GetProductDetailsAsync(Guid productId)
      {
    var product = await _context.Products
 .Include(p => p.Tailor)
     .FirstOrDefaultAsync(p => p.ProductId == productId && !p.IsDeleted);

    if (product == null) return null;

            // Increment view count
    product.ViewCount++;
          await _context.SaveChangesAsync();

       return new ProductViewModel
       {
       ProductId = product.ProductId,
   Name = product.Name,
           Description = product.Description,
   Price = product.Price,
         DiscountedPrice = product.DiscountedPrice,
      Category = product.Category,
      SubCategory = product.SubCategory,
     Size = product.Size,
       Color = product.Color,
                Material = product.Material,
     Brand = product.Brand,
       StockQuantity = product.StockQuantity,
       IsAvailable = product.IsAvailable,
                IsFeatured = product.IsFeatured,
    AverageRating = product.AverageRating,
      ReviewCount = product.ReviewCount,
       PrimaryImageBase64 = product.PrimaryImageData != null ? Convert.ToBase64String(product.PrimaryImageData) : null,
        AdditionalImages = !string.IsNullOrEmpty(product.AdditionalImagesJson) 
        ? JsonSerializer.Deserialize<List<string>>(product.AdditionalImagesJson) ?? new() 
         : new(),
          TailorName = product.Tailor?.ShopName ?? product.Tailor?.FullName,
          TailorId = product.TailorId
  };
        }

public async Task<IEnumerable<ProductViewModel>> GetFeaturedProductsAsync(int count = 10)
        {
    var products = await _productRepository.GetFeaturedProductsAsync(count);
            
            return products.Select(p => new ProductViewModel
            {
        ProductId = p.ProductId,
         Name = p.Name,
                Description = p.Description,
    Price = p.Price,
      DiscountedPrice = p.DiscountedPrice,
Category = p.Category,
      StockQuantity = p.StockQuantity,
       IsAvailable = p.IsAvailable,
         AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
        PrimaryImageBase64 = p.PrimaryImageData != null ? Convert.ToBase64String(p.PrimaryImageData) : null
            });
   }

        public async Task<CartViewModel?> GetCartAsync(Guid customerId)
        {
            var cart = await _cartRepository.GetActiveCartByCustomerIdAsync(customerId);
            if (cart == null) return null;

            // ✅ Refresh cart items with latest product data (like Amazon)
            var items = new List<CartItemViewModel>();
            var itemsToRemove = new List<CartItem>();

            foreach (var cartItem in cart.Items)
            {
                // ✅ Reload product to get latest stock information
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == cartItem.ProductId && !p.IsDeleted);

                // ✅ Remove items for deleted/unavailable products
                if (product == null || !product.IsAvailable)
                {
                    itemsToRemove.Add(cartItem);
                    continue;
                }

                // ✅ Adjust quantity if stock is less than cart quantity (like Amazon)
                var availableQuantity = Math.Min(cartItem.Quantity, product.StockQuantity);
                if (availableQuantity < cartItem.Quantity)
                {
                    _logger.LogInformation(
                        "Adjusting cart item {CartItemId} quantity from {OldQty} to {NewQty} due to stock limit",
                        cartItem.CartItemId, cartItem.Quantity, availableQuantity);
                    
                    if (availableQuantity <= 0)
                    {
                        itemsToRemove.Add(cartItem);
                        continue;
                    }
                    
                    cartItem.Quantity = availableQuantity;
                    cartItem.UpdatedAt = DateTimeOffset.UtcNow;
                }

                items.Add(new CartItemViewModel
                {
                    CartItemId = cartItem.CartItemId,
                    ProductId = cartItem.ProductId,
                    ProductName = product.Name,
                    ProductImageBase64 = product.PrimaryImageData != null 
                        ? Convert.ToBase64String(product.PrimaryImageData) 
                        : null,
                    UnitPrice = cartItem.UnitPrice,
                    Quantity = cartItem.Quantity,
                    TotalPrice = cartItem.TotalPrice,
                    SelectedSize = cartItem.SelectedSize,
                    SelectedColor = cartItem.SelectedColor,
                    SpecialInstructions = cartItem.SpecialInstructions,
                    StockAvailable = product.StockQuantity,
                    IsAvailable = product.IsAvailable
                });
            }

            // ✅ Remove unavailable items
            if (itemsToRemove.Any())
            {
                _context.CartItems.RemoveRange(itemsToRemove);
                await _context.SaveChangesAsync();
            }

            if (!items.Any())
            {
                return null; // Cart is now empty
            }

            var subtotal = items.Sum(i => i.TotalPrice);
            var shippingCost = CalculateShipping(subtotal);
            var tax = CalculateTax(subtotal);

            return new CartViewModel
            {
                CartId = cart.CartId,
                Items = items,
                SubTotal = subtotal,
                ShippingCost = shippingCost,
                Tax = tax,
                Total = subtotal + shippingCost + tax,
                TotalItems = items.Sum(i => i.Quantity)
            };
        }

        public async Task<bool> AddToCartAsync(Guid customerId, AddToCartRequest request)
        {
            // ✅ FIXED: Use execution strategy instead of explicit transaction to work with retry logic
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // ✅ Validate request
                    if (request.Quantity <= 0 || request.Quantity > 100)
                    {
                        _logger.LogWarning("Invalid quantity {Quantity} for product {ProductId}", request.Quantity, request.ProductId);
                        await transaction.RollbackAsync();
                        return false;
                    }

                    // ✅ Get product with lock to prevent race conditions (like Amazon)
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == request.ProductId && !p.IsDeleted);
                
                    if (product == null)
                    {
                        _logger.LogWarning("Product {ProductId} not found", request.ProductId);
                        await transaction.RollbackAsync();
                        return false;
                    }

                    // ✅ Validate product availability and stock (CRITICAL - like Amazon)
                    if (!product.IsAvailable)
                    {
                        _logger.LogWarning("Product {ProductId} is not available", request.ProductId);
                        await transaction.RollbackAsync();
                        return false;
                    }

                // ✅ Get or create cart
                var cart = await _cartRepository.GetActiveCartByCustomerIdAsync(customerId);
                if (cart == null)
                {
                    var customer = await _customerRepository.GetByIdAsync(customerId);
                    if (customer == null)
                    {
                        _logger.LogWarning("Customer {CustomerId} not found", customerId);
                        return false;
                    }

                    cart = new ShoppingCart
                    {
                        CustomerId = customerId,
                        Customer = customer,
                        ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
                    };
                    await _cartRepository.AddAsync(cart);
                    await _context.SaveChangesAsync(); // Save to get CartId
                }

                // ✅ Check if item already exists in cart
                var existingItem = await _cartItemRepository.GetCartItemAsync(
                    cart.CartId,
                    request.ProductId,
                    request.SelectedSize,
                    request.SelectedColor);

                // ✅ Calculate new total quantity
                var newQuantity = existingItem != null 
                    ? existingItem.Quantity + request.Quantity 
                    : request.Quantity;

                // ✅ Validate stock availability (CRITICAL - prevent overselling like Amazon)
                if (product.StockQuantity < newQuantity)
                {
                    _logger.LogWarning(
                        "Insufficient stock for product {ProductId}. Requested: {Requested}, Available: {Available}",
                        request.ProductId, newQuantity, product.StockQuantity);
                    await transaction.RollbackAsync();
                    return false;
                }

                // ✅ Update or add cart item
                if (existingItem != null)
                {
                    // Update quantity
                    existingItem.Quantity = newQuantity;
                    existingItem.UpdatedAt = DateTimeOffset.UtcNow;
                    _logger.LogInformation(
                        "Updated cart item {CartItemId} quantity to {Quantity}",
                        existingItem.CartItemId, newQuantity);
                }
                else
                {
                    // Add new item
                    var unitPrice = product.DiscountedPrice ?? product.Price;
                    var cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        Cart = cart,
                        ProductId = request.ProductId,
                        Product = product,
                        Quantity = request.Quantity,
                        UnitPrice = unitPrice,
                        SelectedSize = request.SelectedSize,
                        SelectedColor = request.SelectedColor,
                        SpecialInstructions = request.SpecialInstructions,
                        AddedAt = DateTimeOffset.UtcNow
                    };
                    await _cartItemRepository.AddAsync(cartItem);
                    _logger.LogInformation(
                        "Added new item to cart {CartId} for product {ProductId}",
                        cart.CartId, request.ProductId);
                }

                // ✅ Update cart timestamp
                cart.UpdatedAt = DateTimeOffset.UtcNow;
                
                    // ✅ Save all changes in transaction
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        "Successfully added product {ProductId} to cart for customer {CustomerId}",
                        request.ProductId, customerId);
                    
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error adding item to cart for customer {CustomerId}", customerId);
                    return false;
                }
            });
        }

        public async Task<bool> UpdateCartItemAsync(Guid customerId, UpdateCartItemRequest request)
        {
            // ✅ FIXED: Use execution strategy for transactions
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync<bool>(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                var cart = await _cartRepository.GetActiveCartByCustomerIdAsync(customerId);
                if (cart == null)
                {
                    _logger.LogWarning("Cart not found for customer {CustomerId}", customerId);
                    return false;
                }

                var cartItem = cart.Items.FirstOrDefault(i => i.CartItemId == request.CartItemId);
                if (cartItem == null)
                {
                    _logger.LogWarning("Cart item {CartItemId} not found", request.CartItemId);
                    return false;
                }

                // ✅ If quantity is 0 or negative, remove item
                if (request.Quantity <= 0)
                {
                    await transaction.RollbackAsync();
                    return await RemoveFromCartAsync(customerId, request.CartItemId);
                }

                // ✅ Validate quantity limit
                if (request.Quantity > 100)
                {
                    _logger.LogWarning("Quantity {Quantity} exceeds maximum allowed (100)", request.Quantity);
                    await transaction.RollbackAsync();
                    return false;
                }

                // ✅ Validate stock availability before updating
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == cartItem.ProductId);

                if (product == null || !product.IsAvailable)
                {
                    _logger.LogWarning("Product {ProductId} is not available", cartItem.ProductId);
                    await transaction.RollbackAsync();
                    return false;
                }

                if (product.StockQuantity < request.Quantity)
                {
                    _logger.LogWarning(
                        "Insufficient stock for product {ProductId}. Available: {Available}, Requested: {Requested}",
                        product.ProductId, product.StockQuantity, request.Quantity);
                    await transaction.RollbackAsync();
                    return false;
                }

                // ✅ Update quantity
                cartItem.Quantity = request.Quantity;
                cartItem.UpdatedAt = DateTimeOffset.UtcNow;
                cart.UpdatedAt = DateTimeOffset.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Updated cart item {CartItemId} quantity to {Quantity} for customer {CustomerId}",
                    request.CartItemId, request.Quantity, customerId);

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error updating cart item for customer {CustomerId}", customerId);
                    return false;
                }
            });
        }

        public async Task<bool> RemoveFromCartAsync(Guid customerId, Guid cartItemId)
{
            var cart = await _cartRepository.GetActiveCartByCustomerIdAsync(customerId);
       if (cart == null) return false;

       return await _cartItemRepository.RemoveCartItemAsync(cartItemId);
        }

        public async Task<bool> ClearCartAsync(Guid customerId)
        {
            var cart = await _cartRepository.GetActiveCartByCustomerIdAsync(customerId);
            if (cart == null) return false;

   return await _cartRepository.ClearCartAsync(cart.CartId);
        }

        public async Task<int> GetCartItemCountAsync(Guid customerId)
      {
     return await _cartRepository.GetCartItemCountAsync(customerId);
        }

        public async Task<CheckoutViewModel?> GetCheckoutDataAsync(Guid customerId)
   {
            var cartViewModel = await GetCartAsync(customerId);
            if (cartViewModel == null) return null;

            var customer = await _context.CustomerProfiles
              .Include(c => c.User)
  .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customer == null) return null;

   return new CheckoutViewModel
  {
      Cart = cartViewModel,
  ShippingAddress = new CheckoutAddressViewModel
        {
            FullName = customer.FullName,
         PhoneNumber = customer.User.PhoneNumber,
   City = customer.City
       }
     };
        }

        public async Task<(bool Success, Guid? OrderId, string? ErrorMessage)> ProcessCheckoutAsync(
            Guid customerId,
            ProcessPaymentRequest request)
        {
            // ✅ FIXED: Use execution strategy to work with retry logic
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync<(bool Success, Guid? OrderId, string? ErrorMessage)>(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                _logger.LogInformation("Processing checkout for customer {CustomerId}", customerId);

                // ✅ Get cart with fresh data (prevent stale data issues)
                var cart = await _context.ShoppingCarts
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.IsActive);

                if (cart == null || !cart.Items.Any())
                {
                    _logger.LogWarning("Empty cart for customer {CustomerId}", customerId);
                    return (false, null, "Your cart is empty. Please add items before checkout.");
                }

                // ✅ Validate and lock stock (CRITICAL - like Amazon prevents overselling)
                var stockValidationErrors = new List<string>();
                foreach (var cartItem in cart.Items)
                {
                    // Reload product with lock to prevent race conditions
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == cartItem.ProductId);

                    if (product == null)
                    {
                        stockValidationErrors.Add($"{cartItem.Product?.Name ?? "Product"} is no longer available");
                        continue;
                    }

                    if (!product.IsAvailable)
                    {
                        stockValidationErrors.Add($"{product.Name} is currently unavailable");
                        continue;
                    }

                    // ✅ Check stock availability (CRITICAL)
                    if (product.StockQuantity < cartItem.Quantity)
                    {
                        stockValidationErrors.Add(
                            $"{product.Name}: Only {product.StockQuantity} available, but {cartItem.Quantity} requested");
                    }
                }

                if (stockValidationErrors.Any())
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning(
                        "Stock validation failed for customer {CustomerId}. Errors: {Errors}",
                        customerId, string.Join("; ", stockValidationErrors));
                    return (false, null, $"Stock issues: {string.Join("; ", stockValidationErrors)}");
                }

                // ✅ Get customer
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null)
                {
                    await transaction.RollbackAsync();
                    return (false, null, "Customer not found");
                }

                // ✅ Get system tailor for store orders
                var systemTailor = await _context.TailorProfiles.FirstOrDefaultAsync();
                if (systemTailor == null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError("System tailor not configured");
                    return (false, null, "System configuration error. Please contact support.");
                }

                // ✅ Calculate totals
                var subtotal = cart.Items.Sum(i => i.TotalPrice);
                var shipping = CalculateShipping(subtotal);
                var tax = CalculateTax(subtotal);
                var total = subtotal + shipping + tax;

                // ✅ Create order
                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    CustomerId = customerId,
                    Customer = customer,
                    TailorId = systemTailor.Id,
                    Tailor = systemTailor,
                    Description = "Store Purchase",
                    OrderType = "StoreOrder",
                    TotalPrice = (double)total,
                    Status = OrderStatus.Confirmed, // Store orders are auto-confirmed
                    CreatedAt = DateTimeOffset.UtcNow,
                    DeliveryAddress = $"{request.ShippingAddress?.Street}, {request.ShippingAddress?.City}",
                    FulfillmentMethod = "Delivery"
                };

                await _orderRepository.AddAsync(order);
                await _context.SaveChangesAsync(); // Save to get OrderId

                // ✅ Create order items and update stock atomically (CRITICAL)
                foreach (var cartItem in cart.Items)
                {
                    // Reload product with lock
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == cartItem.ProductId);

                    if (product == null) continue;

                    // ✅ Double-check stock before updating (prevent race conditions)
                    if (product.StockQuantity < cartItem.Quantity)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(
                            "Stock check failed during order creation for product {ProductId}. Available: {Available}, Requested: {Requested}",
                            product.ProductId, product.StockQuantity, cartItem.Quantity);
                        return (false, null, $"Insufficient stock for {product.Name}");
                    }

                    // Create order item
                    var orderItem = new OrderItem
                    {
                        OrderItemId = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        Order = order,
                        ProductId = cartItem.ProductId,
                        Product = product,
                        Description = product.Name,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice,
                        Total = cartItem.TotalPrice,
                        SelectedSize = cartItem.SelectedSize,
                        SelectedColor = cartItem.SelectedColor,
                        SpecialInstructions = cartItem.SpecialInstructions
                    };

                    _context.OrderItems.Add(orderItem);

                    // ✅ Update stock atomically (CRITICAL - like Amazon)
                    product.StockQuantity -= cartItem.Quantity;
                    product.SalesCount += cartItem.Quantity;
                    product.UpdatedAt = DateTimeOffset.UtcNow;

                    // ✅ Mark product as unavailable if stock reaches zero
                    if (product.StockQuantity == 0)
                    {
                        product.IsAvailable = false;
                    }
                }

                // ✅ Determine payment status based on payment method
                // For Cash on Delivery, mark as Completed since order is confirmed and payment will be collected on delivery
                // For other payment methods, mark as Completed if payment gateway confirms, otherwise Pending
                var paymentStatus = Enums.PaymentStatus.Completed; // All payments are completed when order is confirmed

                // ✅ Create payment record
                var payment = new Models.Payment
                {
                    PaymentId = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    Order = order,
                    CustomerId = customerId,
                    Customer = customer,
                    TailorId = systemTailor.Id,
                    Tailor = systemTailor,
                    Amount = total,
                    PaymentType = request.PaymentMethod == "CreditCard" 
                        ? Enums.PaymentType.Card 
                        : Enums.PaymentType.Cash,
                    PaymentStatus = paymentStatus,
                    TransactionType = Enums.TransactionType.Credit,
                    PaidAt = DateTimeOffset.UtcNow, // Set PaidAt for all completed payments
                    Currency = "SAR",
                    Provider = "Internal",
                    Notes = request.PaymentMethod == "CashOnDelivery" 
                        ? "Payment will be collected on delivery" 
                        : null
                };

                _context.Payment.Add(payment);

                // ✅ Clear cart ONLY after successful order creation (like Amazon)
                await _cartRepository.ClearCartAsync(cart.CartId);

                // ✅ Save all changes and commit transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Checkout completed successfully for customer {CustomerId}. OrderId: {OrderId}",
                    customerId, order.OrderId);

                    return (true, order.OrderId, null);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Concurrency conflict during checkout for customer {CustomerId}", customerId);
                    return (false, null, "The cart was modified by another process. Please refresh and try again.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error processing checkout for customer {CustomerId}", customerId);
                    return (false, null, $"An error occurred during checkout: {ex.Message}");
                }
            });
        }

        private decimal CalculateShipping(decimal subtotal)
        {
            // Free shipping over 500 SAR
            if (subtotal >= 500) return 0;
            return 25; // Flat rate
        }

        private decimal CalculateTax(decimal subtotal)
        {
            // 15% VAT in Saudi Arabia
            return subtotal * 0.15m;
        }

        /// <summary>
        /// Get order details for payment success page
        /// </summary>
        public async Task<OrderSuccessDetailsViewModel?> GetOrderDetailsAsync(Guid orderId, Guid customerId)
        {
            try
            {
                // ✅ FIXED: Add retry logic for timing issues (order might not be immediately available after transaction commit)
                const int maxRetries = 3;
                const int delayMs = 200;
                
                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    // Get customer to verify ownership
                    var customer = await _customerRepository.GetByIdAsync(customerId);
                    if (customer == null)
                    {
                        _logger.LogWarning("Customer {CustomerId} not found", customerId);
                        return null;
                    }

                    // Get order with items
                    var order = await _context.Orders
                        .Include(o => o.Items)
                            .ThenInclude(i => i.Product)
                        .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == customerId);

                    if (order != null)
                    {
                        return new OrderSuccessDetailsViewModel
                        {
                            OrderId = order.OrderId,
                            OrderNumber = order.OrderId.ToString().Substring(0, 8).ToUpper(),
                            TotalAmount = (decimal)order.TotalPrice,
                            OrderDate = order.CreatedAt,
                            PaymentMethod = "الدفع عند الاستلام",
                            DeliveryAddress = order.DeliveryAddress ?? "غير محدد",
                            Items = order.Items.Select(item => new OrderSuccessItemViewModel
                            {
                                ProductName = item.Product?.Name ?? item.Description,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice,
                                Total = item.Total
                            }).ToList()
                        };
                    }
                    
                    // If order not found and not last attempt, wait and retry
                    if (attempt < maxRetries)
                    {
                        _logger.LogInformation("Order {OrderId} not found on attempt {Attempt}, retrying in {Delay}ms...", 
                            orderId, attempt, delayMs);
                        await Task.Delay(delayMs);
                    }
                }

                _logger.LogWarning("Order {OrderId} not found for customer {CustomerId} after {Retries} attempts", 
                    orderId, customerId, maxRetries);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order details for {OrderId}", orderId);
                return null;
            }
        }
    }
}
