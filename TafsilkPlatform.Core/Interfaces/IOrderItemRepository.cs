using TafsilkPlatform.Core.Models;

namespace TafsilkPlatform.Core.Interfaces
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId);
        Task<decimal> GetOrderTotalAsync(Guid orderId);
        Task<bool> RemoveOrderItemsAsync(Guid orderId);
    }
}
