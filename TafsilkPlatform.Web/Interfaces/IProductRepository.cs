using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category, int pageNumber = 1, int pageSize = 12);
 Task<IEnumerable<Product>> SearchProductsAsync(string searchQuery, int pageNumber = 1, int pageSize = 12);
        Task<Product?> GetProductBySlugAsync(string slug);
  Task<bool> UpdateStockAsync(Guid productId, int quantity);
     Task<IEnumerable<Product>> GetRelatedProductsAsync(Guid productId, int count = 4);
    }
}
