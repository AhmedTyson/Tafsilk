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

     var items = cart.Items.Select(i => new CartItemViewModel
   {
      CartItemId = i.CartItemId,
            ProductId = i.ProductId,
      ProductName = i.Product.Name,
      ProductImageBase64 = i.Product.PrimaryImageData != null ? Convert.ToBase64String(i.Product.PrimaryImageData) : null,
     UnitPrice = i.UnitPrice,
      Quantity = i.Quantity,
         TotalPrice = i.TotalPrice,
     SelectedSize = i.SelectedSize,
            SelectedColor = i.SelectedColor,
                StockAvailable = i.Product.StockQuantity,
    IsAvailable = i.Product.IsAvailable
            }).ToList();

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
            try
   {
                // Get or create cart
 var cart = await _cartRepository.GetActiveCartByCustomerIdAsync(customerId);
    if (cart == null)
    {
            var customer = await _customerRepository.GetByIdAsync(customerId);
       if (customer == null) return false;

      cart = new ShoppingCart
        {
            CustomerId = customerId,
            Customer = customer,
          ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
       };
           await _cartRepository.AddAsync(cart);
       }

   // Check if item already exists
       var existingItem = await _cartItemRepository.GetCartItemAsync(
           cart.CartId, 
    request.ProductId, 
    request.SelectedSize, 
    request.SelectedColor);

    var product = await _productRepository.GetByIdAsync(request.ProductId);
         if (product == null || !product.IsAvailable) return false;

          if (existingItem != null)
          {
         // Update quantity
             existingItem.Quantity += request.Quantity;
   existingItem.UpdatedAt = DateTimeOffset.UtcNow;
       await _context.SaveChangesAsync();
      }
          else
     {
  // Add new item
  var cartItem = new CartItem
              {
               CartId = cart.CartId,
Cart = cart,
   ProductId = request.ProductId,
   Product = product,
    Quantity = request.Quantity,
       UnitPrice = product.DiscountedPrice ?? product.Price,
           SelectedSize = request.SelectedSize,
         SelectedColor = request.SelectedColor,
   SpecialInstructions = request.SpecialInstructions
       };
    await _cartItemRepository.AddAsync(cartItem);
      }

                cart.UpdatedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();
     return true;
        }
        catch (Exception ex)
            {
         _logger.LogError(ex, "Error adding item to cart for customer {CustomerId}", customerId);
         return false;
            }
        }

   public async Task<bool> UpdateCartItemAsync(Guid customerId, UpdateCartItemRequest request)
        {
   var cart = await _cartRepository.GetActiveCartByCustomerIdAsync(customerId);
      if (cart == null) return false;

            var cartItem = cart.Items.FirstOrDefault(i => i.CartItemId == request.CartItemId);
  if (cartItem == null) return false;

        if (request.Quantity <= 0)
            {
    return await RemoveFromCartAsync(customerId, request.CartItemId);
  }

     cartItem.Quantity = request.Quantity;
   cartItem.UpdatedAt = DateTimeOffset.UtcNow;
     cart.UpdatedAt = DateTimeOffset.UtcNow;
       await _context.SaveChangesAsync();
            return true;
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
    using var transaction = await _context.Database.BeginTransactionAsync();
        
            try
  {
  // Get cart
           var cart = await _cartRepository.GetActiveCartByCustomerIdAsync(customerId);
        if (cart == null || !cart.Items.Any())
   {
   return (false, null, "Cart is empty");
          }

    // Validate stock
         foreach (var item in cart.Items)
 {
           if (item.Product.StockQuantity < item.Quantity)
      {
               return (false, null, $"Insufficient stock for {item.Product.Name}");
               }
          }

           // Create order
      var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null) return (false, null, "Customer not found");

    // For store orders, we need a default tailor or system user
      var systemTailor = await _context.TailorProfiles.FirstOrDefaultAsync();
      if (systemTailor == null) return (false, null, "System configuration error");

      var subtotal = cart.Items.Sum(i => i.TotalPrice);
 var shipping = CalculateShipping(subtotal);
                var tax = CalculateTax(subtotal);
  var total = subtotal + shipping + tax;

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
        Status = OrderStatus.Confirmed, // ✅ FIXED: Store orders are auto-confirmed (no quote needed)
   CreatedAt = DateTimeOffset.UtcNow,
     DeliveryAddress = $"{request.ShippingAddress?.Street}, {request.ShippingAddress?.City}",
     FulfillmentMethod = "Delivery"
 };

 await _orderRepository.AddAsync(order);

  // Add order items and update stock
             foreach (var cartItem in cart.Items)
      {
  var orderItem = new OrderItem
        {
          OrderItemId = Guid.NewGuid(),
 OrderId = order.OrderId,
         Order = order,
       ProductId = cartItem.ProductId,
          Product = cartItem.Product,
    Description = cartItem.Product.Name,
               Quantity = cartItem.Quantity,
         UnitPrice = cartItem.UnitPrice,
              Total = cartItem.TotalPrice,
SelectedSize = cartItem.SelectedSize,
            SelectedColor = cartItem.SelectedColor,
        SpecialInstructions = cartItem.SpecialInstructions
           };

            _context.OrderItems.Add(orderItem);

            // Update stock
             cartItem.Product.StockQuantity -= cartItem.Quantity;
         cartItem.Product.SalesCount += cartItem.Quantity;
   }

       // Create payment record
    var payment = new Payment
         {
        PaymentId = Guid.NewGuid(),
         OrderId = order.OrderId,
          Order = order,
        CustomerId = customerId,
      Customer = customer,
          TailorId = systemTailor.Id,
           Tailor = systemTailor,
    Amount = total,
 PaymentType = request.PaymentMethod == "CreditCard" ? Enums.PaymentType.Card : Enums.PaymentType.Cash,
   PaymentStatus = request.PaymentMethod == "CashOnDelivery" ? Enums.PaymentStatus.Pending : Enums.PaymentStatus.Completed,
         TransactionType = Enums.TransactionType.Credit,
    PaidAt = DateTimeOffset.UtcNow
    };

  _context.Payment.Add(payment);

       // Clear cart
await _cartRepository.ClearCartAsync(cart.CartId);

  await _context.SaveChangesAsync();
  await transaction.CommitAsync();

      return (true, order.OrderId, null);
    }
 catch (Exception ex)
            {
                await transaction.RollbackAsync();
    _logger.LogError(ex, "Error processing checkout for customer {CustomerId}", customerId);
          return (false, null, "An error occurred during checkout");
  }
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
    }
}
