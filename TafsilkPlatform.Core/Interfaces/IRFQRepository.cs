using TafsilkPlatform.Core.Models;

namespace TafsilkPlatform.Core.Interfaces
{
    public interface IRFQRepository : IRepository<RFQ>
    {
        Task<IEnumerable<RFQ>> GetByCorporateIdAsync(Guid corporateId);
        Task<IEnumerable<RFQ>> GetOpenRFQsAsync();
        Task<IEnumerable<RFQ>> GetRFQsByStatusAsync(string status);
        Task<RFQ?> GetRFQWithBidsAsync(Guid rfqId);
        Task<bool> CloseRFQAsync(Guid rfqId);
        Task<bool> ExtendDeadlineAsync(Guid rfqId, DateTime newDeadline);
    }
}
