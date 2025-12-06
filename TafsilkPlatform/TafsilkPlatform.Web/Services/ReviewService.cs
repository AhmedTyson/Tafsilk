using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Web.Services.Interfaces;

namespace TafsilkPlatform.Web.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(ApplicationDbContext context, ILogger<ReviewService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> CanUserReviewProductAsync(Guid userId, Guid productId)
        {
            // 1. Check if user has purchased the product
            var hasPurchased = await _context.Orders
                .Where(o => o.Customer.UserId == userId && o.Status == OrderStatus.Delivered)
                .SelectMany(o => o.Items)
                .AnyAsync(i => i.ProductId == productId);

            if (!hasPurchased) return false;

            // 2. Check if user has already reviewed this product
            var hasReviewed = await _context.Reviews
                .AnyAsync(r => r.Customer.UserId == userId && r.ProductId == productId);

            return !hasReviewed;
        }

        public async Task AddReviewAsync(Review review)
        {
            // Add review
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Update Product Stats
            await UpdateProductRatingAsync(review.ProductId);

            // Update Tailor Stats
            var product = await _context.Products.FindAsync(review.ProductId);
            if (product != null && product.TailorId.HasValue)
            {
                await UpdateTailorRatingAsync(product.TailorId.Value);
            }
        }

        public async Task<List<Review>> GetProductReviewsAsync(Guid productId)
        {
            return await _context.Reviews
                .Include(r => r.Customer)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Review>> GetTailorReviewsAsync(Guid tailorId, int count = 5)
        {
            // Get reviews for all products belonging to this tailor
            return await _context.Reviews
                .Include(r => r.Product)
                .Include(r => r.Customer)
                .Where(r => r.Product.TailorId == tailorId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        private async Task UpdateProductRatingAsync(Guid productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return;

            var stats = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .GroupBy(r => r.ProductId)
                .Select(g => new { Average = g.Average(r => r.Rating), Count = g.Count() })
                .FirstOrDefaultAsync();

            if (stats != null)
            {
                product.AverageRating = stats.Average;
                product.ReviewCount = stats.Count;
            }
            else
            {
                product.AverageRating = 0;
                product.ReviewCount = 0;
            }

            await _context.SaveChangesAsync();
        }

        private async Task UpdateTailorRatingAsync(Guid tailorId)
        {
            var tailor = await _context.TailorProfiles.FindAsync(tailorId);
            if (tailor == null) return;

            // Calculate average rating from all reviews on their products
            var averageRating = await _context.Reviews
                .Where(r => r.Product.TailorId == tailorId)
                .AverageAsync(r => (double?)r.Rating) ?? 0;

            tailor.AverageRating = (decimal)averageRating;
            await _context.SaveChangesAsync();
        }
    }
}
