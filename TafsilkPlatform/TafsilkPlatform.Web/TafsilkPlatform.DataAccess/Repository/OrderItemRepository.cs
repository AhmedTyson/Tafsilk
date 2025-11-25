using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Models.Models;


namespace TafsilkPlatform.DataAccess.Repository;

public class OrderItemRepository : EfRepository<OrderItem>, IOrderItemRepository
{
    public OrderItemRepository(ApplicationDbContext db) : base(db) { }

    public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId)
        => await _db.OrderItems.Where(oi => oi.OrderId == orderId).AsNoTracking().ToListAsync();

    public async Task<decimal> GetOrderTotalAsync(Guid orderId)
        => await _db.OrderItems.Where(oi => oi.OrderId == orderId).SumAsync(oi => oi.Total);

    public async Task<bool> RemoveOrderItemsAsync(Guid orderId)
    {
        var items = await _db.OrderItems.Where(oi => oi.OrderId == orderId).ToListAsync();
        if (items.Count == 0) return false;
        _db.OrderItems.RemoveRange(items);
        return true;
    }
}
