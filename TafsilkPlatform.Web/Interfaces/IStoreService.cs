using TafsilkPlatform.Web.ViewModels.Store;

namespace TafsilkPlatform.Web.Interfaces
{
    public interface IStoreService
    {
        // Product operations
 Task<ProductListViewModel> GetProductsAsync(string? category, string? searchQuery, int pageNumber, int pageSize, string? sortBy, decimal? minPrice, decimal? maxPrice);
     Task<ProductViewModel?> GetProductDetailsAsync(Guid productId);
    Task<IEnumerable<ProductViewModel>> GetFeaturedProductsAsync(int count = 10);
      
        // Cart operations
   Task<CartViewModel?> GetCartAsync(Guid customerId);
     Task<bool> AddToCartAsync(Guid customerId, AddToCartRequest request);
        Task<bool> UpdateCartItemAsync(Guid customerId, UpdateCartItemRequest request);
        Task<bool> RemoveFromCartAsync(Guid customerId, Guid cartItemId);
        Task<bool> ClearCartAsync(Guid customerId);
   Task<int> GetCartItemCountAsync(Guid customerId);
        
        // Checkout operations
        Task<CheckoutViewModel?> GetCheckoutDataAsync(Guid customerId);
   Task<(bool Success, Guid? OrderId, string? ErrorMessage)> ProcessCheckoutAsync(Guid customerId, ProcessPaymentRequest request);
    }
}
