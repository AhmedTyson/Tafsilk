using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Data;

namespace TafsilkPlatform.Web.Repositories;

public class ReviewRepository : EfRepository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Review>> GetByTailorIdAsync(Guid tailorId)
        => await _db.Reviews.Where(r => r.TailorId == tailorId).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Review>> GetByCustomerIdAsync(Guid customerId)
        => await _db.Reviews.Where(r => r.CustomerId == customerId).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Review>> GetByOrderIdAsync(Guid orderId)
        => await _db.Reviews.Where(r => r.OrderId == orderId).AsNoTracking().ToListAsync();

    public Task<Review?> GetReviewWithDimensionsAsync(Guid reviewId)
        => _db.Reviews.Include(r => r.RatingDimensions).AsNoTracking().FirstOrDefaultAsync(r => r.ReviewId == reviewId);

    public async Task<double> GetAverageRatingAsync(Guid tailorId)
        => await _db.Reviews.Where(r => r.TailorId == tailorId).AverageAsync(r => (double)r.Rating);

    public async Task<int> GetReviewCountAsync(Guid tailorId)
        => await _db.Reviews.CountAsync(r => r.TailorId == tailorId);

    public async Task<IEnumerable<Review>> GetRecentReviewsAsync(Guid tailorId, int count)
        => await _db.Reviews.Where(r => r.TailorId == tailorId).OrderByDescending(r => r.CreatedAt).Take(count).AsNoTracking().ToListAsync();

    public async Task<bool> HasCustomerReviewedOrderAsync(Guid customerId, Guid orderId)
        => await _db.Reviews.AnyAsync(r => r.CustomerId == customerId && r.OrderId == orderId);
}
