using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Data;

namespace TafsilkPlatform.Web.Repositories;

public class RFQRepository : EfRepository<RFQ>, IRFQRepository
{
    public RFQRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<RFQ>> GetByCorporateIdAsync(Guid corporateId)
        => await _db.RFQs.Where(r => r.CorporateAccountId == corporateId).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<RFQ>> GetOpenRFQsAsync()
        => await _db.RFQs.Where(r => r.Status == "Open").AsNoTracking().ToListAsync();

    public async Task<IEnumerable<RFQ>> GetRFQsByStatusAsync(string status)
        => await _db.RFQs.Where(r => r.Status == status).AsNoTracking().ToListAsync();

    public Task<RFQ?> GetRFQWithBidsAsync(Guid rfqId)
        => _db.RFQs.Include(r => r.Bids).AsNoTracking().FirstOrDefaultAsync(r => r.Id == rfqId);

    public async Task<bool> CloseRFQAsync(Guid rfqId)
    {
        var entity = await _db.RFQs.FindAsync(rfqId);
        if (entity is null) return false;
        entity.Status = "Closed";
        _db.RFQs.Update(entity);
        return true;
    }

    public async Task<bool> ExtendDeadlineAsync(Guid rfqId, DateTime newDeadline)
    {
        var entity = await _db.RFQs.FindAsync(rfqId);
        if (entity is null) return false;
        entity.Deadline = newDeadline;
        _db.RFQs.Update(entity);
        return true;
    }
}
