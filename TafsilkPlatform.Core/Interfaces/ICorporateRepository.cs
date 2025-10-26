using TafsilkPlatform.Core.Models;

namespace TafsilkPlatform.Core.Interfaces
{
    public interface ICorporateRepository : IRepository<CorporateAccount>
    {
        Task<CorporateAccount?> GetByUserIdAsync(Guid userId);
        Task<CorporateAccount?> GetCorporateWithRFQsAsync(Guid corporateId);
        Task<IEnumerable<CorporateAccount>> GetApprovedCorporatesAsync();
        Task<bool> ApproveCorporateAsync(Guid corporateId);
        Task<bool> RejectCorporateAsync(Guid corporateId);
    }
}
