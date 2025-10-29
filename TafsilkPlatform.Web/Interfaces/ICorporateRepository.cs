using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Interfaces
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
