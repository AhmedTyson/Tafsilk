using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Core.Interfaces;
using TafsilkPlatform.Core.Models;
using TafsilkPlatform.Infrastructure.Persistence;

namespace TafsilkPlatform.Infrastructure.Repositories;

public class OrderRepository : EfRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId)
        => await _db.Orders.Where(o => o.CustomerId == customerId).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Order>> GetByTailorIdAsync(Guid tailorId)
        => await _db.Orders.Where(o => o.TailorId == tailorId).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
        => await _db.Orders.Where(o => o.Status == OrderStatus.Pending).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status)
    {
        if (!Enum.TryParse<OrderStatus>(status, true, out var parsed))
            return Enumerable.Empty<Order>();
        return await _db.Orders.Where(o => o.Status == parsed).AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetUrgentOrdersAsync(DateTime deadline)
        => await _db.Orders.Where(o => o.DueDate != null && o.DueDate <= deadline).AsNoTracking().ToListAsync();

    public Task<Order?> GetOrderWithDetailsAsync(Guid orderId)
        => _db.Orders
            .Include(o => o.Items)
            .Include(o => o.orderImages)
            .Include(o => o.Payments)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

    public Task<Order?> GetOrderWithImagesAsync(Guid orderId)
        => _db.Orders.Include(o => o.orderImages).AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == orderId);

    public Task<Order?> GetOrderWithPaymentsAsync(Guid orderId)
        => _db.Orders.Include(o => o.Payments).AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == orderId);

    public async Task UpdateOrderStatusAsync(Guid orderId, string status)
    {
        if (!Enum.TryParse<OrderStatus>(status, true, out var parsed)) return;
        var entity = await _db.Orders.FindAsync(orderId);
        if (entity is null) return;
        entity.Status = parsed;
        _db.Orders.Update(entity);
    }

    public async Task<int> GetOrderCountByStatusAsync(Guid tailorId, string status)
    {
        if (!Enum.TryParse<OrderStatus>(status, true, out var parsed)) return 0;
        return await _db.Orders.CountAsync(o => o.TailorId == tailorId && o.Status == parsed);
    }

    public async Task<decimal> GetTotalRevenueAsync(Guid tailorId, DateTime startDate, DateTime endDate)
    {
        return await _db.Payment
            .Where(p => p.TailorId == tailorId && p.PaidAt >= startDate && p.PaidAt <= endDate)
            .SumAsync(p => p.Amount);
    }
}
