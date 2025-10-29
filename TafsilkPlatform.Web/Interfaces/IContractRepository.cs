using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Interfaces
{
    public interface IContractRepository : IRepository<Contract>
    {
        Task<IEnumerable<Contract>> GetByTailorIdAsync(Guid tailorId);
        Task<IEnumerable<Contract>> GetByCorporateIdAsync(Guid corporateId);
        Task<IEnumerable<Contract>> GetActiveContractsAsync();
        Task<IEnumerable<Contract>> GetExpiringContractsAsync(DateTime threshold);
        Task<Contract?> GetContractWithRFQAsync(Guid contractId);
        Task<bool> UpdateContractStatusAsync(Guid contractId, string status);
    }
}
