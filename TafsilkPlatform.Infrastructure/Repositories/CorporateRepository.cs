using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Core.Interfaces;
using TafsilkPlatform.Core.Models;
using TafsilkPlatform.Infrastructure.Persistence;

namespace TafsilkPlatform.Infrastructure.Repositories;

public class CorporateRepository : EfRepository<CorporateAccount>, ICorporateRepository
{
    public CorporateRepository(AppDbContext db) : base(db) { }

    public Task<CorporateAccount?> GetByUserIdAsync(Guid userId)
        => _db.CorporateAccounts.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId);

    public Task<CorporateAccount?> GetCorporateWithRFQsAsync(Guid corporateId)
        => _db.CorporateAccounts.Include(c => c.User).AsNoTracking().FirstOrDefaultAsync(c => c.Id == corporateId);

    public async Task<IEnumerable<CorporateAccount>> GetApprovedCorporatesAsync()
        => await _db.CorporateAccounts.Where(c => c.IsApproved).AsNoTracking().ToListAsync();

    public async Task<bool> ApproveCorporateAsync(Guid corporateId)
    {
        var entity = await _db.CorporateAccounts.FindAsync(corporateId);
        if (entity is null) return false;
        entity.IsApproved = true;
        _db.CorporateAccounts.Update(entity);
        return true;
    }

    public async Task<bool> RejectCorporateAsync(Guid corporateId)
    {
        var entity = await _db.CorporateAccounts.FindAsync(corporateId);
        if (entity is null) return false;
        entity.IsApproved = false;
        _db.CorporateAccounts.Update(entity);
        return true;
    }
}
