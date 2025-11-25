using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Repository
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId);
        Task<decimal> GetOrderTotalAsync(Guid orderId);
        Task<bool> RemoveOrderItemsAsync(Guid orderId);
    }
}
