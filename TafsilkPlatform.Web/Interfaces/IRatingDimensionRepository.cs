using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Interfaces
{
    public interface IRatingDimensionRepository : IRepository<RatingDimension>
    {
        Task<IEnumerable<RatingDimension>> GetByReviewIdAsync(Guid reviewId);
        Task<IEnumerable<RatingDimension>> GetByTailorIdAsync(Guid tailorId);
        Task<double> GetAverageDimensionScoreAsync(Guid tailorId, string dimensionName);
    }
}
