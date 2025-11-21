using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.ViewModels.Orders;
using TafsilkPlatform.Web.Services.Base;
using TafsilkPlatform.Web.Common;

namespace TafsilkPlatform.Web.Services
{
    public class OrderService : BaseService, IOrderService
    {
        private readonly AppDbContext _db;
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(
            AppDbContext db,
            IUnitOfWork unitOfWork,
            ILogger<OrderService> logger) : base(logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Order?> CreateOrderAsync(CreateOrderViewModel model, Guid userId)
        {
            var result = await ExecuteAsync(async () =>
            {
                // Validate inputs
                ValidateRequired(model, nameof(model));
                ValidateGuid(userId, nameof(userId));
                ValidateGuid(model.TailorId, nameof(model.TailorId));
                ValidateNonNegative(model.EstimatedPrice, nameof(model.EstimatedPrice));

                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var customer = await _db.CustomerProfiles
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    if (customer == null)
                    {
                        throw new InvalidOperationException("Customer profile not found. Please complete your profile first.");
                    }

                    var tailor = await _db.TailorProfiles
                        .FirstOrDefaultAsync(t => t.Id == model.TailorId);

                    if (tailor == null)
                    {
                        throw new InvalidOperationException("Tailor not found.");
                    }

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

                    return order;
                });
            }, "CreateOrder", userId);

            return result.IsSuccess ? result.Value : null;
        }

        /// <summary>
        /// ✅ IDEMPOTENCY: Create order and return OrderResult DTO
        /// This method is used by the idempotent order creation endpoint
        /// </summary>
        public async Task<OrderResult> CreateOrderWithResultAsync(CreateOrderViewModel model, Guid userId)
        {
            var result = await ExecuteAsync(async () =>
            {
                // Validate inputs
                ValidateRequired(model, nameof(model));
                ValidateGuid(userId, nameof(userId));
                ValidateGuid(model.TailorId, nameof(model.TailorId));
                ValidateNonNegative(model.EstimatedPrice, nameof(model.EstimatedPrice));

                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var customer = await _db.CustomerProfiles
                        .Include(c => c.User)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    if (customer == null)
                    {
                        throw new InvalidOperationException("Customer profile not found. Please complete your profile first.");
                    }

                    var tailor = await _db.TailorProfiles
                        .Include(t => t.User)
                        .FirstOrDefaultAsync(t => t.Id == model.TailorId);

                    if (tailor == null)
                    {
                        throw new InvalidOperationException("The selected tailor does not exist");
                    }

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
                            ItemCount = 0
                        }
                    };
                });
            }, "CreateOrderWithResult", userId);

            if (result.IsSuccess)
            {
                return result.Value!;
            }
            else
            {
                return new OrderResult
                {
                    Success = false,
                    Message = result.Error,
                    Errors = new List<string> { result.Error }
                };
            }
        }

        public async Task<List<OrderSummaryViewModel>> GetCustomerOrdersAsync(Guid userId)
        {
            var result = await ExecuteAsync(async () =>
            {
                ValidateGuid(userId, nameof(userId));

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
            }, "GetCustomerOrders", userId);

            return result.IsSuccess ? result.Value! : new List<OrderSummaryViewModel>();
        }

        public async Task<OrderDetailsViewModel?> GetOrderDetailsAsync(Guid orderId, Guid userId)
        {
            var result = await ExecuteAsync(async () =>
            {
                ValidateGuid(orderId, nameof(orderId));
                ValidateGuid(userId, nameof(userId));

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
            }, "GetOrderDetails", userId);

            return result.IsSuccess ? result.Value : null;
        }
    }
}
