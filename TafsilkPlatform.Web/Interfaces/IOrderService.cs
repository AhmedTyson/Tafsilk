using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.ViewModels.Orders;

namespace TafsilkPlatform.Web.Interfaces
{
    public interface IOrderService
    {
        Task<Order?> CreateOrderAsync(CreateOrderViewModel model, Guid userId);
        Task<List<OrderSummaryViewModel>> GetCustomerOrdersAsync(Guid userId);
        Task<OrderDetailsViewModel?> GetOrderDetailsAsync(Guid orderId, Guid userId);
    }
}
