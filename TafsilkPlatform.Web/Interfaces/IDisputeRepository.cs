using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Interfaces
{
    public interface IDisputeRepository : IRepository<Dispute>
    {
        Task<IEnumerable<Dispute>> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<Dispute>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Dispute>> GetOpenDisputesAsync();
        Task<IEnumerable<Dispute>> GetDisputesByStatusAsync(string status);
        Task<Dispute?> GetDisputeWithOrderAsync(Guid disputeId);
        Task<bool> ResolveDisputeAsync(Guid disputeId, Guid adminId, string resolutionDetails);
        Task<int> GetDisputeCountByStatusAsync(string status);
    }
}
