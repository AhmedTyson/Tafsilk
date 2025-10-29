using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetByTailorIdAsync(Guid tailorId);
        Task<IEnumerable<Review>> GetByCustomerIdAsync(Guid customerId);
        Task<IEnumerable<Review>> GetByOrderIdAsync(Guid orderId);
        Task<Review?> GetReviewWithDimensionsAsync(Guid reviewId);
        Task<double> GetAverageRatingAsync(Guid tailorId);
        Task<int> GetReviewCountAsync(Guid tailorId);
        Task<IEnumerable<Review>> GetRecentReviewsAsync(Guid tailorId, int count);
        Task<bool> HasCustomerReviewedOrderAsync(Guid customerId, Guid orderId);
    }
}
