using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.DataAccess.Repository;
using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Repository
{
    public class ShoppingCartRepository : EfRepository<ShoppingCart>, IShoppingCartRepository
    {
        public ShoppingCartRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ShoppingCart?> GetActiveCartByCustomerIdAsync(Guid customerId)
        {
     return await _db.ShoppingCarts
   .Include(c => c.Items)
          .ThenInclude(i => i.Product)
     .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.IsActive);
        }

      public async Task<ShoppingCart?> GetCartWithItemsAsync(Guid cartId)
   {
       return await _db.ShoppingCarts
       .Include(c => c.Items)
     .ThenInclude(i => i.Product)
        .Include(c => c.Customer)
  .ThenInclude(c => c.User)
.FirstOrDefaultAsync(c => c.CartId == cartId);
        }

   public async Task<bool> ClearCartAsync(Guid cartId)
        {
      var cart = await GetCartWithItemsAsync(cartId);
 if (cart == null) return false;

      _db.CartItems.RemoveRange(cart.Items);
   await _db.SaveChangesAsync();
    return true;
        }

        public async Task<int> GetCartItemCountAsync(Guid customerId)
   {
   var cart = await GetActiveCartByCustomerIdAsync(customerId);
            return cart?.Items.Sum(i => i.Quantity) ?? 0;
        }
    }
}
