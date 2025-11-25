using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.DataAccess.Repository;
using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Repository
{
    public class CartItemRepository : EfRepository<CartItem>, ICartItemRepository
    {
        public CartItemRepository(ApplicationDbContext context) : base(context)
   {
      }

        public async Task<CartItem?> GetCartItemAsync(Guid cartId, Guid productId, string? size, string? color)
  {
       return await _db.CartItems
     .FirstOrDefaultAsync(ci =>
       ci.CartId == cartId &&
             ci.ProductId == productId &&
  ci.SelectedSize == size &&
         ci.SelectedColor == color);
   }

  public async Task<IEnumerable<CartItem>> GetCartItemsByCartIdAsync(Guid cartId)
        {
            return await _db.CartItems
 .Include(ci => ci.Product)
    .Where(ci => ci.CartId == cartId)
      .ToListAsync();
    }

        public async Task<bool> RemoveCartItemAsync(Guid cartItemId)
        {
    var cartItem = await _db.CartItems.FindAsync(cartItemId);
      if (cartItem == null) return false;

  _db.CartItems.Remove(cartItem);
       await _db.SaveChangesAsync();
  return true;
}

   public async Task<bool> UpdateQuantityAsync(Guid cartItemId, int quantity)
 {
      var cartItem = await _db.CartItems.FindAsync(cartItemId);
    if (cartItem == null) return false;

  cartItem.Quantity = quantity;
      cartItem.UpdatedAt = DateTimeOffset.UtcNow;
       await _db.SaveChangesAsync();
         return true;
   }
    }
}
