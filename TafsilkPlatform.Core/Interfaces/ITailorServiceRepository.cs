using TafsilkPlatform.Core.Models;

namespace TafsilkPlatform.Core.Interfaces
{
    public interface ITailorServiceRepository : IRepository<TailorService>
    {
        Task<IEnumerable<TailorService>> GetByTailorIdAsync(Guid tailorId);
        Task<IEnumerable<TailorService>> GetByServiceNameAsync(string serviceName);
        Task<TailorService?> GetServiceWithPricingAsync(Guid serviceId);
        Task<bool> UpdateServicePricingAsync(Guid serviceId, decimal basePrice);
        Task<IEnumerable<TailorService>> GetPopularServicesAsync(int count);
    }
}
