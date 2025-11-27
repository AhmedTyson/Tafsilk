using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Repository
{
    public class ProductRepository : EfRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10)
        {
            return await _db.Products
       .Where(p => p.IsFeatured && p.IsAvailable && !p.IsDeleted && p.StockQuantity > 0)
      .OrderByDescending(p => p.SalesCount)
 .Take(count)
        .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category, int pageNumber = 1, int pageSize = 12)
        {
            return await _db.Products
                 .Where(p => p.Category == category && p.IsAvailable && !p.IsDeleted)
               .OrderByDescending(p => p.CreatedAt)
                  .Skip((pageNumber - 1) * pageSize)
             .Take(pageSize)
            .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchQuery, int pageNumber = 1, int pageSize = 12)
        {
            return await _db.Products
          .Where(p => !p.IsDeleted && p.IsAvailable &&
               (p.Name.Contains(searchQuery) ||
               p.Description.Contains(searchQuery) ||
                      p.Category.Contains(searchQuery) ||
             (p.Brand != null && p.Brand.Contains(searchQuery))))
               .OrderByDescending(p => p.SalesCount)
              .ThenByDescending(p => p.CreatedAt)
               .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize)
            .ToListAsync();
        }

        public async Task<Product?> GetProductBySlugAsync(string slug)
        {
            return await _db.Products
            .Include(p => p.Tailor)

                   .FirstOrDefaultAsync(p => p.Slug == slug && !p.IsDeleted);
        }

        public async Task<bool> UpdateStockAsync(Guid productId, int quantity)
        {
            var product = await _db.Products.FindAsync(productId);
            if (product == null) return false;

            product.StockQuantity += quantity;
            product.UpdatedAt = DateTimeOffset.UtcNow;

            if (product.StockQuantity <= 0)
            {
                product.IsAvailable = false;
            }

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> GetRelatedProductsAsync(Guid productId, int count = 4)
        {
            var product = await _db.Products.FindAsync(productId);
            if (product == null) return Enumerable.Empty<Product>();

            return await _db.Products
       .Where(p => p.ProductId != productId &&
  p.Category == product.Category &&
   p.IsAvailable &&
                    !p.IsDeleted &&
  p.StockQuantity > 0)
      .OrderByDescending(p => p.SalesCount)
      .Take(count)
           .ToListAsync();
        }
    }
}
