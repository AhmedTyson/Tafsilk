using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Repository
{
    public interface ICartItemRepository : IRepository<CartItem>
    {
        Task<CartItem?> GetCartItemAsync(Guid cartId, Guid productId, string? size, string? color);
   Task<IEnumerable<CartItem>> GetCartItemsByCartIdAsync(Guid cartId);
    Task<bool> RemoveCartItemAsync(Guid cartItemId);
   Task<bool> UpdateQuantityAsync(Guid cartItemId, int quantity);
    }
}
