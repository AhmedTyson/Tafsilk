using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Interfaces
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
   Task<ShoppingCart?> GetActiveCartByCustomerIdAsync(Guid customerId);
   Task<ShoppingCart?> GetCartWithItemsAsync(Guid cartId);
        Task<bool> ClearCartAsync(Guid cartId);
        Task<int> GetCartItemCountAsync(Guid customerId);
    }
}
