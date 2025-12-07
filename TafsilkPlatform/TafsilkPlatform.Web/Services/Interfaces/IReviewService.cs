using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.Web.Services.Interfaces
{
    public interface IReviewService
    {
        Task<bool> CanUserReviewProductAsync(Guid userId, Guid productId);
        Task AddReviewAsync(Review review);
        Task<List<Review>> GetProductReviewsAsync(Guid productId, int page = 1, int pageSize = 10);
        Task<List<Review>> GetTailorReviewsAsync(Guid tailorId, int page = 1, int pageSize = 10);
    }
}
