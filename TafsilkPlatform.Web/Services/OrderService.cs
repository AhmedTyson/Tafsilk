using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.ViewModels.Orders;

namespace TafsilkPlatform.Web.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<OrderService> _logger;

        public OrderService(AppDbContext db, ILogger<OrderService> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Order?> CreateOrderAsync(CreateOrderViewModel model, Guid userId)
        {
            // basic validation
            if (model == null || model.TailorId == Guid.Empty || model.EstimatedPrice < 0)
                return null;

            var customer = await _db.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
                return null;

            var tailor = await _db.TailorProfiles
                .FirstOrDefaultAsync(t => t.Id == model.TailorId);

            if (tailor == null)
                return null;

            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerId = customer.Id,
                TailorId = model.TailorId,
                Description = model.Description,
                OrderType = model.ServiceType ?? "خدمة",
                TotalPrice = (double)model.EstimatedPrice,
                Status = OrderStatus.Pending,
                // use DateTimeOffset because Order.CreatedAt is DateTimeOffset
                CreatedAt = DateTimeOffset.UtcNow,

                // satisfy required navigation properties
                Customer = customer,
                Tailor = tailor
            };

            _db.Orders.Add(order);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch
            {
                // swallow/save behavior: return null on failure
                return null;
            }

            return order;
        }

        /// <summary>
        /// ✅ IDEMPOTENCY: Create order and return OrderResult DTO
        /// This method is used by the idempotent order creation endpoint
        /// </summary>
        public async Task<OrderResult> CreateOrderWithResultAsync(CreateOrderViewModel model, Guid userId)
        {
            try
            {
                _logger.LogInformation("[OrderService] Creating order for user {UserId}, tailor {TailorId}",
                    userId, model.TailorId);

                // Validate model
                if (model == null)
                {
                    return new OrderResult
                    {
                        Success = false,
                        Message = "Invalid order data",
                        Errors = new List<string> { "Order model cannot be null" }
                    };
                }

                if (model.TailorId == Guid.Empty)
                {
                    return new OrderResult
                    {
                        Success = false,
                        Message = "Invalid tailor ID",
                        Errors = new List<string> { "Tailor ID is required" }
                    };
                }

                if (model.EstimatedPrice < 0)
                {
                    return new OrderResult
                    {
                        Success = false,
                        Message = "Invalid price",
                        Errors = new List<string> { "Price must be greater than or equal to zero" }
                    };
                }

                // Get customer profile
                var customer = await _db.CustomerProfiles
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    _logger.LogWarning("[OrderService] Customer profile not found for user {UserId}", userId);
                    return new OrderResult
                    {
                        Success = false,
                        Message = "Customer profile not found",
                        Errors = new List<string> { "Please complete your customer profile first" }
                    };
                }

                // Get tailor profile
                var tailor = await _db.TailorProfiles
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == model.TailorId);

                if (tailor == null)
                {
                    _logger.LogWarning("[OrderService] Tailor not found: {TailorId}", model.TailorId);
                    return new OrderResult
                    {
                        Success = false,
                        Message = "Tailor not found",
                        Errors = new List<string> { "The selected tailor does not exist" }
                    };
                }

                // Create order
                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    TailorId = model.TailorId,
                    Description = model.Description,
                    OrderType = model.ServiceType ?? "خدمة",
                    TotalPrice = (double)model.EstimatedPrice,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Customer = customer,
                    Tailor = tailor
                };

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                _logger.LogInformation("[OrderService] Order created successfully: {OrderId}", order.OrderId);

                // Build success result
                return new OrderResult
                {
                    Success = true,
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderId.ToString().Substring(0, 8).ToUpper(),
                    Message = "Order created successfully",
                    Order = new OrderSummaryDto
                    {
                        OrderId = order.OrderId,
                        OrderNumber = order.OrderId.ToString().Substring(0, 8).ToUpper(),
                        OrderType = order.OrderType,
                        Status = order.Status.ToString(),
                        TotalPrice = (decimal)order.TotalPrice,
                        CreatedAt = order.CreatedAt.DateTime,
                        CustomerName = customer.FullName,
                        TailorName = tailor.ShopName ?? tailor.FullName,
                        ItemCount = 0 // Will be populated if order items exist
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OrderService] Error creating order for user {UserId}", userId);
                return new OrderResult
                {
                    Success = false,
                    Message = "An error occurred while creating the order",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<List<OrderSummaryViewModel>> GetCustomerOrdersAsync(Guid userId)
        {
            var customer = await _db.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
                return new List<OrderSummaryViewModel>();

            return await _db.Orders
                .Include(o => o.Tailor)
                .Where(o => o.CustomerId == customer.Id)
                .Select(o => new OrderSummaryViewModel
                {
                    OrderId = o.OrderId,
                    TailorName = o.Tailor.FullName,
                    ServiceType = o.OrderType,
                    Status = o.Status,
                    TotalPrice = (decimal)o.TotalPrice,
                    CreatedAt = o.CreatedAt
                }).ToListAsync();
        }

        public async Task<OrderDetailsViewModel?> GetOrderDetailsAsync(Guid orderId, Guid userId)
        {
            return await _db.Orders
                .Include(o => o.Tailor).ThenInclude(t => t.User)
                .Include(o => o.Customer).ThenInclude(c => c.User)
                .Where(o => o.OrderId == orderId && (o.Customer.UserId == userId || o.Tailor.UserId == userId))
                .Select(o => new OrderDetailsViewModel
                {
                    OrderId = o.OrderId,
                    Description = o.Description,
                    ServiceType = o.OrderType,
                    Status = o.Status,
                    TotalPrice = (decimal)o.TotalPrice,
                    CustomerName = o.Customer.User.Email,
                    TailorName = o.Tailor.User.Email,
                    CreatedAt = o.CreatedAt
                }).FirstOrDefaultAsync();
        }
    }
}
