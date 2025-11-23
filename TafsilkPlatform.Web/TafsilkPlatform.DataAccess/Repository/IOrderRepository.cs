using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Repository
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);
        Task<IEnumerable<Order>> GetByTailorIdAsync(Guid tailorId);
        Task<IEnumerable<Order>> GetPendingOrdersAsync();
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status);
        Task<IEnumerable<Order>> GetUrgentOrdersAsync(DateTime deadline);
        Task<Order?> GetOrderWithDetailsAsync(Guid orderId);
        Task<Order?> GetOrderWithImagesAsync(Guid orderId);
        Task<Order?> GetOrderWithPaymentsAsync(Guid orderId);
        Task UpdateOrderStatusAsync(Guid orderId, string status);
        Task<int> GetOrderCountByStatusAsync(Guid tailorId, string status);
        Task<decimal> GetTotalRevenueAsync(Guid tailorId, DateTime startDate, DateTime endDate);
    }
}
