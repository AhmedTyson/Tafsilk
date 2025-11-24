using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels.Orders;

namespace TafsilkPlatform.Web.Interfaces
{
    public interface IOrderService
    {
        Task<Order?> CreateOrderAsync(CreateOrderViewModel model, Guid userId);
        Task<OrderResult> CreateOrderWithResultAsync(CreateOrderViewModel model, Guid userId);
        Task<List<OrderSummaryViewModel>> GetCustomerOrdersAsync(Guid userId);
        Task<OrderDetailsViewModel?> GetOrderDetailsAsync(Guid orderId, Guid userId);
    }
}
