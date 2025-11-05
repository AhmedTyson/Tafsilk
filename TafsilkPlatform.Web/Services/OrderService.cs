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

        public OrderService(AppDbContext db)
        {
            _db = db;
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
