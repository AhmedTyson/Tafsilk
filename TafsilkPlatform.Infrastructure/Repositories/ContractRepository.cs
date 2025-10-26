using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Core.Interfaces;
using TafsilkPlatform.Core.Models;
using TafsilkPlatform.Infrastructure.Persistence;

namespace TafsilkPlatform.Infrastructure.Repositories;

public class ContractRepository : EfRepository<Contract>, IContractRepository
{
    public ContractRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Contract>> GetByTailorIdAsync(Guid tailorId)
        => await _db.Contracts.Where(c => c.TailorId == tailorId).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Contract>> GetByCorporateIdAsync(Guid corporateId)
    {
        var query = from c in _db.Contracts
                    join r in _db.RFQs on c.RFQId equals r.Id
                    where r.CorporateAccountId == corporateId
                    select c;
        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Contract>> GetActiveContractsAsync()
        => await _db.Contracts.Where(c => c.ContractStatus == "Active").AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Contract>> GetExpiringContractsAsync(DateTime threshold)
        => await _db.Contracts.Where(c => c.EndDate <= threshold && c.ContractStatus == "Active").AsNoTracking().ToListAsync();

    public Task<Contract?> GetContractWithRFQAsync(Guid contractId)
        => _db.Contracts.Include(c => c.RFQ).AsNoTracking().FirstOrDefaultAsync(c => c.Id == contractId);

    public async Task<bool> UpdateContractStatusAsync(Guid contractId, string status)
    {
        var entity = await _db.Contracts.FindAsync(contractId);
        if (entity is null) return false;
        entity.ContractStatus = status;
        _db.Contracts.Update(entity);
        return true;
    }
}
