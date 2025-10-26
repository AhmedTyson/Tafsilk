using TafsilkPlatform.Core.Models;

namespace TafsilkPlatform.Core.Interfaces
{
    public interface ITailorRepository : IRepository<TailorProfile>
    {
        Task<IEnumerable<TailorProfile>> GetByLocationAsync(decimal latitude, decimal longitude, double radiusKm);
        Task<IEnumerable<TailorProfile>> GetByServiceTypeAsync(string serviceType);
        Task<IEnumerable<TailorProfile>> GetAvailableTailorsAsync(DateTime date);
        Task<IEnumerable<TailorProfile>> GetVerifiedTailorsAsync();
        Task<IEnumerable<TailorProfile>> SearchTailorsAsync(string searchTerm, string? city = null);
        Task<TailorProfile?> GetTailorWithServicesAsync(Guid tailorId);
        Task<TailorProfile?> GetTailorWithPortfolioAsync(Guid tailorId);
        Task<TailorProfile?> GetTailorWithReviewsAsync(Guid tailorId);
        Task UpdateTailorRatingAsync(Guid tailorId);
        Task<IEnumerable<TailorProfile>> GetTopRatedTailorsAsync(int count);
    }
}
