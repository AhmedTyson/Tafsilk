using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Interfaces
{
    public interface IPortfolioRepository : IRepository<PortfolioImage>
    {
        Task<IEnumerable<PortfolioImage>> GetByTailorIdAsync(Guid tailorId);
        Task<IEnumerable<PortfolioImage>> GetBeforeAfterImagesAsync(Guid tailorId);
        Task<bool> AddPortfolioImagesAsync(Guid tailorId, IEnumerable<string> imageUrls, bool isBeforeAfter);
        Task<bool> RemovePortfolioImageAsync(Guid imageId);
        Task<int> GetPortfolioCountAsync(Guid tailorId);
    }
}
