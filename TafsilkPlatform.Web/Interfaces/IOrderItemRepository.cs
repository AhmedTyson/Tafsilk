using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Interfaces
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId);
        Task<decimal> GetOrderTotalAsync(Guid orderId);
        Task<bool> RemoveOrderItemsAsync(Guid orderId);
    }
}
