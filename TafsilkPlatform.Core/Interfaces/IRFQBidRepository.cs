using TafsilkPlatform.Core.Models;

namespace TafsilkPlatform.Core.Interfaces
{
    public interface IRFQBidRepository : IRepository<RFQBid>
    {
        Task<IEnumerable<RFQBid>> GetByRFQIdAsync(Guid rfqId);
        Task<IEnumerable<RFQBid>> GetByTailorIdAsync(Guid tailorId);
        Task<RFQBid?> GetWinningBidAsync(Guid rfqId);
        Task<bool> AcceptBidAsync(Guid bidId);
        Task<bool> RejectBidAsync(Guid bidId);
        Task<int> GetBidCountAsync(Guid rfqId);
    }
}
