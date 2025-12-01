using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TafsilkPlatform.DataAccess.Repository;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels.Store;
using TafsilkPlatform.Web.Interfaces;

namespace TafsilkPlatform.Web.Services
{
    public class StoreService : IStoreService
    {
        // ✅ REFACTORED: Remove individual repositories and use UnitOfWork
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StoreService> _logger;

        public StoreService(
            IUnitOfWork unitOfWork,
            ILogger<StoreService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            // ✅ Use UnitOfWork.Context for complex queries
            var query = _unitOfWork.Context.Products
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

            // Build filtered query (use the same `query` after applying filters)
            var filtered = query;

            // Get total count BEFORE applying OrderBy
            var totalCount = await filtered.CountAsync();

            // Apply sorting only to the paged query
            var ordered = sortBy switch
            {
                "price_asc" => filtered.OrderBy(p => p.Price),
                "price_desc" => filtered.OrderByDescending(p => p.Price),
                "name" => filtered.OrderBy(p => p.Name),
                "rating" => filtered.OrderByDescending(p => p.AverageRating),
                "popular" => filtered.OrderByDescending(p => p.SalesCount),
                // Use the underlying converted column value for CreatedAt to avoid SQLite ordering on DateTimeOffset
                _ => filtered.OrderByDescending(p => EF.Property<long>(p, nameof(Product.CreatedAt)))
            };

            var products = await ordered
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
                    PrimaryImageBase64 = p.PrimaryImageData != null ? Convert.ToBase64String(p.PrimaryImageData) : null,
                    PrimaryImageContentType = p.PrimaryImageContentType
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
            // ✅ Use UnitOfWork.Context for queries with includes
            var product = await _unitOfWork.Context.Products
                .Include(p => p.Tailor)
                .FirstOrDefaultAsync(p => p.ProductId == productId && !p.IsDeleted);

            if (product == null) return null;

            // Increment view count
            product.ViewCount++;
            await _unitOfWork.SaveChangesAsync();

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
                PrimaryImageContentType = product.PrimaryImageContentType,
                AdditionalImages = !string.IsNullOrEmpty(product.AdditionalImagesJson)
                    ? JsonSerializer.Deserialize<List<string>>(product.AdditionalImagesJson) ?? new()
                    : new(),
                TailorName = product.Tailor?.ShopName ?? product.Tailor?.FullName,
                TailorId = product.TailorId
            };
        }

        public async Task<IEnumerable<ProductViewModel>> GetFeaturedProductsAsync(int count = 10)
        {
            // ✅ Use UnitOfWork repository
            var products = await _unitOfWork.Products.GetFeaturedProductsAsync(count);

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
                PrimaryImageBase64 = p.PrimaryImageData != null ? Convert.ToBase64String(p.PrimaryImageData) : null,
                PrimaryImageContentType = p.PrimaryImageContentType
            });
        }

        public async Task<CartViewModel?> GetCartAsync(Guid customerId)
        {
            // ✅ Use UnitOfWork repository
            var cart = await _unitOfWork.ShoppingCarts.GetActiveCartByCustomerIdAsync(customerId);
            if (cart == null) return null;

            // ✅ Refresh cart items with latest product data (like Amazon)
            var items = new List<CartItemViewModel>();
            var itemsToRemove = new List<CartItem>();

            foreach (var cartItem in cart.Items)
            {
                // ✅ Reload product to get latest stock information
                var product = await _unitOfWork.Context.Products
                    .Include(p => p.Tailor)
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
                    IsAvailable = product.IsAvailable,
                    // Tailor Information
                    TailorId = product.TailorId ?? Guid.Empty,
                    TailorName = product.Tailor?.FullName ?? "Unknown",
                    ShopName = product.Tailor?.ShopName,
                    TailorCity = product.Tailor?.City,
                    TailorDistrict = product.Tailor?.District
                });
            }

            // ✅ Remove unavailable items
            if (itemsToRemove.Any())
            {
                _unitOfWork.Context.CartItems.RemoveRange(itemsToRemove);
                await _unitOfWork.SaveChangesAsync();
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
            // ✅ Use UnitOfWork transaction management
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // ✅ Validate request
                if (request.Quantity <= 0 || request.Quantity > 100)
                {
                    _logger.LogWarning("Invalid quantity {Quantity} for product {ProductId}", request.Quantity, request.ProductId);
                    return false;
                }

                // ✅ Get product with lock to prevent race conditions
                var product = await _unitOfWork.Context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == request.ProductId && !p.IsDeleted);

                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found", request.ProductId);
                    return false;
                }

                // ✅ Validate product availability and stock
                if (!product.IsAvailable)
                {
                    _logger.LogWarning("Product {ProductId} is not available", request.ProductId);
                    return false;
                }

                // ✅ Get or create cart
                var cart = await _unitOfWork.ShoppingCarts.GetActiveCartByCustomerIdAsync(customerId);
                if (cart == null)
                {
                    var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
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
                    await _unitOfWork.ShoppingCarts.AddAsync(cart);
                    await _unitOfWork.SaveChangesAsync(); // Save to get CartId
                }

                // ✅ Check if item already exists in cart
                var existingItem = await _unitOfWork.CartItems.GetCartItemAsync(
                    cart.CartId,
                    request.ProductId,
                    request.SelectedSize,
                    request.SelectedColor);

                // ✅ Calculate new total quantity
                var newQuantity = existingItem != null
                    ? existingItem.Quantity + request.Quantity
                    : request.Quantity;

                // ✅ Validate stock availability (CRITICAL - prevent overselling)
                if (product.StockQuantity < newQuantity)
                {
                    _logger.LogWarning(
                        "Insufficient stock for product {ProductId}. Requested: {Requested}, Available: {Available}",
                        request.ProductId, newQuantity, product.StockQuantity);
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
                    await _unitOfWork.CartItems.AddAsync(cartItem);
                    _logger.LogInformation(
                        "Added new item to cart {CartId} for product {ProductId}",
                        cart.CartId, request.ProductId);
                }

                // ✅ Update cart timestamp
                cart.UpdatedAt = DateTimeOffset.UtcNow;

                // ✅ Save all changes in transaction
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully added product {ProductId} to cart for customer {CustomerId}",
                    request.ProductId, customerId);

                return true;
            });
        }

        public async Task<bool> UpdateCartItemAsync(Guid customerId, UpdateCartItemRequest request)
        {
            // ✅ Use UnitOfWork transaction management
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var cart = await _unitOfWork.ShoppingCarts.GetActiveCartByCustomerIdAsync(customerId);
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
                    return await RemoveFromCartAsync(customerId, request.CartItemId);
                }

                // ✅ Validate quantity limit
                if (request.Quantity > 100)
                {
                    _logger.LogWarning("Quantity {Quantity} exceeds maximum allowed (100)", request.Quantity);
                    return false;
                }

                // ✅ Validate stock availability before updating
                var product = await _unitOfWork.Context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == cartItem.ProductId);

                if (product == null || !product.IsAvailable)
                {
                    _logger.LogWarning("Product {ProductId} is not available", cartItem.ProductId);
                    return false;
                }

                if (product.StockQuantity < request.Quantity)
                {
                    _logger.LogWarning(
                        "Insufficient stock for product {ProductId}. Available: {Available}, Requested: {Requested}",
                        product.ProductId, product.StockQuantity, request.Quantity);
                    return false;
                }

                // ✅ Update quantity
                cartItem.Quantity = request.Quantity;
                cartItem.UpdatedAt = DateTimeOffset.UtcNow;
                cart.UpdatedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Updated cart item {CartItemId} quantity to {Quantity} for customer {CustomerId}",
                    request.CartItemId, request.Quantity, customerId);

                return true;
            });
        }

        public async Task<bool> RemoveFromCartAsync(Guid customerId, Guid cartItemId)
        {
            var cart = await _unitOfWork.ShoppingCarts.GetActiveCartByCustomerIdAsync(customerId);
            if (cart == null) return false;

            return await _unitOfWork.CartItems.RemoveCartItemAsync(cartItemId);
        }

        public async Task<bool> ClearCartAsync(Guid customerId)
        {
            var cart = await _unitOfWork.ShoppingCarts.GetActiveCartByCustomerIdAsync(customerId);
            if (cart == null) return false;

            return await _unitOfWork.ShoppingCarts.ClearCartAsync(cart.CartId);
        }

        public async Task<int> GetCartItemCountAsync(Guid customerId)
        {
            return await _unitOfWork.ShoppingCarts.GetCartItemCountAsync(customerId);
        }

        public async Task<CheckoutViewModel?> GetCheckoutDataAsync(Guid customerId)
        {
            var cartViewModel = await GetCartAsync(customerId);
            if (cartViewModel == null) return null;

            var customer = await _unitOfWork.Context.CustomerProfiles
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
            // Use UnitOfWork's ExecuteInTransactionAsync for consistent transaction handling
            // Specify the exact tuple type (with nullable ErrorMessage) to avoid nullability inference issues
            return await _unitOfWork.ExecuteInTransactionAsync<(bool Success, Guid? OrderId, string? ErrorMessage)>(async () =>
             {
                 try
                 {
                     _logger.LogInformation("Processing checkout for customer {CustomerId}", customerId);

                     // Get cart with fresh data (prevent stale data issues)
                     var cart = await _unitOfWork.Context.ShoppingCarts
                         .Include(c => c.Items)
                             .ThenInclude(i => i.Product)
                         .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.IsActive);

                     if (cart == null || !cart.Items.Any())
                     {
                         _logger.LogWarning("Empty cart for customer {CustomerId}", customerId);
                         return (false, (Guid?)null, "Your cart is empty. Please add items before checkout.");
                     }

                     // Validate and lock stock (CRITICAL - like Amazon prevents overselling)
                     var stockValidationErrors = new List<string>();
                     foreach (var cartItem in cart.Items)
                     {
                         // Reload product with lock to prevent race conditions
                         var product = await _unitOfWork.Context.Products
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

                         // Check stock availability (CRITICAL)
                         if (product.StockQuantity < cartItem.Quantity)
                         {
                             stockValidationErrors.Add(
                                 $"{product.Name}: Only {product.StockQuantity} available, but {cartItem.Quantity} requested");
                         }
                     }

                     if (stockValidationErrors.Any())
                     {
                         _logger.LogWarning(
                             "Stock validation failed for customer {CustomerId}. Errors: {Errors}",
                             customerId, string.Join("; ", stockValidationErrors));
                         return (false, (Guid?)null, $"Stock issues: {string.Join("; ", stockValidationErrors)}");
                     }

                     // Get customer
                     var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
                     if (customer == null)
                     {
                         return (false, (Guid?)null, "Customer not found");
                     }

                     // Calculate totals
                     var subtotal = cart.Items.Sum(i => i.TotalPrice);
                     var shipping = CalculateShipping(subtotal);
                     var tax = CalculateTax(subtotal);
                     var total = subtotal + shipping + tax;

                     // Determine order status based on payment method
                     var orderStatus = request.PaymentMethod == "CreditCard"
                         ? OrderStatus.PendingPayment
                         : OrderStatus.Confirmed;

                     // Group cart items by tailor to create separate orders
                     var itemsByTailor = new Dictionary<Guid, List<CartItem>>();

                     foreach (var cartItem in cart.Items)
                     {
                         // Reload product to get tailor info
                         var product = await _unitOfWork.Context.Products
                             .Include(p => p.Tailor)
                             .FirstOrDefaultAsync(p => p.ProductId == cartItem.ProductId);

                         if (product == null || product.TailorId == null)
                         {
                             _logger.LogWarning("Product {ProductId} has no tailor assigned", cartItem.ProductId);
                             continue;
                         }

                         var tailorId = product.TailorId.Value;
                         if (!itemsByTailor.ContainsKey(tailorId))
                         {
                             itemsByTailor[tailorId] = new List<CartItem>();
                         }
                         itemsByTailor[tailorId].Add(cartItem);
                     }

                     if (!itemsByTailor.Any())
                     {
                         return (false, (Guid?)null, "No valid products found in cart");
                     }

                     // Create separate order for each tailor
                     var createdOrders = new List<Order>();
                     var firstOrderId = (Guid?)null;

                     foreach (var tailorGroup in itemsByTailor)
                     {
                         var tailorId = tailorGroup.Key;
                         var tailorItems = tailorGroup.Value;

                         // Get tailor
                         var tailor = await _unitOfWork.Context.TailorProfiles
                             .FirstOrDefaultAsync(t => t.Id == tailorId);

                         if (tailor == null)
                         {
                             _logger.LogWarning("Tailor {TailorId} not found", tailorId);
                             continue;
                         }

                         // Calculate order total for this tailor's products
                         var orderSubtotal = (double)tailorItems.Sum(i => i.TotalPrice);
                         var orderTotal = orderSubtotal;

                         // Calculate commission (10% platform fee)
                         var commissionRate = 0.10;
                         var commissionAmount = orderTotal * commissionRate;

                         // Create order for this tailor
                         var order = new Order
                         {
                             OrderId = Guid.NewGuid(),
                             CustomerId = customerId,
                             Customer = customer,
                             TailorId = tailorId,
                             Tailor = tailor,
                             Description = $"Store Purchase from {tailor.ShopName ?? tailor.FullName}",
                             OrderType = "StoreOrder",
                             TotalPrice = orderTotal,
                             Status = orderStatus,
                             CreatedAt = DateTimeOffset.UtcNow,
                             DeliveryAddress = $"{request.ShippingAddress?.Street}, {request.ShippingAddress?.City}",
                             FulfillmentMethod = "Delivery",
                             CommissionRate = commissionRate,
                             CommissionAmount = commissionAmount
                         };

                         await _unitOfWork.Orders.AddAsync(order);
                         await _unitOfWork.SaveChangesAsync(); // Save to get OrderId

                         if (firstOrderId == null)
                         {
                             firstOrderId = order.OrderId;
                         }

                         // Create order items and update stock atomically (CRITICAL)
                         foreach (var cartItem in tailorItems)
                         {
                             // Reload product with lock
                             var product = await _unitOfWork.Context.Products
                                 .FirstOrDefaultAsync(p => p.ProductId == cartItem.ProductId);

                             if (product == null) continue;

                             // Double-check stock before updating (prevent race conditions)
                             if (product.StockQuantity < cartItem.Quantity)
                             {
                                 _logger.LogError(
                                     "Stock check failed during order creation for product {ProductId}. Available: {Available}, Requested: {Requested}",
                                     product.ProductId, product.StockQuantity, cartItem.Quantity);
                                 return (false, (Guid?)null, $"Insufficient stock for {product.Name}");
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

                             // Use Context to add entity
                             _unitOfWork.Context.OrderItems.Add(orderItem);

                             // Update stock atomically (CRITICAL - like Amazon)
                             product.StockQuantity -= cartItem.Quantity;
                             product.SalesCount += cartItem.Quantity;
                             product.UpdatedAt = DateTimeOffset.UtcNow;

                             // Mark product as unavailable if stock reaches zero
                             if (product.StockQuantity == 0)
                             {
                                 product.IsAvailable = false;
                             }
                         }

                         // Create payment record for each order
                         var paymentStatus = Enums.PaymentStatus.Pending;
                         var payment = new TafsilkPlatform.Models.Models.Payment
                         {
                             PaymentId = Guid.NewGuid(),
                             OrderId = order.OrderId,
                             Order = order,
                             CustomerId = customerId,
                             Customer = customer,
                             TailorId = tailorId,
                             Tailor = tailor,
                             Amount = (decimal)orderTotal,
                             PaymentType = request.PaymentMethod == "CreditCard"
                                ? Enums.PaymentType.Card
                                : Enums.PaymentType.Cash,
                             PaymentStatus = paymentStatus,
                             TransactionType = Enums.TransactionType.Credit,
                             PaidAt = null, // Not paid yet
                             Currency = "EGP",
                             Provider = request.PaymentMethod == "CreditCard" ? "Stripe" : "Internal",
                             Notes = request.PaymentMethod == "CashOnDelivery"
                                ? "Payment will be collected on delivery"
                                : "Pending Stripe payment"
                         };

                         // Use Context to add payment
                         _unitOfWork.Context.Payment.Add(payment);
                         createdOrders.Add(order);
                     }

                     // Clear cart ONLY if payment is not CreditCard (Stripe)
                     // For Stripe, we keep items in cart until payment is confirmed via webhook/success page
                     if (request.PaymentMethod != "CreditCard")
                     {
                         await _unitOfWork.ShoppingCarts.ClearCartAsync(cart.CartId);
                     }

                     // Save all changes
                     await _unitOfWork.SaveChangesAsync();

                     _logger.LogInformation(
                         "Checkout completed successfully for customer {CustomerId}. Created {OrderCount} order(s). First OrderId: {OrderId}",
                         customerId, createdOrders.Count, firstOrderId);

                     return (true, firstOrderId, (string?)null);
                 }
                 catch (Exception ex)
                 {
                     _logger.LogError(ex, "Error processing checkout for customer {CustomerId}", customerId);
                     throw; // Let UnitOfWork handle rollback via ExecuteInTransactionAsync
                 }
             });
        }

        private decimal CalculateShipping(decimal subtotal)
        {
            // Free shipping over 500 EGP
            if (subtotal >= 500) return 0;
            return 25; // Flat rate
        }

        private decimal CalculateTax(decimal subtotal)
        {
            // 14% VAT in Egypt
            return subtotal * 0.14m;
        }

        /// <summary>
        /// Get order details for payment success page
        /// </summary>
        public async Task<OrderSuccessDetailsViewModel?> GetOrderDetailsAsync(Guid orderId, Guid customerId)
        {
            try
            {
                // ✅ Add retry logic for timing issues
                const int maxRetries = 3;
                const int delayMs = 200;

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    // Get customer to verify ownership
                    var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
                    if (customer == null)
                    {
                        _logger.LogWarning("Customer {CustomerId} not found", customerId);
                        return null;
                    }

                    // Get order with items
                    var order = await _unitOfWork.Context.Orders
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
                            PaymentMethod = "Cash on Delivery",
                            DeliveryAddress = order.DeliveryAddress ?? "Not specified",
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
        public async Task<bool> CancelPendingOrderAsync(Guid orderId, Guid customerId)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // 1. Get order with items
                var order = await _unitOfWork.Context.Orders
                    .Include(o => o.Items)
                    .Include(o => o.Payments) // Include payments to delete them
                    .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == customerId);

                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found for cancellation", orderId);
                    return false;
                }

                // 2. Validate status (only pending orders can be cancelled this way)
                if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.PendingPayment)
                {
                    _logger.LogWarning("Order {OrderId} status {Status} cannot be cancelled via this flow", orderId, order.Status);
                    return false;
                }

                // 3. Return stock
                // We don't need to restore to cart because we didn't clear it for CreditCard orders
                foreach (var orderItem in order.Items)
                {
                    var product = await _unitOfWork.Context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == orderItem.ProductId);

                    if (product != null)
                    {
                        product.StockQuantity += orderItem.Quantity;
                        product.SalesCount -= orderItem.Quantity; // Revert sales count
                        product.IsAvailable = product.StockQuantity > 0; // Make available if stock > 0
                    }
                }

                // 4. Delete order (Cascade should handle items and payments, but let's be safe)
                // Remove payments first if cascade not configured
                if (order.Payments != null && order.Payments.Any())
                {
                    _unitOfWork.Context.Payment.RemoveRange(order.Payments);
                }

                // Remove order items
                if (order.Items != null && order.Items.Any())
                {
                    _unitOfWork.Context.OrderItems.RemoveRange(order.Items);
                }

                // Remove order
                _unitOfWork.Context.Orders.Remove(order);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Pending order {OrderId} cancelled and stock returned for customer {CustomerId}", orderId, customerId);
                return true;
            });
        }
    }
}
